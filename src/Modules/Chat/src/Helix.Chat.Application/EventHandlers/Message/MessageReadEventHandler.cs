using Helix.Chat.Domain.Events.Message;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Application.EventHandlers.Message;
public sealed class MessageReadEventHandler : IDomainEventHandler<MessageRead>
{
    private readonly IMessageProducer _bus;
    public MessageReadEventHandler(IMessageProducer bus) => _bus = bus;

    public Task HandleAsync(MessageRead domainEvent, CancellationToken cancellationToken)
        => _bus.SendMessageAsync(domainEvent, cancellationToken);
}
