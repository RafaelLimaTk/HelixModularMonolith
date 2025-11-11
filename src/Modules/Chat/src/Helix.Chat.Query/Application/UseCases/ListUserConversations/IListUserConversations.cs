using MediatR;

namespace Helix.Chat.Query.Application.UseCases.ListUserConversations;
public interface IListUserConversations
    : IRequestHandler<ListUserConversationsInput, ListUserConversationsOutput>
{ }
