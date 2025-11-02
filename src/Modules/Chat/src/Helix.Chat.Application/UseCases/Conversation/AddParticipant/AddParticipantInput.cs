using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.AddParticipant;
public sealed record AddParticipantInput(
    Guid ConversationId,
    Guid UserId
) : IRequest<AddParticipantOutput>;
