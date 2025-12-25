using Helix.Chat.UnitTests.Application.Conversation.Common;

namespace Helix.Chat.UnitTests.Application.Conversation.SendMessage;

[CollectionDefinition(nameof(SendMessageTestFixture))]
public class SendMessageTestFixtureCollection
    : ICollectionFixture<SendMessageTestFixture>
{ }

public class SendMessageTestFixture
    : ConversationUseCasesBaseFixture
{
    public string GetValidContent()
        => Faker.Lorem.Sentence();

    public string GetLongContent(int lengthContent = 10000)
    {
        if (lengthContent <= 0) return string.Empty;

        var builder = new StringBuilder();

        while (builder.Length < lengthContent)
        {
            var sentence = Faker.Lorem.Sentence().Trim();
            if (sentence.Length == 0) continue;

            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(sentence);
        }

        return builder.ToString()[..lengthContent];
    }
}
