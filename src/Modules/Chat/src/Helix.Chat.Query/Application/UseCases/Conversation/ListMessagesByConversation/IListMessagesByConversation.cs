using MediatR;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;
public interface IListMessagesByConversation
    : IRequestHandler<ListMessagesByConversationInput, ListMessagesByConversationOutput>
{ }
