using Helix.Chat.Application.EventHandlers.Conversation;
using Helix.Chat.Domain.Events.Conversation;

namespace Helix.Chat.UnitTests.Application.EventHandlers.Conversation;
public class ParticipantAddedEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "ParticipantAdded - EventHandlers")]
    public async Task HandleAsync()
    {
        var messageProducerMock = new Mock<IMessageProducer>();
        messageProducerMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<ParticipantAdded>(),
                It.IsAny<CancellationToken>()
             ))
            .Returns(Task.CompletedTask);
        var handler = new ParticipantAddedEventHandler(messageProducerMock.Object);
        ParticipantAdded @event = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow
        );

        await handler.HandleAsync(@event, CancellationToken.None);

        messageProducerMock
            .Verify(x => x.SendMessageAsync(@event, CancellationToken.None),
                Times.Once);
    }
}
