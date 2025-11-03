using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Models;
using Shared.Application.Projections;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Projections.Message;

public sealed class MessageReadProjection(ISynchronizeDb sync) : IProjectionHandler<MessageRead>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageRead e, CancellationToken cancellationToken)
        => _sync.UpsertAsync(
            new MessageQueryModel
            {
                Id = e.MessageId,
                ConversationId = e.ConversationId,
                ReadAt = e.ReadAt,
                Status = "Read"
            },
            m => m.Id == e.MessageId,
            cancellationToken
        );
}