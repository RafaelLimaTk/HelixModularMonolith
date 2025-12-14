using Shared.Query.Specifications;
using System.Linq.Expressions;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;

public sealed class ListMessagesByConversationSpecification
    : IQuerySpecification<MessageQueryModel>
{
    public Expression<Func<MessageQueryModel, bool>>? Criteria { get; }
    public IReadOnlyList<OrderExpression<MessageQueryModel>> Orders { get; }
    public int Page { get; }
    public int PerPage { get; }

    public ListMessagesByConversationSpecification(
        Guid conversationId,
        int page = 1,
        int perPage = 20,
        string search = "",
        string sort = "",
        SearchOrder dir = SearchOrder.Desc)
    {
        var term = (search ?? string.Empty).Trim().ToLowerInvariant();
        var hasTerm = term.Length > 0;

        Criteria = m =>
             m.ConversationId == conversationId &&
             (!hasTerm || m.Content.ToLowerInvariant().Contains(term));

        var orders = MapSortWithTieBreaker(sort, dir);
        Orders = orders;

        Page = page <= 0 ? 1 : page;
        PerPage = perPage <= 0 ? 20 : Math.Min(perPage, 100);
    }

    static List<OrderExpression<MessageQueryModel>> MapSortWithTieBreaker(
        string sort,
        SearchOrder dir)
    {
        var s = string.IsNullOrWhiteSpace(sort) ? "sentAt" : sort.Trim().ToLowerInvariant();
        var desc = dir == SearchOrder.Desc;

        return s switch
        {
            "status" =>
            [
                new(x => x.Status, desc),
                new(x => x.Id, desc)
            ],
            "deliveredat" =>
            [
                new(x => x.DeliveredAt!, desc),
                new(x => x.SentAt, desc)
            ],
            "readat" =>
            [
                new(x => x.ReadAt!, desc),
                new(x => x.SentAt, desc)
            ],
            "sentat" =>
            [
                new(x => x.SentAt, desc),
                new(x => x.Id, desc)
            ],
            _ =>
            [
                new(x => x.SentAt, desc),
                new(x => x.Id, desc)
            ]
        };
    }
}
