using Helix.Chat.IntegrationTests.Query.Base;

namespace Helix.Chat.IntegrationTests.Query.Repositories.ConversationReadOnlyRepository;

[CollectionDefinition(nameof(ConversationReadOnlyRepositoryTestFixture))]
public class ConversationReadOnlyRepositoryTestFixtureCollection
    : ICollectionFixture<ConversationReadOnlyRepositoryTestFixture>
{ }


public class ConversationReadOnlyRepositoryTestFixture
    : QueryBaseFixture
{
    public List<Guid> GetExampleParticipantsList()
        => Enumerable.Range(0, Faker.Random.Int(2, 5))
            .Select(_ => Guid.NewGuid())
            .ToList();

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
            ?? GetExampleParticipantsList();

        return new ConversationQueryModel
        {
            Id = Guid.NewGuid(),
            Title = resolvedTitle,
            ParticipantIds = participants,
            CreatedAt = created,
            UpdatedAt = updated,
        };
    }

    public List<ConversationQueryModel> GetExampleConversationsList(
        int length = 10,
        IEnumerable<Guid>? commonParticipantIds = null,
        double mixRatio = 0.5,
        int seed = 42)
    {
        var baseTime = DateTime.UtcNow;

        var conversations = Enumerable.Range(0, length)
            .Select(index => GetExampleConversation(
                createdAt: baseTime.AddMilliseconds(index * 10),
                updatedAt: baseTime.AddMilliseconds((index * 10) + 1)))
            .ToList();

        if (commonParticipantIds == null)
            return conversations;

        var commonList = commonParticipantIds.ToList();
        if (commonList.Count == 0)
            return conversations;

        var random = new Random(seed);

        var targetsCount = Math.Max(1, (int)Math.Round(conversations.Count * Math.Clamp(mixRatio, 0.0, 1.0)));

        var targetConversations = conversations
            .OrderBy(_ => random.Next())
            .Take(targetsCount)
            .ToList();

        foreach (var conversation in targetConversations)
        {
            var participantsToAddCount = random.Next(1, commonList.Count + 1);

            var participantsToAdd = commonList
                .OrderBy(_ => random.Next())
                .Take(participantsToAddCount);

            foreach (var participantId in participantsToAdd)
            {
                if (!conversation.ParticipantIds.Contains(participantId))
                    conversation.ParticipantIds.Add(participantId);
            }
        }

        return conversations;
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
        return ApplyOrdering(listClone, orderBy, order).ToList();
    }

    public List<ConversationQueryModel> FilterOrderAndPaginate(
        List<ConversationQueryModel> source,
        Func<ConversationQueryModel, bool>? predicate = null,
        string orderBy = "createdAt",
        SearchOrder order = SearchOrder.Desc,
        int page = 1,
        int perPage = 10)
    {
        IEnumerable<ConversationQueryModel> query = source;

        if (predicate != null)
            query = query.Where(predicate);

        var ordered = ApplyOrdering(query, orderBy, order);

        var skip = (Math.Max(1, page) - 1) * Math.Max(1, perPage);
        return ordered.Skip(skip).Take(Math.Max(0, perPage)).ToList();
    }

    private static IOrderedEnumerable<ConversationQueryModel> ApplyOrdering(
        IEnumerable<ConversationQueryModel> source,
        string orderBy,
        SearchOrder dir)
    {
        var orderKey = orderBy.Trim().ToLowerInvariant();
        return (orderKey, dir) switch
        {
            ("title", SearchOrder.Asc) => source.OrderBy(c => c.Title).ThenBy(c => c.Id),
            ("title", SearchOrder.Desc) => source.OrderByDescending(c => c.Title).ThenByDescending(c => c.Id),
            ("createdat", SearchOrder.Asc) => source.OrderBy(c => c.CreatedAt).ThenBy(c => c.Title),
            ("createdat", SearchOrder.Desc) => source.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Title),
            ("updatedat", SearchOrder.Asc) => source.OrderBy(c => c.UpdatedAt).ThenBy(c => c.Title),
            ("updatedat", SearchOrder.Desc) => source.OrderByDescending(c => c.UpdatedAt).ThenByDescending(c => c.Title),
            _ => source.OrderBy(c => c.Title).ThenBy(c => c.Id),
        };
    }
}
