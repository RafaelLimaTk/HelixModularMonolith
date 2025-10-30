namespace Shared.Domain.SeedWorks;
public abstract class DomainEvent
{
    public DateTime OccuredOn { get; set; }
    protected DomainEvent()
    {
        OccuredOn = DateTime.UtcNow;
    }
}