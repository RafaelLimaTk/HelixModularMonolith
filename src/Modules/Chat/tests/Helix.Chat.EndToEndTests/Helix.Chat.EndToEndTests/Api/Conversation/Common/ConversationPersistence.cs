namespace Helix.Chat.EndToEndTests.Api.Conversation.Common;

public sealed class ConversationPersistence(HelixChatDbContext context)
{
    private readonly HelixChatDbContext _context = context;

    public async Task<DomainEntity.Conversation?> GetById(Guid id)
        => await _context
            .Conversations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task InsertList(IList<DomainEntity.Conversation> conversations)
    {
        await _context.Conversations.AddRangeAsync(conversations);
        await _context.SaveChangesAsync();
    }
}
