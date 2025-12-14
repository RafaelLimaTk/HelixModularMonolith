using Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.Common;

namespace Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.ListUserConversations;

[CollectionDefinition(nameof(ListUserConversationsTestFixture))]
public class ListUserConversationsTestFixtureCollection
    : ICollectionFixture<ListUserConversationsTestFixture>
{ }

public class ListUserConversationsTestFixture
    : ConversationQueryUseCasesBaseFixture
{
    public List<ConversationQueryModel> GetExampleConversationsListByTitles(
            List<string> titles,
            Guid? userId = null)
    {
        var baseTime = DateTime.UtcNow;
        var resolvedUserId = userId ?? Guid.NewGuid();

        return titles.Select((title, index) =>
        {
            var createdAt = baseTime.AddMinutes(-index);
            var updatedAt = createdAt.AddSeconds(30);

            return new ConversationQueryModel
            {
                Id = Guid.NewGuid(),
                Title = title,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ParticipantIds = [resolvedUserId, Guid.NewGuid()],
                LastMessage = index % 2 == 0
                    ? new ConversationQueryModel.MessageSnapshot(
                        MessageId: Guid.NewGuid(),
                        Content: $"Last message for {title}",
                        SentAt: updatedAt.AddSeconds(-5),
                        Status: "Sent")
                    : null
            };
        }).ToList();
    }

    public List<ConversationQueryModel> CloneConversationsListOrdered(
        List<ConversationQueryModel> source,
        string sort,
        SearchOrder direction)
    {
        var normalizedSort = string.IsNullOrWhiteSpace(sort)
            ? "updatedAt"
            : sort.Trim().ToLowerInvariant();

        IEnumerable<ConversationQueryModel> ordered = (normalizedSort, direction) switch
        {
            ("title", SearchOrder.Asc) => source
                .OrderBy(x => x.Title)
                .ThenBy(x => x.Id),
            ("title", SearchOrder.Desc) => source
                .OrderByDescending(x => x.Title)
                .ThenByDescending(x => x.Id),

            ("createdat", SearchOrder.Asc) => source
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Title),
            ("createdat", SearchOrder.Desc) => source
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Title),

            ("updatedat", SearchOrder.Asc) => source
                .OrderBy(x => x.UpdatedAt)
                .ThenBy(x => x.Title),
            ("updatedat", SearchOrder.Desc) => source
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Title),

            _ => source
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Title),
        };

        return ordered.ToList();
    }
}
