using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.AddParticipant;
public interface IAddParticipant
    : IRequestHandler<AddParticipantInput, AddParticipantOutput>
{ }
