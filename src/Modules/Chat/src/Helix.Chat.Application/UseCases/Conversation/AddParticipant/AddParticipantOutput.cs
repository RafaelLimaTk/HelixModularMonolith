namespace Helix.Chat.Application.UseCases.Conversation.AddParticipant;
public record AddParticipantOutput(
    Guid ConversationId,
    Guid UserId,
    DateTime JoinedAt,
    bool Added
);
