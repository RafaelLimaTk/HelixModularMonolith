using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.SeedWorks;
using Shared.Infra.Outbox.Interfaces;
using System.Text.Json;

namespace Shared.Infra.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDomainEventPublisher _publisher;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IServiceScopeFactory scopeFactory,
        IDomainEventPublisher publisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

                var messages = await store.TakeBatchAsync(100, stoppingToken);
                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.Type, throwOnError: false);
                        if (eventType is null)
                        {
                            await store.MarkFailedAsync(
                                message.Id,
                                $"Type '{message.Type}' not found.",
                                stoppingToken);

                            continue;
                        }

                        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
                        if (domainEvent is null)
                        {
                            await store.MarkFailedAsync(
                                message.Id,
                                "Deserialization returned null.",
                                stoppingToken);

                            continue;
                        }

                        await _publisher.PublishAsync((dynamic)domainEvent, stoppingToken);
                        await store.MarkProcessedAsync(message.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Outbox: error processing message {MessageId}.",
                            message.Id);
                        await store.MarkFailedAsync(message.Id, ex.Message, stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox: processor canceled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
