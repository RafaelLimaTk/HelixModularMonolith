namespace Helix.Chat.Infra.Data.EF.Models;
public sealed class ConversationsParticipants
{
    public ConversationsParticipants(
        Guid conversationId,
        Guid userId,
        DateTime joinedAt)
    {
        ConversationId = conversationId;
        UserId = userId;
        JoinedAt = joinedAt;
    }

    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public Conversation? Conversation { get; set; }
}
