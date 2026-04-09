using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
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

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

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

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(sparqlResponse, Encoding.UTF8, "application/sparql-results+xml"),
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new SparqlService(configuration, httpClient);

        // Act
        var result = await service.QueryTriplesAsync(1);

        // Assert
        Assert.NotNull(result);
        var triple = Assert.Single(result);
        Assert.Equal("http://example.org/subject", triple.Subject);
        Assert.Equal("http://example.org/predicate", triple.Predicate);
        Assert.StartsWith("object", triple.Object);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
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