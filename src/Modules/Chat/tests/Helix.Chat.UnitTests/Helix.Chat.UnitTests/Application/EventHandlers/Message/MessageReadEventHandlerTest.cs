using Helix.Chat.Application.EventHandlers.Message;

namespace Helix.Chat.UnitTests.Application.EventHandlers.Message;
public class MessageReadEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "MessageRead - EventHandlers")]
    public async Task HandleAsync()
    {
        var messageProducerMock = new Mock<IMessageProducer>();
        messageProducerMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<MessageRead>(),
                It.IsAny<CancellationToken>()
             ))
            .Returns(Task.CompletedTask);
        var handler = new MessageReadEventHandler(messageProducerMock.Object);
        MessageRead @event = new(
            Guid.NewGuid(),
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
