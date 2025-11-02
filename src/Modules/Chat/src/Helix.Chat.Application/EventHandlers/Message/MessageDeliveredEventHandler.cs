using Helix.Chat.Domain.Events.Message;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Application.EventHandlers.Message;
public sealed class MessageDeliveredEventHandler : IDomainEventHandler<MessageDelivered>
{
    private readonly IMessageProducer _bus;
    public MessageDeliveredEventHandler(IMessageProducer bus) => _bus = bus;

    public Task HandleAsync(MessageDelivered domainEvent, CancellationToken cancellationToken)
        => _bus.SendMessageAsync(domainEvent, cancellationToken);
}
