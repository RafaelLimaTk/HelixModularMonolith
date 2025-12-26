using Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.EndToEndTests.Api.Conversation.CreateConversation;

[CollectionDefinition(nameof(CreateConversationApiTestFixture))]
public class CreateConversationApiTestFixtureCollection
    : ICollectionFixture<CreateConversationApiTestFixture>
{ }

public class CreateConversationApiTestFixture
    : ConversationBaseFixture
{
    public CreateConversationInput GetValidInput()
        => new(GetValidConversationTitle());

    public CreateConversationInput GetInvalidInput()
        => new(string.Empty);
}
