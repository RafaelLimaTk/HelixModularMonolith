using Helix.Chat.UnitTests.Application.Conversation.Common;

namespace Helix.Chat.UnitTests.Application.Conversation.AddParticipant;

[CollectionDefinition(nameof(AddParticipantTestFixture))]
public class AddParticipantTestFixtureCollection
    : ICollectionFixture<AddParticipantTestFixture>
{ }

public class AddParticipantTestFixture
    : ConversationUseCasesBaseFixture
{ }
