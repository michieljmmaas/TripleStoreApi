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
            Subject = r.HasValue("s") ? r["s"].ToString() : string.Empty,
            Predicate = r.HasValue("p") ? r["p"].ToString() : string.Empty,
            Object = r.HasValue("o") ? r["o"].ToString() : string.Empty
        });
    }

    public async Task<IEnumerable<QueryResult>> ExecuteQueryAsync(string sparql)
    {
        var results = await _client.QueryWithResultSetAsync(sparql);

        return results.Select(r => new QueryResult
        {
            Bindings = r.Variables
                .Where(v => r.HasValue(v))
                .ToDictionary(v => v, v => r[v].ToString() ?? string.Empty)
        });
    }
}