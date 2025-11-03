using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Models;
using MongoDB.Driver;
using Shared.Application.Projections;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Projections.Message;
public sealed class MessageDeliveredProjection(ISynchronizeDb sync) : IProjectionHandler<MessageDelivered>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageDelivered messageDelivered, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageQueryModel>.Filter.Eq(x => x.Id, messageDelivered.MessageId) &
                     Builders<MessageQueryModel>.Filter.Ne(x => x.Status, MessageStatus.Read);

        var updateDefinition = Builders<MessageQueryModel>.Update
            .Set(x => x.Status, MessageStatus.Delivered)
            .Max(x => x.DeliveredAt, messageDelivered.DeliveredAt);

        return _sync.UpdateAsync(filter, updateDefinition, cancellationToken);
    }
}