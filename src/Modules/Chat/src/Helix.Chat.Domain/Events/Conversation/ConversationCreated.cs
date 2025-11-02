using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Events.Conversation;
public sealed class ConversationCreated : DomainEvent
{
    public Guid ConversationId { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }

    public ConversationCreated(Guid conversationId, string title, DateTime createdAt)
        => (ConversationId, Title, CreatedAt) = (conversationId, title, createdAt);
}