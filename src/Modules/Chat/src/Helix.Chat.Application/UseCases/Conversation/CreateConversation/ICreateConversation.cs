using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.CreateConversation;
public interface ICreateConversation
    : IRequestHandler<CreateConversationInput, CreateConversationOutput>
{ }
