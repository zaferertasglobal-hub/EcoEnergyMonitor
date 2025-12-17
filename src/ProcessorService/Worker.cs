using Confluent.Kafka;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessorService
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "kafka:9092",
                GroupId = "energy-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe("energy-events");

            var elasticConfig = new ConnectionConfiguration(new Uri("http://elasticsearch:9200"));
            var elasticClient = new ElasticLowLevelClient(elasticConfig);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult?.Message?.Value != null)
                    {
                        // Orijinal mesajı JSON olarak parse et
                        using var doc = JsonDocument.Parse(consumeResult.Message.Value);
                        var root = doc.RootElement;

                        var sensorId = root.GetProperty("sensorId").GetInt32();
                        var energyKwh = root.GetProperty("energyKwh").GetInt32();
                        var timestamp = root.GetProperty("timestamp").GetString();

                        // Yeni enriched JSON oluştur
                        var enrichedJson = JsonSerializer.Serialize(new
                        {
                            sensorId,
                            energyKwh,
                            timestamp,
                            processedAt = DateTime.UtcNow.ToString("o")
                        });

                        var response = await elasticClient.IndexAsync<DynamicResponse>("energy-index", PostData.String(enrichedJson));

                        if (response.Success)
                        {
                            Console.WriteLine($"Processed and indexed: {enrichedJson}");
                        }
                        else
                        {
                            Console.WriteLine($"Index failed: {response.DebugInformation}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}