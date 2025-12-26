using Helix.Chat.Application.UseCases.Conversation.AddParticipant;

namespace Helix.Chat.Endpoints.ApiModels.Conversation;

public sealed record AddParticipantApiInput(
    Guid UserId
)
{
    public AddParticipantInput ToInput(Guid conversationId)
        => new(conversationId, UserId);
}
