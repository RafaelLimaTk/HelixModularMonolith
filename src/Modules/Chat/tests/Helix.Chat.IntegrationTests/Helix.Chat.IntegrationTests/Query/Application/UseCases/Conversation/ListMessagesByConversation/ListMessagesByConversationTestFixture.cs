using Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.Common;

namespace Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.ListMessagesByConversation;

[CollectionDefinition(nameof(ListMessagesByConversationTestFixture))]
public class ListMessagesByConversationTestFixtureCollection
    : ICollectionFixture<ListMessagesByConversationTestFixture>
{ }

public class ListMessagesByConversationTestFixture : ConversationQueryUseCasesBaseFixture
{
    public List<MessageQueryModel> CloneMessagesListOrdered(
        List<MessageQueryModel> source,
        string sort,
        SearchOrder direction)
    {
        var normalizedSort = string.IsNullOrWhiteSpace(sort)
            ? "sentAt"
            : sort.Trim().ToLowerInvariant();

        IEnumerable<MessageQueryModel> ordered = (normalizedSort, direction) switch
        {
            ("status", SearchOrder.Asc) => source
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Id),
            ("status", SearchOrder.Desc) => source
                .OrderByDescending(x => x.Status)
                .ThenByDescending(x => x.Id),
            ("deliveredat", SearchOrder.Asc) => source
                .OrderBy(x => x.DeliveredAt)
                .ThenBy(x => x.SentAt),
            ("deliveredat", SearchOrder.Desc) => source
                .OrderByDescending(x => x.DeliveredAt)
                .ThenByDescending(x => x.SentAt),
            ("readat", SearchOrder.Asc) => source
                .OrderBy(x => x.ReadAt)
                .ThenBy(x => x.SentAt),
            ("readat", SearchOrder.Desc) => source
                .OrderByDescending(x => x.ReadAt)
                .ThenByDescending(x => x.SentAt),
            ("sentat", SearchOrder.Asc) => source
                .OrderBy(x => x.SentAt)
                .ThenBy(x => x.Id),
            ("sentat", SearchOrder.Desc) => source
                .OrderByDescending(x => x.SentAt)
                .ThenByDescending(x => x.Id),
            _ => source
                .OrderByDescending(x => x.SentAt)
                .ThenByDescending(x => x.Id),
        };

        return ordered.ToList();
    }
}