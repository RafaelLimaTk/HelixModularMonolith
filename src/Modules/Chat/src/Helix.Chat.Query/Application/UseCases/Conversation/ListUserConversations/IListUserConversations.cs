using MediatR;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;
public interface IListUserConversations
    : IRequestHandler<ListUserConversationsInput, ListUserConversationsOutput>
{ }
