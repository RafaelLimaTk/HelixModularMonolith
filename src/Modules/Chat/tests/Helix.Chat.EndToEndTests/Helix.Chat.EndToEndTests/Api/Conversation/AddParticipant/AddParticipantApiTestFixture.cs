using Helix.Chat.Endpoints.ApiModels.Conversation;

namespace Helix.Chat.EndToEndTests.Api.Conversation.AddParticipant;

[CollectionDefinition(nameof(AddParticipantApiTestFixture))]
public class AddParticipantApiTestFixtureCollection
    : ICollectionFixture<AddParticipantApiTestFixture>
{ }

public class AddParticipantApiTestFixture
    : ConversationBaseFixture
{
    public AddParticipantApiInput GetValidInput(Guid participantId)
        => new(participantId);
}
