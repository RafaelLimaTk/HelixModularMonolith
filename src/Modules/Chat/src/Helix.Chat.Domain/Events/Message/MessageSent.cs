using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Events.Message;
public sealed class MessageSent : DomainEvent
{
    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public Guid SenderId { get; }
    public string Content { get; }
    public DateTime SentAt { get; }

    public MessageSent(
        Guid messageId,
        Guid conversationId,
        Guid senderId,
        string content,
        DateTime sentAt)
        : base()
    {
        MessageId = messageId;
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        SentAt = sentAt;
    }
}