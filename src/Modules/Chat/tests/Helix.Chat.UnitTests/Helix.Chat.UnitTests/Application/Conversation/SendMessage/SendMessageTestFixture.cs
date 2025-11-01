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

    public string GetLongContent(int lenghtContent = 10000)
    {
        var builder = new StringBuilder();

        while (builder.Length < lenghtContent)
        {
            var sentence = Faker.Lorem.Sentence().Trim();
            if (sentence.Length == 0) continue;

            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(sentence);
        }

        var result = builder.ToString().Trim();
        if (result.Length <= lenghtContent)
        {
            var extra = Faker.Lorem.Sentence().Trim();
            if (extra.Length > 0)
                result = string.Concat(result, " ", extra);
        }

        return result;
    }
}
