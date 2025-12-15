using Helix.Chat.Query.Data.Conventions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Helix.Chat.Query.Data.Context;

public sealed class NoSqlDbContext : IChatReadDbContext
{
    private readonly IMongoDatabase _db;
    private readonly Dictionary<Type, string> _collectionMappings;

    public NoSqlDbContext(IOptions<MongoSettings> opt)
    {
        MongoConventions.Apply();
        var client = new MongoClient(opt.Value.ConnectionString);
        _db = client.GetDatabase(opt.Value.Database);

        var configurations = MongoConfigurationLocator.FindAllConfigurations();
        _collectionMappings = configurations.ToDictionary(c => c.ModelType, c => c.CollectionName);

        foreach (var cfg in configurations)
        {
            cfg.ConfigureClassMap();

            if (cfg.CollectionName != null)
            {
                var raw = _db.GetCollection<BsonDocument>(cfg.CollectionName);
                cfg.ConfigureIndexes(raw);
            }
        }
    }

    public IMongoCollection<TModel> GetCollection<TModel>(string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
            return _db.GetCollection<TModel>(name);

        var type = typeof(TModel);
        return _collectionMappings.TryGetValue(type, out var collectionName)
            ? _db.GetCollection<TModel>(collectionName)
            : _db.GetCollection<TModel>(type.Name.ToSnakeCase());
    }
}
