using System.Text;

namespace Helix.Chat.IntegrationTests.Infra.Data.EF.UnitOfWork;

[CollectionDefinition(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTestFixtureCollection
    : ICollectionFixture<UnitOfWorkTestFixture>
{ }

public class UnitOfWorkTestFixture
    : BaseFixture
{
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

    public Conversation GetConversationExample(int titleLength = 10, List<Guid>? userIds = null)
    {
        var conversation = new Conversation(GetShortTitle(titleLength));
        userIds?.ForEach(id => conversation.AddParticipant(id));
        return conversation;
    }

    public List<Conversation> GetExampleConversationsList(int count = 5)
        => Enumerable.Range(0, count)
            .Select(_ => GetConversationExample())
            .ToList();
}
