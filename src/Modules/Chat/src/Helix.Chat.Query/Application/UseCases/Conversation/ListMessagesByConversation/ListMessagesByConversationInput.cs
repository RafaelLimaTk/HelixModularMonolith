using Helix.Chat.Query.Application.Common;
using MediatR;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;
public sealed class ListMessagesByConversationInput
    : PaginatedListInput, IRequest<ListMessagesByConversationOutput>
{
    public Guid ConversationId { get; init; }

    public ListMessagesByConversationInput(
        Guid conversationId,
        int page = 1,
        int perPage = 20,
        string search = "",
        string sort = "",
        SearchOrder dir = SearchOrder.Desc)
        : base(page, perPage, search, sort, dir)
    {
        ConversationId = conversationId;
    }
}
