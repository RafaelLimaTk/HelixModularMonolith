using MongoDB.Bson;

namespace Helix.Chat.Query.Data.Configurations;

public sealed class ConversationConfiguration : BaseMongoConfiguration<ConversationQueryModel>
{
    public override string CollectionName => CollectionNames.Conversations;

    public override void ConfigureIndexes(IMongoCollection<BsonDocument> col)
    {
        var byParticipantUpdated = Builders<BsonDocument>.IndexKeys
            .Ascending("participant_ids").Descending("updated_at");
        var byUpdatedOnly = Builders<BsonDocument>.IndexKeys
            .Descending("updated_at");
        col.Indexes.CreateMany([
            new CreateIndexModel<BsonDocument>(byParticipantUpdated),
            new CreateIndexModel<BsonDocument>(byUpdatedOnly)
        ]);
    }
}