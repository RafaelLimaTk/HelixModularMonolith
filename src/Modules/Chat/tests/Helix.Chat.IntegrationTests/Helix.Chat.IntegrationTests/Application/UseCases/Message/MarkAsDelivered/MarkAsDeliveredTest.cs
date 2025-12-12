using Helix.Chat.Domain.Enums;
using UseCase = Helix.Chat.Application.UseCases.Message.MarkAsDelivered;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Message.MarkAsDelivered;

[Collection(nameof(MarkAsDeliveredTestFixture))]
public class MarkAsDeliveredTest(MarkAsDeliveredTestFixture fixture)
{
    private readonly MarkAsDeliveredTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(MarkMessageAsDelivered))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task MarkMessageAsDelivered()
    {
        var dbContext = _fixture.CreateDbContext();
        var conversationRepository = new Repository.ConversationRepository(dbContext);
        var messageRepository = new Repository.MessageRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var senderId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId]
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();

        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsDeliveredInput(
            message.Id
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.Changed.Should().BeTrue();
        output.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == message.Id);
        dbMessage.Should().NotBeNull();
        dbMessage!.Status.Should().Be(MessageStatus.Delivered);
        dbMessage.DeliveredAt.Should().NotBeNull();
        dbMessage.DeliveredAt!.Value.Should().BeCloseTo(output.DeliveredAt, TimeSpan.FromSeconds(1));
        dbMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(MarkAsDeliveredRaisesMessageDeliveredEvent))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task MarkAsDeliveredRaisesMessageDeliveredEvent()
    {
        var dbContext = _fixture.CreateDbContext();
        var conversationRepository = new Repository.ConversationRepository(dbContext);
        var messageRepository = new Repository.MessageRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var senderId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId]
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsDeliveredInput(
            message.Id
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Changed.Should().BeTrue();
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.Is<EventEnvelope>(e =>
                e.EventName == "MessageDelivered" &&
                e.ClrType.Contains("MessageDelivered") &&
                !string.IsNullOrWhiteSpace(e.Payload)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(MarkAsDeliveredIsIdempotentAndDoesNotRaiseEventAgain))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task MarkAsDeliveredIsIdempotentAndDoesNotRaiseEventAgain()
    {
        var dbContext = _fixture.CreateDbContext();
        var conversationRepository = new Repository.ConversationRepository(dbContext);
        var messageRepository = new Repository.MessageRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var senderId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId]
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsDeliveredInput(
            message.Id
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.Changed.Should().BeFalse();
        output.DeliveredAt.Should().Be(message.DeliveredAt);
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.IsAny<EventEnvelope>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.Status.Should().Be(MessageStatus.Delivered);
    }
}
