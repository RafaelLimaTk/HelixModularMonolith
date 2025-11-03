using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Models;
using Shared.Application.Projections;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Projections;
public sealed class MessageDeliveredProjection(ISynchronizeDb sync) : IProjectionHandler<MessageDelivered>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageDelivered @event, CancellationToken cancellationToken)
        => _sync.UpsertAsync(
            new MessageQueryModel
            {
                Id = @event.MessageId,
                DeliveredAt = @event.DeliveredAt,
                Status = "Delivered"
            },
            m => m.Id == @event.MessageId,
            cancellationToken
        );
}