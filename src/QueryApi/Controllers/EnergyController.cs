using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Mvc;

namespace QueryApi.Controllers;

[ApiController]
[Route("api/energy")]
public class EnergyController : ControllerBase
{
    private readonly ElasticsearchClient _client;

    public EnergyController()
    {
        var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
            .DefaultIndex("energy-index");

        _client = new ElasticsearchClient(settings);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var response = await _client.SearchAsync<EnergyDoc>(s => s
            .Index("energy-index")
            .Size(10_000)
            .Query(q => q.Bool(b => b.Filter(BuildDateRangeFilter(from, to))))
        );

        if (!response.IsValidResponse || response.Documents is null)
        {
            return StatusCode(500, response.DebugInformation);
        }

        var docs = response.Documents;

        var total = docs.Sum(x => x.EnergyKwh);
        var count = docs.Count;
        var avg = count > 0 ? total / count : 0;

        return Ok(new
        {
            AverageEnergyKwh = Math.Round(avg, 2),
            TotalEnergyKwh = Math.Round(total, 2),
            DocumentCount = count,
            TimeRange = new { From = from, To = to }
        });
    }

    private static List<Query> BuildDateRangeFilter(DateTime? from, DateTime? to)
    {
        var filters = new List<Query>();

        if (from.HasValue || to.HasValue)
        {
            filters.Add(new DateRangeQuery("timestamp")
            {
                Gte = from,
                Lte = to
            });
        }
        else
        {
            filters.Add(new MatchAllQuery());
        }

        return filters;
    }
}

public class EnergyDoc
{
    public DateTime Timestamp { get; set; }
    public double EnergyKwh { get; set; }
}
