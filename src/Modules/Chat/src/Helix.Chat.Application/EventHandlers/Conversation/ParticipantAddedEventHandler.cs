using Helix.Chat.Domain.Events.Conversation;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Application.EventHandlers.Conversation;
public sealed class ParticipantAddedEventHandler
    : IDomainEventHandler<ParticipantAdded>
{
    private readonly IMessageProducer _bus;
    public ParticipantAddedEventHandler(IMessageProducer bus) => _bus = bus;

    public Task HandleAsync(ParticipantAdded domainEvent, CancellationToken cancellationToken)
        => _bus.SendMessageAsync(domainEvent, cancellationToken);
}
