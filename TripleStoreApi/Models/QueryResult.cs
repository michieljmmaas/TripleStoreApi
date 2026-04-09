namespace TripleStoreApi.Models;

public class QueryResult
{
    public Dictionary<string, string> Bindings { get; set; } = new();
}