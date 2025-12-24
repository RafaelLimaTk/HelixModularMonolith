using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.SeedWorks;
using Shared.Infra.Outbox.Configuration;
using Shared.Infra.Outbox.Interfaces;
using Shared.Infra.Outbox.Models;
using System.Diagnostics;
using System.Text.Json;

public sealed class OutboxProcessor : BackgroundService, IOutboxProcessor
{
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDomainEventPublisher _publisher;
    private readonly OutboxProcessorOptions _options;
    private readonly ITypeResolver _typeResolver;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IServiceScopeFactory scopeFactory,
        IDomainEventPublisher publisher,
        OutboxProcessorOptions options,
        ITypeResolver typeResolver)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _options = options;
        _typeResolver = typeResolver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Outbox processor is disabled");
            return;
        }

        _logger.LogInformation(
            "Outbox processor started. BatchSize: {BatchSize}, EmptyQueueDelay: {EmptyDelay}s, ParallelProcessing: {Parallel}",
            _options.BatchSize,
            _options.EmptyQueueDelaySeconds,
            _options.EnableParallelProcessing);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox processor shutting down gracefully");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in outbox processor loop");
                await Task.Delay(TimeSpan.FromSeconds(_options.ErrorRetryDelaySeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing pending outbox messages manually");

        bool hasMore;
        do
        {
            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var messages = await store.TakeBatchAsync(_options.BatchSize, cancellationToken);

            if (messages.Count == 0)
                break;

            if (_options.EnableParallelProcessing)
            {
                await ProcessMessagesInParallelAsync(messages, store, cancellationToken);
            }
            else
            {
                await ProcessMessagesSequentiallyAsync(messages, store, cancellationToken);
            }

            hasMore = messages.Count == _options.BatchSize;
        } while (hasMore);

        _logger.LogInformation("Finished processing pending outbox messages");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

        var stopwatch = Stopwatch.StartNew();
        var messages = await store.TakeBatchAsync(_options.BatchSize, cancellationToken);
        stopwatch.Stop();

        if (messages.Count == 0)
        {
            _logger.LogDebug("No messages in outbox, waiting {Delay}s", _options.EmptyQueueDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_options.EmptyQueueDelaySeconds), cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Retrieved {Count} messages from outbox in {ElapsedMs}ms",
            messages.Count,
            stopwatch.ElapsedMilliseconds);

        if (_options.EnableParallelProcessing)
        {
            await ProcessMessagesInParallelAsync(messages, store, cancellationToken);
        }
        else
        {
            await ProcessMessagesSequentiallyAsync(messages, store, cancellationToken);
        }

        if (messages.Count < _options.BatchSize)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.PartialBatchDelaySeconds), cancellationToken);
        }
    }

    private async Task ProcessMessagesSequentiallyAsync(
        IReadOnlyList<OutboxMessage> messages,
        IOutboxStore store,
        CancellationToken cancellationToken)
    {
        var processed = 0;
        var failed = 0;

        foreach (var message in messages)
        {
            var result = await ProcessSingleMessageAsync(message, store, cancellationToken);
            if (result) processed++; else failed++;
        }

        _logger.LogInformation(
            "Batch completed: {Processed} processed, {Failed} failed",
            processed,
            failed);
    }

    private async Task ProcessMessagesInParallelAsync(
        IReadOnlyList<OutboxMessage> messages,
        IOutboxStore store,
        CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(_options.MaxDegreeOfParallelism);
        var processedCount = 0;
        var failedCount = 0;
        var countLock = new object();

        var tasks = messages.Select(async message =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ProcessSingleMessageAsync(message, store, cancellationToken);

                lock (countLock)
                {
                    if (result) processedCount++; else failedCount++;
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Parallel batch completed: {Processed} processed, {Failed} failed",
            processedCount,
            failedCount);
    }

    private async Task<bool> ProcessSingleMessageAsync(
        OutboxMessage message,
        IOutboxStore store,
        CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.MessageTimeoutSeconds));

            var eventType = _typeResolver.ResolveType(message.Type);
            if (eventType is null)
            {
                _logger.LogWarning(
                    "Failed to resolve type '{TypeName}' for message {MessageId}",
                    message.Type,
                    message.Id);

                await store.MarkFailedAsync(
                    message.Id,
                    $"Type '{message.Type}' not found or could not be resolved",
                    cancellationToken);

                return false;
            }

            var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
            if (domainEvent is null)
            {
                _logger.LogWarning(
                    "Deserialization returned null for message {MessageId}",
                    message.Id);

                await store.MarkFailedAsync(
                    message.Id,
                    "Deserialization returned null",
                    cancellationToken);

                return false;
            }

            if (domainEvent is not DomainEvent typedEvent)
            {
                _logger.LogWarning(
                    "Deserialized object is not an IDomainEvent for message {MessageId}",
                    message.Id);

                await store.MarkFailedAsync(
                    message.Id,
                    $"Type '{eventType.Name}' does not implement IDomainEvent",
                    cancellationToken);

                return false;
            }

            await _publisher.PublishAsync(typedEvent, cts.Token);
            await store.MarkProcessedAsync(message.Id, cancellationToken);

            _logger.LogDebug(
                "Successfully processed message {MessageId} of type {EventType}",
                message.Id,
                eventType.Name);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Processing of message {MessageId} was cancelled due to shutdown",
                message.Id);
            throw;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(
                ex,
                "Timeout processing message {MessageId} after {Timeout}s",
                message.Id,
                _options.MessageTimeoutSeconds);

            await store.MarkFailedAsync(
                message.Id,
                $"Processing timeout after {_options.MessageTimeoutSeconds}s: {ex.Message}",
                cancellationToken);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing outbox message {MessageId}: {ErrorMessage}",
                message.Id,
                ex.Message);

            await store.MarkFailedAsync(
                message.Id,
                $"{ex.GetType().Name}: {ex.Message}",
                cancellationToken);

            return false;
        }
    }
}