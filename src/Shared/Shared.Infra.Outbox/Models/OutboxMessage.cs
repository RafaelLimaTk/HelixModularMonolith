namespace Shared.Infra.Outbox.Models;
public sealed class OutboxMessage
{
    public long Id { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Type { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public int Attempts { get; private set; }
    public string? Error { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload, DateTime occurredOnUtc)
        => new() { Type = type, Payload = payload, OccurredOnUtc = occurredOnUtc };

    public void MarkProcessed() => ProcessedOnUtc = DateTime.UtcNow;
    public void MarkFailed(string error) { Attempts++; Error = error; }
}