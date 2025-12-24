namespace Shared.Infra.Outbox.Configuration;

public sealed class OutboxProcessorOptions
{
    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public int EmptyQueueDelaySeconds { get; set; } = 5;
    public int PartialBatchDelaySeconds { get; set; } = 1;
    public int ErrorRetryDelaySeconds { get; set; } = 10;
    public int MessageTimeoutSeconds { get; set; } = 30;
    public bool EnableParallelProcessing { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 10;
}
