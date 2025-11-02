using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Events.Conversation;
public sealed class MessageDelivered : DomainEvent
{
    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public DateTime DeliveredAt { get; }

    public MessageDelivered(Guid messageId, Guid conversationId, DateTime deliveredAt)
        : base()
    {
        MessageId = messageId;
        ConversationId = conversationId;
        DeliveredAt = deliveredAt;
    }
}
