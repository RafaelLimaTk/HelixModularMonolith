using MongoDB.Bson;

namespace Helix.Chat.Query.Data.Configurations;

public sealed class MessageConfiguration : BaseMongoConfiguration<MessageQueryModel>
{
    public override string CollectionName => CollectionNames.Messages;

    public override void ConfigureIndexes(IMongoCollection<BsonDocument> col)
    {
        var byConvSent = Builders<BsonDocument>.IndexKeys
            .Ascending("conversation_id").Ascending("sent_at");
        col.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(byConvSent));
    }
}
