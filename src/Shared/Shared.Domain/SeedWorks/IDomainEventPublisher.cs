namespace Shared.Domain.SeedWorks;
public interface IDomainEventPublisher
{
    Task PublishAsync<TDomainEvent>(
        TDomainEvent domainEvent, CancellationToken cancellationToken)
            where TDomainEvent : DomainEvent;
}