using Helix.Chat.Query.Data.Conventions;
using Mongo2Go;
using MongoDB.Bson;

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
        AutoRegisterMongoConfigurations();
    }

    public IChatReadDbContext CreateReadDbContext(bool preserveData = false)
    {
        var context = new ChatReadDbContext(Database);
        if (!preserveData)
            context.EnsureDeleted();
        return context;
    }

    private void AutoRegisterMongoConfigurations()
    {
        var configurations = MongoConfigurationLocator.FindAllConfigurations();

        foreach (var cfg in configurations)
        {
            cfg.ConfigureClassMap();

            if (cfg.CollectionName != null)
            {
                var bsonCollection = Database.GetCollection<BsonDocument>(cfg.CollectionName);
                cfg.ConfigureIndexes(bsonCollection);
            }
        }
    }

    private sealed class ChatReadDbContext(IMongoDatabase database) : IChatReadDbContext
    {
        private readonly IMongoDatabase _database = database;

        private readonly Dictionary<Type, string> _collectionMappings
            = MongoConfigurationLocator.GetCollectionMappings();

        public IMongoCollection<T> GetCollection<T>(string? name)
            => _database.GetCollection<T>(name);

        public void EnsureDeleted()
        {
            foreach (var collectionName in _collectionMappings.Values.Distinct())
            {
                var collection = _database.GetCollection<BsonDocument>(collectionName);
                collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            }
        }
    }

    public void Dispose()
        => _runner.Dispose();
}
