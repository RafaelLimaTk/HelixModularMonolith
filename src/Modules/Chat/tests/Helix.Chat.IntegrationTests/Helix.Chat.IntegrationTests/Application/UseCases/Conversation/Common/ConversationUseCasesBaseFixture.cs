using System.Text;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.Common;

public class ConversationUseCasesBaseFixture : BaseFixture
{
    public string GetShortTitle(int length)
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

    public string GetValidTitle()
        => Faker.Lorem.Sentence(3).Trim();

    public string GetLongTitle(int length)
    {
        var builder = new StringBuilder();
        while (builder.Length < length)
        {
            var word = Faker.Lorem.Word().Trim();
            if (word.Length == 0) continue;

            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(word);
        }

        var result = builder.ToString().Trim();
        if (result.Length <= length)
        {
            var extra = Faker.Lorem.Word().Trim();
            if (extra.Length > 0)
                result = string.Concat(result, " ", extra);
        }

        return result.Trim();
    }

    public List<Guid> GetParticipantIds(int count = 2)
        => Enumerable.Range(0, count)
            .Select(_ => Guid.NewGuid())
            .ToList();

    public Domain.Entities.Conversation GetExampleConversation(
        int titleLength = 10,
        List<Guid>? participantIds = null)
    {
        var conversation = new Domain.Entities.Conversation(GetShortTitle(titleLength));

        participantIds?.ForEach(userId => conversation.AddParticipant(userId));

        return conversation;
    }

    public List<Domain.Entities.Conversation> GetExampleConversationsList(int count = 5)
        => Enumerable.Range(0, count)
            .Select(_ => GetExampleConversation())
            .ToList();

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
}