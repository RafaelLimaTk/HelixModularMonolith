using Shared.Infra.Outbox.Interfaces;

namespace Helix.Chat.Infra.Data.EF;

public sealed class EfOutboxStore(HelixChatDbContext dbContext) : IOutboxStore
{
    private readonly HelixChatDbContext _dbContext = dbContext;

    public Task AppendAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var outboxMessage = OutboxMessage.Create(
            type: envelope.ClrType,
            payload: envelope.Payload,
            occurredOnUtc: envelope.OccurredOnUtc
        );
        _dbContext.OutboxMessages.Add(outboxMessage);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<OutboxMessage>> TakeBatchAsync(int max, CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.Id)
            .Take(max)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(long id, CancellationToken cancellationToken)
    {
        var message = await _dbContext.OutboxMessages
            .FirstAsync(message => message.Id == id, cancellationToken);
        message.MarkProcessed();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(long id, string error, CancellationToken cancellationToken)
    {
        var message = await _dbContext.OutboxMessages
            .FirstAsync(message => message.Id == id, cancellationToken);
        message.MarkFailed(error);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}