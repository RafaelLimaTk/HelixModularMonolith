namespace Shared.Infra.Outbox.Models;
public sealed class EventEnvelope
{
    public string EventName { get; }
    public string ClrType { get; }
    public string Payload { get; }
    public DateTime OccurredOnUtc { get; }

    public EventEnvelope(string eventName, string clrType, string payload, DateTime occurredOnUtc)
        => (EventName, ClrType, Payload, OccurredOnUtc) = (eventName, clrType, payload, occurredOnUtc);
}
