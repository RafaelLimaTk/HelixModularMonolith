using Helix.Chat.Domain.Events.Conversation;

namespace Helix.Chat.Query.Projections.Conversation;
public sealed class ConversationCreatedProjection(ISynchronizeDb sync)
    : IProjectionHandler<ConversationCreated>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(ConversationCreated conversationCreated, CancellationToken cancellationToken)
    {
        var filter = Builders<ConversationQueryModel>.Filter.Eq(x => x.Id, conversationCreated.ConversationId);
        var updateBuilder = Builders<ConversationQueryModel>.Update;
        var updateDefinition = updateBuilder
            .SetOnInsert(x => x.Id, conversationCreated.ConversationId)
            .SetOnInsert(x => x.Title, conversationCreated.Title)
            .SetOnInsert(x => x.CreatedAt, conversationCreated.CreatedAt)
            .SetOnInsert(x => x.ParticipantIds, new List<Guid>())
            .Set(x => x.UpdatedAt, conversationCreated.CreatedAt);

        return _sync.UpdateAsync(filter, updateDefinition, cancellationToken, upsert: true);
    }
}