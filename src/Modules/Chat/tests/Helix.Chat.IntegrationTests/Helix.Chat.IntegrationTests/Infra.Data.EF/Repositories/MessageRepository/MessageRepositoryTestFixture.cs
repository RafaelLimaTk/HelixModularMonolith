namespace Helix.Chat.IntegrationTests.Infra.Data.EF.Repositories.MessageRepository;

[CollectionDefinition(nameof(MessageRepositoryTestFixture))]
public class MessageRepositoryTestFixtureCollection
    : ICollectionFixture<MessageRepositoryTestFixture>
{ }

public class MessageRepositoryTestFixture
    : BaseFixture
{
    public Message GetMessageExample(Guid? conversationId = null, Guid? senderId = null)
    {
        var message = new Message(
            conversationId ?? Guid.NewGuid(),
            senderId ?? Guid.NewGuid(),
            Faker.Lorem.Sentence());
        return message;
    }
}
