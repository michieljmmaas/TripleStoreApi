using Scalar.AspNetCore;
using Serilog;
using TripleStoreApi.Endpoints;
using TripleStoreApi.Services;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Register services
builder.Services.AddHttpClient<ISparqlService, SparqlService>(client =>
{
    client.DefaultRequestVersion = System.Net.HttpVersion.Version11;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Map routes
app.MapGraphEndpoints();

app.Run();
