using TripleStoreApi.Models;
using VDS.RDF.Query;
using Serilog;

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
        try
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
        catch (HttpRequestException ex)
        {
            Log.Warning(ex, "Could not connect to the SPARQL endpoint or dataset was not found.");
            return Enumerable.Empty<TripleResult>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while querying triples.");
            return Enumerable.Empty<TripleResult>();
        }
    }

    public async Task<IEnumerable<QueryResult>> ExecuteQueryAsync(string sparql)
    {
        try
        {
            var results = await _client.QueryWithResultSetAsync(sparql);

            return results.Select(r => new QueryResult
            {
                Bindings = r.Variables
                    .Where(v => r.HasValue(v))
                    .ToDictionary(v => v, v => r[v].ToString() ?? string.Empty)
            });
        }
        catch (HttpRequestException ex)
        {
            Log.Warning(ex, "Could not connect to the SPARQL endpoint or dataset was not found.");
            return Enumerable.Empty<QueryResult>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while executing SPARQL query.");
            return Enumerable.Empty<QueryResult>();
        }
    }
}