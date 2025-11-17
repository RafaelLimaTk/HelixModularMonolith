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
            (!hasTerm || m.Content.Contains(term, StringComparison.InvariantCultureIgnoreCase));

        var key = MapSort(sort);
        Orders = [new OrderExpression<MessageQueryModel>(key, dir == SearchOrder.Desc)];

        Page = page <= 0 ? 1 : page;
        PerPage = perPage <= 0 ? 20 : Math.Min(perPage, 100);
    }

    static Expression<Func<MessageQueryModel, object>> MapSort(string sort)
    {
        var s = string.IsNullOrWhiteSpace(sort) ? "sentAt" : sort;
        switch (s.Trim().ToLowerInvariant())
        {
            case "status": return x => x.Status;
            case "readat": return x => x.ReadAt!;
            case "deliveredat": return x => x.DeliveredAt!;
            case "sentat": return x => x.SentAt;
            default: return x => x.SentAt;
        }
    }
}
