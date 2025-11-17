namespace Helix.Chat.Infra.Data.EF.Repositories;

public class MessageRepository(HelixChatDbContext context) : IMessageRepository
{
    private readonly HelixChatDbContext _context = context;
    private DbSet<Message> _messages => _context.Set<Message>();

    public async Task Insert(Message message, CancellationToken cancellationToken)
        => await _messages.AddAsync(message, cancellationToken);

    public async Task<Message> Get(Guid id, CancellationToken cancellationToken)
    {
        var message = await _messages
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken);
        NotFoundException.ThrowIfNull(message, $"Message '{id}' not found.");
        return message!;
    }

    public Task Update(Message message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
