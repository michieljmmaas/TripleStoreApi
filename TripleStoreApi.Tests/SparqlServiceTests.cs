using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using TripleStoreApi.Services;

namespace TripleStoreApi.Tests;

public class SparqlServiceTests
{
    [Fact]
    public async Task QueryTriplesAsync_ReturnsExpectedResults()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Fuseki:EndpointUrl", "http://localhost:3030/ds/query" }
            })
            .Build();

        // This is the expected SPARQL response in XML format (dotNetRDF SparqlQueryClient uses XML by default if not specified otherwise, or it might be content-negotiated).
        // Let's assume XML for now as it's common.
        var sparqlResponse = @"<?xml version='1.0'?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
  <head>
    <variable name='s'/>
    <variable name='p'/>
    <variable name='o'/>
  </head>
  <results>
    <result>
      <binding name='s'><uri>http://example.org/subject</uri></binding>
      <binding name='p'><uri>http://example.org/predicate</uri></binding>
      <binding name='o'><literal>object</literal></binding>
    </result>
  </results>
</sparql>";

        var handlerMock = new TestHttpMessageHandler((request, cancellationToken) => 
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(sparqlResponse, Encoding.UTF8, "application/sparql-results+xml"),
            }));

        var httpClient = new HttpClient(handlerMock);
        var service = new SparqlService(configuration, httpClient);

        // Act
        var result = await service.QueryTriplesAsync(1);

        // Assert
        Assert.NotNull(result);
        var triple = Assert.Single(result);
        Assert.Equal("http://example.org/subject", triple.Subject);
        Assert.Equal("http://example.org/predicate", triple.Predicate);
        Assert.StartsWith("object", triple.Object);

        Assert.Equal(1, handlerMock.CallCount);
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;
        public int CallCount { get; private set; }

        public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return _sendAsync(request, cancellationToken);
        }
    }

    [Fact]
    public void Constructor_ThrowsException_WhenEndpointNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new SparqlService(configuration, httpClient));
    }
}