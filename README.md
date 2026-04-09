# TripleStoreApi

A minimal ASP.NET Core Web API that queries an RDF triple store (Apache Jena Fuseki) via SPARQL. It exposes two simple read-only endpoints and ships with OpenAPI + a modern API reference UI (Scalar) in Development.

## What is it?
- Minimal API that forwards SPARQL `SELECT` queries to a Fuseki endpoint and returns JSON results.
- Endpoints:
  - `GET /triples?limit=10` — returns sample triples
  - `GET /triples/query?sparql=...` — executes an arbitrary SPARQL `SELECT` query

## Tools / Stack
- .NET SDK 10.0, ASP.NET Core Minimal APIs
- dotNetRDF (`VDS.RDF.Query`) for SPARQL
- Serilog for structured logging
- Scalar.AspNetCore for API reference UI (Development only)
- Docker Compose for local Apache Jena Fuseki
- CI: GitHub Actions; Pre-commit: Lefthook + CSharpier

## Quickstart
1) Start Fuseki (dataset `/test`):
   ```bash
   cd docker
   docker compose up -d
   ```
   - Admin/UI: http://localhost:3030
   - SPARQL endpoint (default): http://localhost:3030/test/query

2) Run the API from repo root:
   ```bash
   dotnet restore
   dotnet run --project TripleStoreApi
   ```
   - Note the printed base URL, e.g. `Now listening on: http://localhost:5xyz`

3) (Optional) Configure target Fuseki endpoint:
   - `TripleStoreApi/appsettings.json` → `Fuseki:EndpointUrl`
   - or environment variable `Fuseki__EndpointUrl`

## Endpoints & URLs
Assuming the app listens at `http://localhost:5xyz` (see console):
- Data
  - `GET http://localhost:5xyz/triples?limit=10`
  - `GET http://localhost:5xyz/triples/query?sparql=SELECT%20*%20WHERE%20%7B%20%3Fs%20%3Fp%20%3Fo%20%7D%20LIMIT%2010`
- Docs (Development environment only)
  - OpenAPI JSON: `http://localhost:5xyz/openapi/v1.json`
  - Scalar UI: `http://localhost:5xyz/scalar/v1`
