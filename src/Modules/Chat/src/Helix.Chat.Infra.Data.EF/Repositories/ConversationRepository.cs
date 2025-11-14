using Helix.Chat.Domain.Interfaces;
using Shared.Application.Exceptions;

namespace Helix.Chat.Infra.Data.EF.Repositories;
public class ConversationRepository(HelixChatDbContext context) : IConversationRepository
{
    private readonly HelixChatDbContext _context = context;
    private DbSet<Conversation> _conversations => _context.Set<Conversation>();

    public async Task Insert(Conversation conversation, CancellationToken cancellationToken)
        => await _conversations.AddAsync(conversation, cancellationToken);

    public async Task<Conversation> Get(Guid id, CancellationToken cancellationToken)
    {
        var conversation = await _conversations
            .FirstOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);
        NotFoundException.ThrowIfNull(conversation, $"Conversation '{id}' not found.");
        return conversation!;
    }

    public Task Update(Conversation conversation, CancellationToken _)
        => Task.FromResult(_conversations.Update(conversation));

    public Task Delete(Conversation conversation, CancellationToken _)
        => Task.FromResult(_conversations.Remove(conversation));
}
