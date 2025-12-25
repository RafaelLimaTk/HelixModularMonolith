namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.Common;

public class ConversationUseCasesBaseFixture : BaseFixture
{
    public string GetValidTitle()
    {
        var conversationTitle = "";
        while (conversationTitle.Length < 3)
            conversationTitle = Faker.Lorem.Sentence(3);
        if (conversationTitle.Length > 128)
            conversationTitle = conversationTitle[..128];
        return conversationTitle;
    }

    public string GetInvalidTitleTooLong(int length)
    {
        var tooLongTitleForConversation = Faker.Lorem.Paragraph();
        while (tooLongTitleForConversation.Length <= length)
            tooLongTitleForConversation = $"{tooLongTitleForConversation} {Faker.Lorem.Paragraph()}";
        return tooLongTitleForConversation;
    }

    public List<Guid> GetParticipantIds(int count = 2)
        => Enumerable.Range(0, count)
            .Select(_ => Guid.NewGuid())
            .ToList();

    public DomainEntity.Conversation GetExampleConversation(
        List<Guid>? participantIds = null)
    {
        var conversation = new DomainEntity.Conversation(GetValidTitle());
        participantIds?.ForEach(userId => conversation.AddParticipant(userId));
        return conversation;
    }

    public string GetValidContent()
    {
        var messageContent = "";
        while (messageContent.Length < 10)
            messageContent = Faker.Lorem.Sentence(10);
        if (messageContent.Length > 10_000)
            messageContent = messageContent[..10_000];
        return messageContent;
    }

    public string GetInvalidContentTooLong(int length)
    {
        var tooLongContentForConversation = Faker.Lorem.Paragraph();
        while (tooLongContentForConversation.Length <= length)
            tooLongContentForConversation = $"{tooLongContentForConversation} {Faker.Lorem.Paragraph()}";
        return tooLongContentForConversation;
    }
}