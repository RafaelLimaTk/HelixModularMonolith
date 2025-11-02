using Helix.Chat.Domain.Events.Message;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Application.EventHandlers.Message;
public sealed class MessageSentEventHandler : IDomainEventHandler<MessageSent>
{
    private readonly IMessageProducer _bus;
    public MessageSentEventHandler(IMessageProducer bus) => _bus = bus;

    public Task HandleAsync(MessageSent domainEvent, CancellationToken cancellationToken)
        => _bus.SendMessageAsync(domainEvent, cancellationToken);
}
