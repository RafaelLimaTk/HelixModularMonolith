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

    [Fact(DisplayName = nameof(DoesNotChangeWhenAlreadyRead))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task DoesNotChangeWhenAlreadyRead()
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
            participantIds: new List<Guid> { senderId }
        );
        var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        message.MarkAsRead();
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
        dbMessage.Status.Should().Be(MessageStatus.Read);
        dbMessage.DeliveredAt.Should().NotBeNull();
        dbMessage.ReadAt.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenMessageNotFound))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task ThrowWhenMessageNotFound()
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
        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var nonExistentId = Guid.NewGuid();
        var input = new UseCase.MarkAsDeliveredInput(
            nonExistentId
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Message '{nonExistentId}' not found.");
    }

    [Fact(DisplayName = nameof(MarkMultipleMessagesAsDelivered))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task MarkMultipleMessagesAsDelivered()
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
        var messages = Enumerable.Range(0, 5)
            .Select(_ => conversation.SendMessage(senderId, _fixture.GetValidContent()))
            .ToList();
        await conversationRepository.Insert(conversation, CancellationToken.None);
        foreach (var message in messages)
        {
            await messageRepository.Insert(message, CancellationToken.None);
        }
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );

        foreach (var message in messages)
        {
            var input = new UseCase.MarkAsDeliveredInput(
                message.Id
            );
            var output = await useCase.Handle(input, CancellationToken.None);
            output.Changed.Should().BeTrue();
        }

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessages = await assertDbContext.Messages
            .AsNoTracking()
            .Where(m => messages.Select(msg => msg.Id).Contains(m.Id))
            .ToListAsync();
        dbMessages.Should().HaveCount(5);
        dbMessages.Should().OnlyContain(m => m.Status == MessageStatus.Delivered);
        dbMessages.Should().OnlyContain(m => m.DeliveredAt != null);
        dbMessages.Should().OnlyContain(m => m.ReadAt == null);
    }

    [Fact(DisplayName = nameof(PreservesOriginalMessageData))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task PreservesOriginalMessageData()
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
        var content = _fixture.GetValidContent();
        var conversation = _fixture.GetExampleConversation(
            participantIds: new List<Guid> { senderId }
        );
        var message = conversation.SendMessage(senderId, content);
        var originalSentAt = message.SentAt;
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

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.ConversationId.Should().Be(conversation.Id);
        dbMessage.SenderId.Should().Be(senderId);
        dbMessage.Content.Should().Be(content);
        dbMessage.SentAt.Should().BeCloseTo(originalSentAt, TimeSpan.FromSeconds(1));
        dbMessage.Status.Should().Be(MessageStatus.Delivered);
        dbMessage.DeliveredAt.Should().NotBeNull();
        dbMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(MultipleConversationsMarkMessagesIndependently))]
    [Trait("Chat/Integration/Application", "MarkAsDelivered - Use Cases")]
    public async Task MultipleConversationsMarkMessagesIndependently()
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
        var conversations = Enumerable.Range(0, 3)
            .Select(_ =>
            {
                var senderId = Guid.NewGuid();
                var conv = _fixture.GetExampleConversation(participantIds: [senderId]);
                return (Conversation: conv, SenderId: senderId);
            })
            .ToList();
        var messages = new List<DomainEntity.Message>();
        foreach (var (conv, senderId) in conversations)
        {
            var message = conv.SendMessage(senderId, _fixture.GetValidContent());
            messages.Add(message);
            await conversationRepository.Insert(conv, CancellationToken.None);
            await messageRepository.Insert(message, CancellationToken.None);
        }
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsDelivered(
            messageRepository,
            conversationRepository,
            unitOfWork
        );

        foreach (var message in messages)
        {
            var input = new UseCase.MarkAsDeliveredInput(
                message.Id
            );
            var output = await useCase.Handle(input, CancellationToken.None);
            output.Changed.Should().BeTrue();
        }

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessages = await assertDbContext.Messages
            .AsNoTracking()
            .Where(m => messages.Select(msg => msg.Id).Contains(m.Id))
            .ToListAsync();
        dbMessages.Should().HaveCount(3);
        dbMessages.Should().OnlyContain(m => m.Status == MessageStatus.Delivered);
        var conversationIds = dbMessages.Select(m => m.ConversationId).Distinct().ToList();
        conversationIds.Should().HaveCount(3);
    }
}
