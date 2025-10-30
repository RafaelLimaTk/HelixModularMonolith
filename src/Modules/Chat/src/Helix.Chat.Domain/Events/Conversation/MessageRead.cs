using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Events.Conversation;
public sealed class MessageRead : DomainEvent
{
    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public Guid ReaderId { get; }
    public DateTime ReadAt { get; }

    public MessageRead(
        Guid messageId,
        Guid conversationId,
        Guid readerId,
        DateTime readAt)
        : base()
    {
        MessageId = messageId;
        ConversationId = conversationId;
        ReaderId = readerId;
        ReadAt = readAt;
    }
}
