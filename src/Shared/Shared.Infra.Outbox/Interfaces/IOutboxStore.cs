using Shared.Infra.Outbox.Models;

namespace Shared.Infra.Outbox.Interfaces;
public interface IOutboxStore
{
    Task AppendAsync(EventEnvelope envelope, CancellationToken ct);
    Task<IReadOnlyList<OutboxMessage>> TakeBatchAsync(int max, CancellationToken ct);
    Task MarkProcessedAsync(long id, CancellationToken ct);
    Task MarkFailedAsync(long id, string error, CancellationToken ct);
}
