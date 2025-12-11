using System.Text;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Message.Common;

public class MessageUseCasesBaseFixture : BaseFixture
{
    public string GetShortContent(int length)
    {
        if (length <= 0) return string.Empty;

        var word = Faker.Lorem.Word().Replace(" ", "");
        if (word.Length >= length) return word[..length];

        var builder = new StringBuilder(word);
        while (builder.Length < length)
        {
            var next = Faker.Random.AlphaNumeric(Math.Min(length - builder.Length, 8));
            builder.Append(next);
        }
        return builder.ToString()[..length];
    }

    public string GetValidContent()
        => Faker.Lorem.Sentence();

    public string GetLongContent(int length)
    {
        var builder = new StringBuilder();

        while (builder.Length < length)
        {
            var sentence = Faker.Lorem.Sentence().Trim();
            if (sentence.Length == 0) continue;

            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(sentence);
        }

        var result = builder.ToString().Trim();
        if (result.Length <= length)
        {
            var extra = Faker.Lorem.Sentence().Trim();
            if (extra.Length > 0)
                result = string.Concat(result, " ", extra);
        }

        return result;
    }

    public DomainEntity.Conversation GetExampleConversation(
        string? title = null,
        List<Guid>? participantIds = null)
    {
        var conversation = new DomainEntity.Conversation(
            title ?? Faker.Lorem.Sentence(3).Trim()
        );

        participantIds?.ForEach(userId => conversation.AddParticipant(userId));

        return conversation;
    }

    public DomainEntity.Message GetExampleMessage(
        Guid? conversationId = null,
        Guid? senderId = null,
        string? content = null)
    {
        var message = new DomainEntity.Message(
            conversationId ?? Guid.NewGuid(),
            senderId ?? Guid.NewGuid(),
            content ?? GetValidContent()
        );

        return message;
    }

    public List<DomainEntity.Message> GetExampleMessagesList(
        int count,
        Guid conversationId,
        Guid senderId)
        => Enumerable.Range(0, count)
            .Select(_ => GetExampleMessage(conversationId, senderId))
            .ToList();
}