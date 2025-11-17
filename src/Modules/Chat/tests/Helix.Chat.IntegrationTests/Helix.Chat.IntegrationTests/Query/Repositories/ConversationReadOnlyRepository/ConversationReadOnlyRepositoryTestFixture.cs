using Helix.Chat.IntegrationTests.Query.Base;
using Helix.Chat.Query.Models;

namespace Helix.Chat.IntegrationTests.Query.Repositories.ConversationReadOnlyRepository;

[CollectionDefinition(nameof(ConversationReadOnlyRepositoryTestFixture))]
public class ConversationReadOnlyRepositoryTestFixtureCollection
    : ICollectionFixture<ConversationReadOnlyRepositoryTestFixture>
{ }


public class ConversationReadOnlyRepositoryTestFixture
    : QueryBaseFixture
{
    public ConversationQueryModel GetExampleConversation(
        Guid? id = null,
        IEnumerable<Guid>? participantIds = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var conversationId = id ?? Guid.NewGuid();
        var created = createdAt ?? DateTime.UtcNow.AddMinutes(-Faker.Random.Int(0, 60));
        var updated = updatedAt ?? created.AddMinutes(Faker.Random.Int(0, 60));

        var participants = (participantIds ?? Enumerable.Range(0, 3)
            .Select(_ => Guid.NewGuid()))
            .ToList();

        return new ConversationQueryModel
        {
            Id = conversationId,
            Title = Faker.Lorem.Sentence(3),
            ParticipantIds = participants,
            CreatedAt = created,
            UpdatedAt = updated,
        };
    }

    public List<ConversationQueryModel> GetExampleConversationsList(int length = 10)
        => Enumerable.Range(0, length)
            .Select(_ => GetExampleConversation())
            .ToList();
}
