using Helix.Chat.Query.Data.Configurations;
using Helix.Chat.Query.Data.Conventions;
using Mongo2Go;
using MongoDB.Bson;
using Shared.Query.Interfaces;

namespace Helix.Chat.IntegrationTests.Query.Base;

public abstract class QueryBaseFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    protected IMongoDatabase Database { get; }
    protected Faker Faker { get; }

    protected QueryBaseFixture()
    {
        MongoConventions.Apply();
        _runner = MongoDbRunner.Start();

        var client = new MongoClient(_runner.ConnectionString);
        Database = client.GetDatabase("integration-query-tests-db");

        Faker = new Faker("pt_BR");
        RegisterReadModelMappings();
    }

    public IChatReadDbContext CreateReadDbContext()
        => new ChatReadDbContext(Database);

    public void Dispose()
        => _runner.Dispose();

    private void RegisterReadModelMappings()
    {
        var configurations = new IReadDbConfiguration[]
        {
            new MessageConfiguration(),
            new ConversationConfiguration()
        };

        foreach (var configuration in configurations)
        {
            configuration.ConfigureClassMap();

            var bsonCollection = Database.GetCollection<BsonDocument>(configuration.CollectionName);
            configuration.ConfigureIndexes(bsonCollection);
        }
    }

    private sealed class ChatReadDbContext(IMongoDatabase database) : IChatReadDbContext
    {
        private readonly IMongoDatabase _database = database;

        public IMongoCollection<T> GetCollection<T>(string? name)
            => _database.GetCollection<T>(name);
    }
}
