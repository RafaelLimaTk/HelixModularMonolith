namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.AddParticipant;

[CollectionDefinition(nameof(AddParticipantTestFixture))]
public class AddParticipantTestFixtureCollection
    : ICollectionFixture<AddParticipantTestFixture>
{ }

public class AddParticipantTestFixture
    : ConversationUseCasesBaseFixture
{
}
