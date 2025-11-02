using Helix.Chat.Domain.Interfaces;

namespace Helix.Chat.UnitTests.Application.Message.Common;

public class MessageUseCasesBaseFixture
    : BaseFixture
{
    public Mock<IUnitOfWork> GetUnitOfWorkMock()
        => new();

    public Mock<IConversationRepository> GetConversationRepositoryMock()
        => new();

    public Mock<IMessageRepository> GetMessageRepositoryMock()
        => new();

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

    public List<Guid> GetParticipantIds(int count = 2)
        => Enumerable.Range(0, count)
            .Select(_ => Guid.NewGuid())
            .ToList();

    public DomainEntity.Conversation GetConversationExample(int titleLength = 10, List<Guid>? userIds = null)
    {
        var conversation = new DomainEntity.Conversation(GetShortTitle(titleLength));
        userIds?.ForEach(id => conversation.AddParticipant(id));
        return conversation;
    }
}
