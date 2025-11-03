using Shared.Domain.SeedWorks;

namespace Shared.Application.Projections;
public interface IProjectionHandler<in TEvent> where TEvent : DomainEvent
{
    Task ProjectAsync(TEvent @event, CancellationToken cancellationToken);
}