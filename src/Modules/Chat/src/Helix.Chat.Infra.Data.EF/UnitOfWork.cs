using Microsoft.Extensions.Logging;
using Shared.Infra.Outbox.Interfaces;
using System.Text.Json;

namespace Helix.Chat.Infra.Data.EF;
public class UnitOfWork
    : IUnitOfWork
{
    private readonly HelixChatDbContext _context;
    private readonly IOutboxStore _outbox;
    private readonly ILogger<UnitOfWork> _logger;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public UnitOfWork(
        HelixChatDbContext context,
        IOutboxStore outbox,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        var aggregateRoots = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.Events.Any())
            .Select(entry => entry.Entity);

        _logger.LogInformation(
            "Commit: {AggregatesCount} aggregate roots with events.",
            aggregateRoots.Count());

        var events = aggregateRoots
            .SelectMany(aggregate => aggregate.Events);

        _logger.LogInformation(
            "Commit: {EventsCount} events raised.", events.Count());

        foreach (var @event in events)
        {
            var type = @event.GetType();
            var payload = JsonSerializer.Serialize(@event, type, _json);
            var envelope = new EventEnvelope(
                eventName: type.Name,
                clrType: type.AssemblyQualifiedName!,
                payload: payload,
                occurredOnUtc: DateTime.UtcNow
            );
            await _outbox.AppendAsync(envelope, cancellationToken);
        }

        foreach (var aggregate in aggregateRoots)
            aggregate.ClearEvents();

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
