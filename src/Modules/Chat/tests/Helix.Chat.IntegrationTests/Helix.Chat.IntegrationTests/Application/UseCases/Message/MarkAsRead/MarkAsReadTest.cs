using UseCase = Helix.Chat.Application.UseCases.Message.MarkAsRead;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Message.MarkAsRead;

[Collection(nameof(MarkAsReadTestFixture))]
public class MarkAsReadTest(MarkAsReadTestFixture fixture)
{
    private readonly MarkAsReadTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(MarkMessageAsRead))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task MarkMessageAsRead()
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
        var readerId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId, readerId]
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsReadInput(
            message.Id,
            readerId
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.Changed.Should().BeTrue();
        output.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == message.Id);
        dbMessage.Should().NotBeNull();
        dbMessage!.Status.Should().Be(Domain.Enums.MessageStatus.Read);
        dbMessage.ReadAt.Should().NotBeNull();
        dbMessage.ReadAt!.Value.Should().BeCloseTo(output.ReadAt, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = nameof(MarkAsReadRaisesMessageReadEvent))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task MarkAsReadRaisesMessageReadEvent()
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
        var readerId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId, readerId]
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsReadInput(
            message.Id,
            readerId
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Changed.Should().BeTrue();
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.Is<EventEnvelope>(e =>
                e.EventName == "MessageRead" &&
                e.ClrType.Contains("MessageRead") &&
                !string.IsNullOrWhiteSpace(e.Payload)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
