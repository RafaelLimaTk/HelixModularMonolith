using System.Text;

namespace Helix.Chat.IntegrationTests.Infra.Data.EF.Repositories.ConversationRepository;

[CollectionDefinition(nameof(ConversationRepositoryTestFixture))]
public class ConversationRepositoryTestFixtureCollection
    : ICollectionFixture<ConversationRepositoryTestFixture>
{ }

public class ConversationRepositoryTestFixture
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

    public List<Guid> GetParticipantIds(int count = 2)
        => Enumerable.Range(0, count)
            .Select(_ => Guid.NewGuid())
            .ToList();

    public Conversation GetConversationExample(int titleLength = 10, List<Guid>? userIds = null)
    {
        var conversation = new Conversation(GetShortTitle(titleLength));
        userIds?.ForEach(id => conversation.AddParticipant(id));
        return conversation;
    }
}
