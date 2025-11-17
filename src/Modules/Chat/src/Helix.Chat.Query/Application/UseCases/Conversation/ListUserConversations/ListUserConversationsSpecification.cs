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
        var normalizedSearch = (search ?? string.Empty).Trim();
        var hasTerm = !string.IsNullOrEmpty(normalizedSearch);
        var termLower = normalizedSearch.ToLowerInvariant();

        Criteria = c =>
            c.ParticipantIds.Contains(userId) &&
            (!hasTerm || c.Title.Contains(termLower));

        var keySelector = MapSort(sort);
        Orders = [new OrderExpression<ConversationQueryModel>(keySelector, dir == SearchOrder.Desc)];

        Page = page <= 0 ? 1 : page;
        PerPage = perPage <= 0 ? 20 : Math.Min(perPage, 100);
    }

    static Expression<Func<ConversationQueryModel, object>> MapSort(string sort)
    {
        var s = string.IsNullOrWhiteSpace(sort) ? "updatedAt" : sort;
        switch (s.Trim().ToLowerInvariant())
        {
            case "title": return x => x.Title;
            case "createdat": return x => x.CreatedAt;
            case "updatedat": return x => x.UpdatedAt;
            default: return x => x.UpdatedAt;
        }
    }
}