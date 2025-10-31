using Helix.Chat.UnitTests.Application.Conversation.Common;

namespace Helix.Chat.UnitTests.Application.Conversation.SendMessage;
public class SendMessageTestFixture
    : ConversationUseCasesBaseFixture
{
    public string GetValidContent()
        => Faker.Lorem.Sentence();

    public string GetLongContent(int len)
    {
        if (len <= 0) return string.Empty;
        var builder = new StringBuilder(Faker.Lorem.Paragraph());
        while (builder.Length < len) builder.Append(' ').Append(Faker.Lorem.Sentence());
        return builder.ToString()[..len];
    }
}
