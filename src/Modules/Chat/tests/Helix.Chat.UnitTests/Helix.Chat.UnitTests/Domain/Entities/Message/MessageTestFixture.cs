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
    public string GetLongContent(int len)
    {
        if (len <= 0) return string.Empty;
        var builder = new StringBuilder(Faker.Lorem.Paragraph());
        while (builder.Length < len) builder.Append(' ').Append(Faker.Lorem.Sentence());
        return builder.ToString()[..len];
    }
}
