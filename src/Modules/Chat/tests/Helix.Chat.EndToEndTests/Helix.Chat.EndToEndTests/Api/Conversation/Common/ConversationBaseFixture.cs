namespace Helix.Chat.EndToEndTests.Api.Conversation.Common;

public class ConversationBaseFixture
    : BaseFixture
{
    public ConversationPersistence ConversationPersistence;

    public ConversationBaseFixture()
        : base()
    {
        ConversationPersistence = new ConversationPersistence(CreateDbContext());
    }

    public string GetValidConversationTitle()
    {
        var conversationTitle = "";
        while (conversationTitle.Length < 3)
            conversationTitle = Faker.Lorem.Sentence(3);
        if (conversationTitle.Length > 128)
            conversationTitle = conversationTitle[..128];
        return conversationTitle;
    }

    public string GetInvalidTitleTooShort()
        => Faker.Lorem.Sentence(1)[..2];

    public string GetInvalidTitleTooLong()
    {
        var tooLongTitleForConversation = Faker.Lorem.Paragraph();
        while (tooLongTitleForConversation.Length <= 128)
            tooLongTitleForConversation = $"{tooLongTitleForConversation} {Faker.Lorem.Paragraph()}";
        return tooLongTitleForConversation;
    }

    public DomainEntity.Conversation GetExampleConversation()
        => new(GetValidConversationTitle());

    public List<DomainEntity.Conversation> GetExampleConversationsList(int count = 10)
        => Enumerable.Range(0, count)
            .Select(_ => GetExampleConversation())
            .ToList();
}
