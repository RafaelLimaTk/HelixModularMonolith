using Helix.Chat.Domain.Events.Conversation;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Application.EventHandlers.Conversation;
public sealed class ConversationCreatedEventHandler
    : IDomainEventHandler<ConversationCreated>
{
    private readonly IMessageProducer _bus;
    public ConversationCreatedEventHandler(IMessageProducer bus) => _bus = bus;

    public Task HandleAsync(ConversationCreated domainEvent, CancellationToken cancellationToken)
        => _bus.SendMessageAsync(domainEvent, cancellationToken);
}
