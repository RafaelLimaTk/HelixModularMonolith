using Helix.Chat.Query.Data.Conventions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Helix.Chat.Query.Data.Context;

public sealed class NoSqlDbContext : IChatReadDbContext
{
    private readonly IMongoDatabase _db;

    public NoSqlDbContext(IOptions<MongoSettings> opt, IEnumerable<IReadDbConfiguration> configurations)
    {
        MongoConventions.Apply();
        var client = new MongoClient(opt.Value.ConnectionString);
        _db = client.GetDatabase(opt.Value.Database);

        foreach (var cfg in configurations)
        {
            cfg.ConfigureClassMap();
            var raw = _db.GetCollection<BsonDocument>(cfg.CollectionName);
            cfg.ConfigureIndexes(raw);
        }
    }

    public IMongoCollection<TModel> GetCollection<TModel>(string? name = null)
        => _db.GetCollection<TModel>(name ?? typeof(TModel).Name.ToSnakeCase());
}
