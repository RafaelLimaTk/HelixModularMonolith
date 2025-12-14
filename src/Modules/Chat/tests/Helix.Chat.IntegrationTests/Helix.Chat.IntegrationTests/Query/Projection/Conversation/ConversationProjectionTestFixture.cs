namespace Helix.Chat.IntegrationTests.Query.Projection.Conversation;

[CollectionDefinition(nameof(ConversationProjectionTestFixture))]
public class ConversationProjectionTestFixtureCollection
    : ICollectionFixture<ConversationProjectionTestFixture>
{ }

public class ConversationProjectionTestFixture
    : QueryBaseFixture
{
    public ISynchronizeDb CreateSynchronizeDb(bool preserveData = false)
    {
        var context = CreateReadDbContext(preserveData);
        return new MongoSynchronizeDb(context);
    }

    public string GetValidTitle() => Faker.Commerce.ProductName();

    public ConversationQueryModel GetExampleConversation(
        Guid? id = null,
        string? title = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        List<Guid>? participantIds = null)
    {
        var conversationId = id ?? Guid.NewGuid();
        var created = createdAt ?? DateTime.UtcNow;
        var updated = updatedAt ?? created.AddSeconds(5);

        return new ConversationQueryModel
        {
            Id = conversationId,
            Title = title ?? GetValidTitle(),
            CreatedAt = created,
            UpdatedAt = updated,
            ParticipantIds = participantIds ?? new List<Guid>()
        };
    }
}
