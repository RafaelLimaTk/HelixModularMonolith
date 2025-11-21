using Helix.Chat.IntegrationTests.Query.Base;

namespace Helix.Chat.IntegrationTests.Query.Repositories.ConversationReadOnlyRepository;

[CollectionDefinition(nameof(ConversationReadOnlyRepositoryTestFixture))]
public class ConversationReadOnlyRepositoryTestFixtureCollection
    : ICollectionFixture<ConversationReadOnlyRepositoryTestFixture>
{ }


public class ConversationReadOnlyRepositoryTestFixture
    : QueryBaseFixture
{
    public ConversationQueryModel GetExampleConversation(
        string? title = null,
        IEnumerable<Guid>? participantIds = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var resolvedTitle = title ?? Faker.Lorem.Sentence(3);

        var created = createdAt
            ?? DateTime.UtcNow.AddMinutes(-Faker.Random.Int(0, 60));

        var updated = updatedAt
            ?? created.AddMinutes(Faker.Random.Int(0, 60));

        var participants = participantIds?.ToList()
            ?? Enumerable.Range(0, Faker.Random.Int(2, 5))
                .Select(_ => Guid.NewGuid())
                .ToList();

        return new ConversationQueryModel
        {
            Id = Guid.NewGuid(),
            Title = resolvedTitle,
            ParticipantIds = participants,
            CreatedAt = created,
            UpdatedAt = updated,
        };
    }

    public List<ConversationQueryModel> GetExampleConversationsList(int length = 10)
    {
        var baseTime = DateTime.UtcNow;
        return Enumerable.Range(0, length)
            .Select(i => GetExampleConversation(
                createdAt: baseTime.AddMilliseconds(i * 10),
                updatedAt: baseTime.AddMilliseconds((i * 10) + 1)))
            .ToList();
    }

    public List<ConversationQueryModel> GetExampleConversationsListByTitles(List<string> titles)
        => titles.Select(title => GetExampleConversation(title: title)).ToList();

    public List<ConversationQueryModel> CloneConversationsListOrdered(
        List<ConversationQueryModel> conversationsList,
        string orderBy,
        SearchOrder order
    )
    {
        var listClone = new List<ConversationQueryModel>(conversationsList);
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("title", SearchOrder.Asc) => listClone.OrderBy(c => c.Title)
                .ThenBy(c => c.Id),
            ("title", SearchOrder.Desc) => listClone.OrderByDescending(c => c.Title)
                .ThenByDescending(c => c.Id),
            ("createdat", SearchOrder.Asc) => listClone.OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Title),
            ("createdat", SearchOrder.Desc) => listClone.OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Title),
            ("updatedat", SearchOrder.Asc) => listClone.OrderBy(c => c.UpdatedAt)
                .ThenBy(c => c.Title),
            ("updatedat", SearchOrder.Desc) => listClone.OrderByDescending(c => c.UpdatedAt)
                .ThenByDescending(c => c.Title),
            _ => listClone.OrderBy(c => c.Title).ThenBy(c => c.Id),
        };
        return orderedEnumerable.ToList();
    }
}
