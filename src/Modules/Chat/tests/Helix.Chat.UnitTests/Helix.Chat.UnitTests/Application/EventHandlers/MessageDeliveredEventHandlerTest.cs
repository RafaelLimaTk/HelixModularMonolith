using Helix.Chat.Application.EventHandlers.Message;

namespace Helix.Chat.UnitTests.Application.EventHandlers;
public class MessageDeliveredEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "MessageDelivered - EventHandlers")]
    public async Task HandleAsync()
    {
        var messageProducerMock = new Mock<IMessageProducer>();
        messageProducerMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<MessageDelivered>(),
                It.IsAny<CancellationToken>()
             ))
            .Returns(Task.CompletedTask);
        var handler = new MessageDeliveredEventHandler(messageProducerMock.Object);
        MessageDelivered @event = new(
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
