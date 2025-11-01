namespace Shared.Domain.SeedWorks;
public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : DomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}