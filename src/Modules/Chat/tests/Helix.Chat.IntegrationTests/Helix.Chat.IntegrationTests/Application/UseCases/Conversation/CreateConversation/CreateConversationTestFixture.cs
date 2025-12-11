using Helix.Chat.IntegrationTests.Application.UseCases.Conversation.Common;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.CreateConversation;

[CollectionDefinition(nameof(CreateConversationTestFixture))]
public class CreateConversationTestFixtureCollection
    : ICollectionFixture<CreateConversationTestFixture>
{ }

public class CreateConversationTestFixture
    : ConversationUseCasesBaseFixture
{
}
