# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy global.json first
COPY ["global.json", "./"]

# Copy project files and restore dependencies
COPY ["TripleStoreApi/TripleStoreApi.csproj", "TripleStoreApi/"]
RUN dotnet restore "TripleStoreApi/TripleStoreApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TripleStoreApi"
RUN dotnet build "TripleStoreApi.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "TripleStoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TripleStoreApi.dll"]
