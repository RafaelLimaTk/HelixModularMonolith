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
        => Enumerable.Range(0, length)
            .Select(_ => GetExampleConversation())
            .ToList();

    public List<ConversationQueryModel> GetExampleConversationsListByTitles(List<string> titles)
        => titles.Select(title => GetExampleConversation(title: title)).ToList();
}
