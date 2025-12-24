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
}
