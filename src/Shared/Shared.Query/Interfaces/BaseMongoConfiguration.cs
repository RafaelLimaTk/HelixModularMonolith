using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Shared.Query.Interfaces;

public abstract class BaseMongoConfiguration<TModel> : IReadDbConfiguration<TModel>
    where TModel : IQueryModel
{
    public abstract string CollectionName { get; }
    public Type ModelType => typeof(TModel);

    public virtual void ConfigureClassMap()
    {
        if (BsonClassMap.IsClassMapRegistered(ModelType)) return;

        BsonClassMap.RegisterClassMap<TModel>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            var idProperty = typeof(TModel).GetProperty("Id");
            if (idProperty != null && idProperty.CanRead)
                cm.MapIdProperty("Id");
        });
    }

    public abstract void ConfigureIndexes(IMongoCollection<BsonDocument> col);

    protected CreateIndexModel<BsonDocument> CreateIndex(
        IndexKeysDefinition<BsonDocument> keys,
        CreateIndexOptions? options = null)
        => new(keys, options);
}