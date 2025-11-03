using Helix.Chat.Domain.Events.Conversation;

namespace Helix.Chat.Query.Projections.Conversation;

public sealed class ParticipantAddedProjection(ISynchronizeDb sync)
    : IProjectionHandler<ParticipantAdded>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(ParticipantAdded participantAdded, CancellationToken cancellationToken)
    {
        var filter = Builders<ConversationQueryModel>.Filter.Eq(x => x.Id, participantAdded.ConversationId);
        var updateBuilder = Builders<ConversationQueryModel>.Update;
        var updateDefinition = updateBuilder
            .AddToSet(x => x.ParticipantIds, participantAdded.UserId)
            .Set(x => x.UpdatedAt, participantAdded.JoinedAt);

        return _sync.UpdateAsync(filter, updateDefinition, cancellationToken, upsert: false);
    }
}