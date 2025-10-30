using Helix.Chat.Domain.Enums;
using Shared.Domain.Exceptions;
using Shared.Domain.SeedWorks;
using Shared.Domain.Validations;

namespace Helix.Chat.Domain.Entities;
public sealed class Message : Entity
{
    public const int MAX_LENGTH = 10000;

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }
    public MessageStatus Status { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public Message(Guid conversationId, Guid senderId, string content)
    {
        if (conversationId == Guid.Empty) throw new EntityValidationException("ConversationId should not be null");
        if (senderId == Guid.Empty) throw new EntityValidationException("SenderId should not be null");

        ConversationId = conversationId;
        SenderId = senderId;

        Content = (content ?? string.Empty).Trim();
        Validate();

        SentAt = DateTime.UtcNow;
        Status = MessageStatus.Sent;
    }

    public bool MarkAsDelivered()
    {
        if (Status == MessageStatus.Read) return false;
        if (Status == MessageStatus.Delivered) return false;

        Status = MessageStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        return true;
    }

    public bool MarkAsRead()
    {
        if (Status == MessageStatus.Read) return false;
        if (Status == MessageStatus.Sent)
            throw new EntityValidationException("Cannot mark as read from Sent state");

        Status = MessageStatus.Read;
        ReadAt = DateTime.UtcNow;
        return true;
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Content, nameof(Content));
        DomainValidation.MaxLength(Content, MAX_LENGTH, nameof(Content));
    }
}
