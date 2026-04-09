using TripleStoreApi.Models;
using VDS.RDF.Query;

namespace TripleStoreApi.Services;

public class SparqlService : ISparqlService
{
    private readonly SparqlQueryClient _client;

    public SparqlService(IConfiguration configuration, HttpClient httpClient)
    {
        var endpointUrl = configuration["Fuseki:EndpointUrl"]
                          ?? throw new InvalidOperationException("Fuseki endpoint not configured.");

        _client = new SparqlQueryClient(httpClient, new Uri(endpointUrl));
    }

    public async Task<IEnumerable<TripleResult>> QueryTriplesAsync(int limit = 10)
    {
        var query = $"SELECT ?s ?p ?o WHERE {{ ?s ?p ?o }} LIMIT {limit}";
        var results = await _client.QueryWithResultSetAsync(query);

        return results.Select(r => new TripleResult
        {
            Subject = r["s"].ToString(),
            Predicate = r["p"].ToString(),
            Object = r["o"].ToString()
        });
    }
}