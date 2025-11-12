using MediatR;

namespace Helix.Chat.Query.Application.UseCases.ListMessagesByConversation;
public interface IListMessagesByConversation
    : IRequestHandler<ListMessagesByConversationInput, ListMessagesByConversationOutput>
{ }
