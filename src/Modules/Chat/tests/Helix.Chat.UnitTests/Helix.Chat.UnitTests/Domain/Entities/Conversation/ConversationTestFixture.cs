namespace Helix.Chat.UnitTests.Domain.Entities.Conversation;

[CollectionDefinition(nameof(ConversationTestFixture))]
public class ConversationTestFixtureCollection
    : ICollectionFixture<ConversationTestFixture>
{ }

public class ConversationTestFixture : BaseFixture
{
    public string GetValidTitle()
        => Faker.Lorem.Sentence(3).Trim();

    public string GetValidContent() => Faker.Lorem.Sentence();

    public string GetShortTitle(int len)
    {
        if (len <= 0) return string.Empty;

        var word = Faker.Lorem.Word().Replace(" ", "");
        if (word.Length >= len) return word[..len];

        var builder = new StringBuilder(word);
        while (builder.Length < len)
        {
            var next = Faker.Random.AlphaNumeric(Math.Min(len - builder.Length, 8));
            builder.Append(next);
        }
        return builder.ToString()[..len];
    }

    public string GetLongTitle(int len)
    {
        if (len <= 0) return string.Empty;

        var sentence = Faker.Lorem.Sentence(Math.Max(2, len / 10));
        if (sentence.Length >= len) return sentence[..len].Trim();

        var builder = new StringBuilder(sentence);
        while (builder.Length < len)
        {
            builder.Append(' ').Append(Faker.Lorem.Word());
        }
        return builder.ToString()[..len].Trim();
    }

    public Guid GetValidUserId() => Guid.NewGuid();
}
