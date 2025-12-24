namespace Shared.Infra.Outbox.Interfaces;

public interface IOutboxProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}
