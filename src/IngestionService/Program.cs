using Confluent.Kafka;
using System;
using System.Threading;

class Program
{
    static void Main()
    {
        var config = new ProducerConfig { BootstrapServers = "kafka:9092" };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        int id = 0;
        while (true)
        {
            var message = $"{{\"sensorId\": {id++}, \"energyKwh\": {new Random().Next(1, 100)}, \"timestamp\": \"{DateTime.UtcNow:o}\"}}";
            try
            {
                producer.Produce("energy-events", new Message<Null, string> { Value = message });
                Console.WriteLine($"Produced: {message}");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }

            Thread.Sleep(5000); // Her 5 saniyede bir
        }
    }
}