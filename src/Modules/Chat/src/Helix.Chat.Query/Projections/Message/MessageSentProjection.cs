using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Models;
using MongoDB.Driver;
using Shared.Application.Projections;
using Shared.Query.Extensions;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Projections.Message;
public sealed class MessageSentProjection(ISynchronizeDb sync) : IProjectionHandler<MessageSent>
{
    private readonly ISynchronizeDb _sync = sync;

    public Task ProjectAsync(MessageSent messageSent, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageQueryModel>.Filter.Eq(x => x.Id, messageSent.MessageId);
        var updateBuilder = Builders<MessageQueryModel>.Update;

        var updateDefinition = updateBuilder.UpsertWithId(messageSent.MessageId,
            updateBuilder.SetOnInsert(x => x.ConversationId, messageSent.ConversationId),
            updateBuilder.SetOnInsert(x => x.SenderId, messageSent.SenderId),
            updateBuilder.Set(x => x.Content, messageSent.Content),
            updateBuilder.Set(x => x.SentAt, messageSent.SentAt),
            updateBuilder.Set(x => x.Status, MessageStatus.Sent));

        return _sync.UpdateAsync(filter, updateDefinition, cancellationToken, upsert: true);
    }
}