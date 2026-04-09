using TripleStoreApi.Services;

namespace TripleStoreApi.Endpoints;

public static class GraphEndpoints
{
    public static void MapGraphEndpoints(this WebApplication app)
    {
        app.MapGet("/triples", async (ISparqlService sparqlService, int limit = 10) =>
            {
                var results = await sparqlService.QueryTriplesAsync(limit);
                return Results.Ok(results);
            })
            .WithName("GetTriples")
            .WithOpenApi();

        app.MapGet("/triples/query", async (ISparqlService sparqlService, string sparql) =>
            {
                // For raw SPARQL queries passed as a query string parameter
                var results = await sparqlService.QueryTriplesAsync(10);
                return Results.Ok(results);
            })
            .WithName("QueryTriples")
            .WithOpenApi();
    }
}