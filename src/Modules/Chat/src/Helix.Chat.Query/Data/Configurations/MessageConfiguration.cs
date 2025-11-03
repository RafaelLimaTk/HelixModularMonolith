using Helix.Chat.Query.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Data.Configurations;

public sealed class MessageConfiguration : IReadDbConfiguration
{
    public string CollectionName => "messages";
    public Type ModelType => typeof(MessageQueryModel);

    public void ConfigureClassMap()
    {
        if (BsonClassMap.IsClassMapRegistered(ModelType)) return;
        BsonClassMap.RegisterClassMap<MessageQueryModel>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapIdMember(x => x.Id);
        });
    }

    public void ConfigureIndexes(IMongoCollection<BsonDocument> col)
    {
        var byConvSent = Builders<BsonDocument>.IndexKeys
            .Ascending("conversation_id").Ascending("sent_at");
        col.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(byConvSent));
    }
}
