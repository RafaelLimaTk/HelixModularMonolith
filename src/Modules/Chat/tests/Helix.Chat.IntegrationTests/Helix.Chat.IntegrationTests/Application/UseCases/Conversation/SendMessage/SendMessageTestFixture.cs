namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.SendMessage;

[CollectionDefinition(nameof(SendMessageTestFixture))]
public class SendMessageTestFixtureCollection
    : ICollectionFixture<SendMessageTestFixture>
{ }

public class SendMessageTestFixture
    : ConversationUseCasesBaseFixture
{
}