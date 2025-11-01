using Helix.Chat.Application.EventHandlers;
using Helix.Chat.Domain.Events.Conversation;

namespace Helix.Chat.UnitTests.Application.EventHandlers;
public class MessageSentEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "EventHandlers")]
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
