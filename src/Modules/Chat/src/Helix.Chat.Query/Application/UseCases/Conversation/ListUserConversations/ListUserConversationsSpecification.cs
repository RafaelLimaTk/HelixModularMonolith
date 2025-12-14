using Shared.Query.Specifications;
using System.Linq.Expressions;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;

public sealed class ListUserConversationsSpecification : IQuerySpecification<ConversationQueryModel>
{
    public Expression<Func<ConversationQueryModel, bool>>? Criteria { get; }
    public IReadOnlyList<OrderExpression<ConversationQueryModel>> Orders { get; }
    public int Page { get; }
    public int PerPage { get; }

    public ListUserConversationsSpecification(
        Guid userId,
        int page = 1,
        int perPage = 20,
        string search = "",
        string sort = "",
        SearchOrder dir = SearchOrder.Desc)
    {
        var term = (search ?? string.Empty).Trim().ToLowerInvariant();
        var hasTerm = term.Length > 0;

        Criteria = c =>
            c.ParticipantIds.Contains(userId) &&
            (!hasTerm || c.Title.ToLowerInvariant().Contains(term));

        var orders = MapSortWithTieBreaker(sort, dir);
        Orders = orders;

        Page = page <= 0 ? 1 : page;
        PerPage = perPage <= 0 ? 20 : Math.Min(perPage, 100);
    }

    static List<OrderExpression<ConversationQueryModel>> MapSortWithTieBreaker(
        string sort,
        SearchOrder dir)
    {
        var normalizedSort = string.IsNullOrWhiteSpace(sort) ? "updatedAt" : sort.Trim().ToLowerInvariant();
        var desc = dir == SearchOrder.Desc;

        return normalizedSort switch
        {
            "title" =>
            [
                new(x => x.Title, desc),
                new(x => x.Id, desc)
            ],
            "createdat" => [
                new(x => x.CreatedAt, desc),
                new(x => x.Title, desc)
            ],
            "updatedat" => [
                new(x => x.UpdatedAt, desc),
                new(x => x.Title, desc)
            ],
            _ => [
                new(x => x.UpdatedAt, desc),
                new(x => x.Title, desc)
            ],
        };
    }
}