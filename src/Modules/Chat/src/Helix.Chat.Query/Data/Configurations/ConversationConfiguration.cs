using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Helix.Chat.Query.Data.Configurations;
public sealed class ConversationConfiguration : IReadDbConfiguration
{
    public string CollectionName => "conversations";
    public Type ModelType => typeof(ConversationQueryModel);

    public void ConfigureClassMap()
    {
        if (BsonClassMap.IsClassMapRegistered(ModelType)) return;

        BsonClassMap.RegisterClassMap<ConversationQueryModel>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapIdMember(x => x.Id);
        });
    }

    public void ConfigureIndexes(IMongoCollection<BsonDocument> col)
    {
        var byParticipantUpdated = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys
                .Ascending("participant_ids")
                .Descending("updated_at"));

        var byUpdatedOnly = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys
                .Descending("updated_at"));

        col.Indexes.CreateMany(new[] { byParticipantUpdated, byUpdatedOnly });
    }
}