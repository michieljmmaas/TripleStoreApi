using TripleStoreApi.Models;

namespace TripleStoreApi.Services;

public interface ISparqlService
{
    Task<IEnumerable<TripleResult>> QueryTriplesAsync(int limit = 10);
}