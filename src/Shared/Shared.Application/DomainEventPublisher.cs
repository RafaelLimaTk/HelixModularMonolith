using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.SeedWorks;

namespace Shared.Application;
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;
    public DomainEventPublisher(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task PublishAsync<TDomainEvent>(
        TDomainEvent domainEvent, CancellationToken cancellationToken)
            where TDomainEvent : DomainEvent
    {
        using var scope = _scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider
            .GetServices<IDomainEventHandler<TDomainEvent>>();
        if (handlers is null) return;

        foreach (var handler in handlers)
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
