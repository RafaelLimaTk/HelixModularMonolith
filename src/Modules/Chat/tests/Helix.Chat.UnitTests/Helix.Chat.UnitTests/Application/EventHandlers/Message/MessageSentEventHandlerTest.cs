using Helix.Chat.Application.EventHandlers.Message;

namespace Helix.Chat.UnitTests.Application.EventHandlers.Message;
public class MessageSentEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "MessageSent - EventHandlers")]
    public async Task HandleAsync()
    {
        var messageProducerMock = new Mock<IMessageProducer>();
        messageProducerMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<MessageSent>(),
                It.IsAny<CancellationToken>()
             ))
            .Returns(Task.CompletedTask);
        var handler = new MessageSentEventHandler(messageProducerMock.Object);
        MessageSent @event = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hello, world!",
            DateTime.UtcNow
        );

        await handler.HandleAsync(@event, CancellationToken.None);

        messageProducerMock
            .Verify(x => x.SendMessageAsync(@event, CancellationToken.None),
                Times.Once);
    }
}
