using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Models;
using Shared.Application.Projections;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Projections;
public sealed class MessageSentProjection(ISynchronizeDb sync) : IProjectionHandler<MessageSent>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageSent @event, CancellationToken cancellationToken)
        => _sync.UpsertAsync(
            new MessageQueryModel
            {
                Id = @event.MessageId,
                ConversationId = @event.ConversationId,
                SenderId = @event.SenderId,
                Content = @event.Content,
                SentAt = @event.SentAt,
                Status = "Sent"
            },
            m => m.Id == @event.MessageId,
            cancellationToken
        );
}