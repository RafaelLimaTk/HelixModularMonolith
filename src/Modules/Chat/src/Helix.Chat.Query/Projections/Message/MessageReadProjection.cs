using Helix.Chat.Domain.Events.Message;

namespace Helix.Chat.Query.Projections.Message;

public sealed class MessageReadProjection(ISynchronizeDb sync) : IProjectionHandler<MessageRead>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageRead messageRead, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageQueryModel>.Filter.Eq(x => x.Id, messageRead.MessageId) &
                     Builders<MessageQueryModel>.Filter.Ne(x => x.Status, MessageStatus.Read);

        var updateDefinition = Builders<MessageQueryModel>.Update
            .Set(x => x.Status, MessageStatus.Read)
            .Max(x => x.ReadAt, messageRead.ReadAt);

        return _sync.UpdateAsync(filter, updateDefinition, cancellationToken);
    }
}