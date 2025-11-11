using Helix.Chat.Query.Application.Common;
using MediatR;

namespace Helix.Chat.Query.Application.UseCases.ListUserConversations;
public sealed class ListUserConversationsInput(
    Guid userId,
    int page = 1,
    int perPage = 20,
    string search = "",
    string sort = "",
    SearchOrder dir = SearchOrder.Asc)
        : PaginatedListInput(page, perPage, search, sort, dir),
            IRequest<ListUserConversationsOutput>
{
    public Guid UserId { get; init; } = userId;
}
