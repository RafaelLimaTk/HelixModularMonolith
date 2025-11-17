using Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;
using Helix.Chat.UnitTests.Query.Application.Conversation.Common;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListMessagesByConversation;

[CollectionDefinition(nameof(ListMessagesByConversationTestFixture))]
public class ListMessagesByConversationTestFixtureCollection
    : ICollectionFixture<ListMessagesByConversationTestFixture>
{ }

public class ListMessagesByConversationTestFixture
    : ConversationQueryUseCasesBaseFixture
{
    public List<MessageQueryModel> GetExampleMessagesList(Guid conversationId, int length = 5)
    {
        var now = DateTime.UtcNow;
        return Enumerable.Range(0, length)
            .Select(messageIndex => CreateMessageQueryModel(
                conversationId,
                senderId: null,
                content: messageIndex % 2 == 0 ? $"Test MESSAGE {messageIndex}" : $"Hello {messageIndex}",
                sentAt: now.AddMinutes(-messageIndex),
                deliveredAt: messageIndex % 3 == 0 ? now.AddMinutes(-(messageIndex + 1)) : null,
                readAt: messageIndex % 5 == 0 ? now.AddMinutes(-(messageIndex + 2)) : null,
                status: messageIndex % 2 == 0 ? "Sent" : (messageIndex % 3 == 0 ? "Delivered" : "Read")
            ))
            .ToList();
    }

    public static List<MessageQueryModel> CloneMessagesListOrdered(
        List<MessageQueryModel> source,
        string sort,
        SearchOrder direction)
    {
        IEnumerable<MessageQueryModel> ordered = (sort.Trim().ToLowerInvariant(), direction) switch
        {
            ("status", SearchOrder.Asc) => source.OrderBy(x => x.Status).ThenBy(x => x.Id),
            ("status", SearchOrder.Desc) => source.OrderByDescending(x => x.Status).ThenByDescending(x => x.Id),
            ("deliveredat", SearchOrder.Asc) => source.OrderBy(x => x.DeliveredAt).ThenBy(x => x.Id),
            ("deliveredat", SearchOrder.Desc) => source.OrderByDescending(x => x.DeliveredAt).ThenByDescending(x => x.Id),
            ("readat", SearchOrder.Asc) => source.OrderBy(x => x.ReadAt).ThenBy(x => x.Id),
            ("readat", SearchOrder.Desc) => source.OrderByDescending(x => x.ReadAt).ThenByDescending(x => x.Id),
            ("sentat", SearchOrder.Asc) => source.OrderBy(x => x.SentAt).ThenBy(x => x.Id),
            ("sentat", SearchOrder.Desc) => source.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.Id),
            _ => source.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.Id),
        };
        return ordered.ToList();
    }

    public static ListMessagesByConversationInput GetExampleInput(Guid conversationId)
    {
        var random = new Random();
        return new ListMessagesByConversationInput(
            conversationId: conversationId,
            page: random.Next(1, 4),
            perPage: random.Next(5, 25),
            search: random.Next(0, 2) == 0 ? "" : "test",
            sort: random.Next(0, 5) switch
            {
                0 => "sentAt",
                1 => "status",
                2 => "readAt",
                3 => "deliveredAt",
                _ => ""
            },
            dir: random.Next(0, 10) > 5 ? SearchOrder.Asc : SearchOrder.Desc
        );
    }
}
