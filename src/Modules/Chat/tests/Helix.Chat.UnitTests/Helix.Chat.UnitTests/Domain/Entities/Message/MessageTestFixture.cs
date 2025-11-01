namespace Helix.Chat.UnitTests.Domain.Entities.Message;

[CollectionDefinition(nameof(MessageTestFixture))]
public class MessageTestFixtureCollection
    : ICollectionFixture<MessageTestFixture>
{ }

public class MessageTestFixture : BaseFixture
{
    public Guid GetValidConversationId() => Guid.NewGuid();
    public Guid GetValidSenderId() => Guid.NewGuid();
    public string GetValidContent() => Faker.Lorem.Sentence();
    public string GetValidContent(int length = 3)
    {
        if (length <= 0) return string.Empty;

        var sb = new StringBuilder();
        while (sb.Length < length)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(Faker.Lorem.Sentence().Trim());
        }

        var result = sb.ToString().Substring(0, length);

        result = result.Trim();
        if (result.Length < length)
            result = string.Concat(result, Faker.Random.AlphaNumeric(length - result.Length));
        else if (result.Length > length)
            result = result.Substring(0, length);

        return result;
    }

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
