using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Events.Conversation;
public sealed class ParticipantAdded : DomainEvent
{
    public Guid ConversationId { get; }
    public Guid UserId { get; }
    public DateTime JoinedAt { get; }

    public ParticipantAdded(Guid conversationId, Guid userId, DateTime joinedAt)
        => (ConversationId, UserId, JoinedAt) = (conversationId, userId, joinedAt);
}
