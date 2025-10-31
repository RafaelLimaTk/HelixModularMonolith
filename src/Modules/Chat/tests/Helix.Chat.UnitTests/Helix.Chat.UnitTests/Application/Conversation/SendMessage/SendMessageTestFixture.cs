using Helix.Chat.UnitTests.Application.Conversation.Common;

namespace Helix.Chat.UnitTests.Application.Conversation.SendMessage;
public class SendMessageTestFixture
    : ConversationUseCasesBaseFixture
{
    public string GetValidContent()
        => Faker.Lorem.Sentence();
}
