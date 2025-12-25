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

    public string GetLongTitle(int lengthTitle = 128)
    {
        if (lengthTitle <= 0) return string.Empty;

        var builder = new StringBuilder();
        while (builder.Length < lengthTitle)
        {
            var word = Faker.Lorem.Word().Trim();
            if (word.Length == 0) continue;

            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(word);
        }

        return builder.ToString()[..lengthTitle];
    }

    public Guid GetValidUserId() => Guid.NewGuid();
}
