using TripleStoreApi.Services;

namespace TripleStoreApi.Endpoints;

public static class GraphEndpoints
{
    public static void MapGraphEndpoints(this WebApplication app)
    {
        app.MapGet(
                "/triples",
                async (ISparqlService sparqlService, int limit = 10) =>
                {
                    var results = await sparqlService.QueryTriplesAsync(limit);
                    return Results.Ok(results);
                }
            )
            .WithName("GetTriples");

        app.MapGet(
                "/triples/query",
                async (ISparqlService sparqlService, string sparql) =>
                {
                    if (string.IsNullOrWhiteSpace(sparql))
                        return Results.BadRequest("A SPARQL query must be provided.");

                    var results = await sparqlService.ExecuteQueryAsync(sparql);
                    return Results.Ok(results);
                }
            )
            .WithName("QueryTriples");
    }
}
