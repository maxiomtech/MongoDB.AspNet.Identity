using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace MongoDB.AspNet.Identity.Tests;

/// <summary>
/// Shared fixture that manages a MongoDB container for all tests.
/// </summary>
public class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public IMongoDatabase GetDatabase(string name = "TestDb")
    {
        var client = new MongoClient(ConnectionString);
        return client.GetDatabase(name);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for sharing the MongoDB fixture across test classes.
/// </summary>
[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}
