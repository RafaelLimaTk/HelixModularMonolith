using Helix.Chat.Application.EventHandlers.Conversation;
using Helix.Chat.Domain.Events.Conversation;

namespace Helix.Chat.UnitTests.Application.EventHandlers.Conversation;
public class ConversationCreatedEventHandlerTest
{
    [Fact(DisplayName = nameof(HandleAsync))]
    [Trait("Chat/Application", "ConversationCreated - EventHandlers")]
    public async Task HandleAsync()
    {
        var messageProducerMock = new Mock<IMessageProducer>();
        messageProducerMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<ConversationCreated>(),
                It.IsAny<CancellationToken>()
             ))
            .Returns(Task.CompletedTask);
        var handler = new ConversationCreatedEventHandler(messageProducerMock.Object);
        ConversationCreated @event = new(
            Guid.NewGuid(),
            "Test Conversation",
            DateTime.UtcNow
        );

        await handler.HandleAsync(@event, CancellationToken.None);

        messageProducerMock
            .Verify(x => x.SendMessageAsync(@event, CancellationToken.None),
                Times.Once);
    }
}
