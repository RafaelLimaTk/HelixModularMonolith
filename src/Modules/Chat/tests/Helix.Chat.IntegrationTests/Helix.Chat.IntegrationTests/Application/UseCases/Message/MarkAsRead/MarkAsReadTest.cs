using Helix.Chat.Domain.Enums;
using Shared.Domain.Exceptions;
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

    [Fact(DisplayName = nameof(MarkAsReadIsIdempotentAndDoesNotRaiseEventAgain))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task MarkAsReadIsIdempotentAndDoesNotRaiseEventAgain()
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
        message.MarkAsRead();
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
        output.Changed.Should().BeFalse();
        output.ReadAt.Should().NotBe(default);
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.IsAny<EventEnvelope>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.Status.Should().Be(MessageStatus.Read);
    }

    [Fact(DisplayName = nameof(ThrowWhenMessageNotFound))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
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
        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var nonExistentId = Guid.NewGuid();
        var input = new UseCase.MarkAsReadInput(
            nonExistentId,
            Guid.NewGuid()
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Message '{nonExistentId}' not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenReaderIsNotParticipant))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task ThrowWhenReaderIsNotParticipant()
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

        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var nonParticipantReaderId = Guid.NewGuid();
        var input = new UseCase.MarkAsReadInput(
            message.Id,
            nonParticipantReaderId
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("ReaderId must be a participant of the conversation");
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.Status.Should().Be(MessageStatus.Delivered);
        dbMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenMessageStatusIsSent))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task ThrowWhenMessageStatusIsSent()
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
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await messageRepository.Insert(message, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );
        var input = new UseCase.MarkAsReadInput(
            message.Id,
            readerId
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Cannot mark as read from Sent state");
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.Status.Should().Be(Domain.Enums.MessageStatus.Sent);
        dbMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(MultipleParticipantsMarkDifferentMessagesAsRead))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task MultipleParticipantsMarkDifferentMessagesAsRead()
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
        var participantIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var conversation = _fixture.GetExampleConversation(participantIds: participantIds);
        var messages = new List<DomainEntity.Message>();
        foreach (var senderId in participantIds)
        {
            var message = conversation.SendMessage(senderId, _fixture.GetValidContent());
            message.MarkAsDelivered();
            messages.Add(message);
        }

        await conversationRepository.Insert(conversation, CancellationToken.None);
        foreach (var message in messages)
        {
            await messageRepository.Insert(message, CancellationToken.None);
        }
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.MarkAsRead(
            messageRepository,
            conversationRepository,
            unitOfWork
        );

        for (int i = 0; i < messages.Count; i++)
        {
            var readerId = participantIds[(i + 1) % participantIds.Count];
            var input = new UseCase.MarkAsReadInput(
                messages[i].Id,
                readerId
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
        dbMessages.Should().OnlyContain(m => m.Status == MessageStatus.Read);
        dbMessages.Should().OnlyContain(m => m.ReadAt != null);
    }

    [Fact(DisplayName = nameof(TransitionFromDeliveredToRead))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
    public async Task TransitionFromDeliveredToRead()
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
        var initialDbContext = _fixture.CreateDbContext(true);
        var initialMessage = await initialDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        initialMessage.Status.Should().Be(MessageStatus.Delivered);
        initialMessage.DeliveredAt.Should().NotBeNull();
        initialMessage.ReadAt.Should().BeNull();
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

        output.MessageId.Should().Be(message.Id);
        output.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        output.Changed.Should().BeTrue();
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.Status.Should().Be(MessageStatus.Read);
        dbMessage.DeliveredAt.Should().NotBeNull();
        dbMessage.ReadAt.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(PreservesOriginalMessageData))]
    [Trait("Chat/Integration/Application", "MarkAsRead - Use Cases")]
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
        var readerId = Guid.NewGuid();
        var content = _fixture.GetValidContent();
        var conversation = _fixture.GetExampleConversation(
            participantIds: [senderId, readerId]
        );
        var message = conversation.SendMessage(senderId, content);
        var originalSentAt = message.SentAt;
        message.MarkAsDelivered();
        var originalDeliveredAt = message.DeliveredAt!.Value;
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

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id);
        dbMessage.ConversationId.Should().Be(conversation.Id);
        dbMessage.SenderId.Should().Be(senderId);
        dbMessage.Content.Should().Be(content);
        dbMessage.SentAt.Should().BeCloseTo(originalSentAt, TimeSpan.FromSeconds(1));
        dbMessage.DeliveredAt.Should().NotBeNull();
        dbMessage.DeliveredAt!.Value.Should().BeCloseTo(originalDeliveredAt, TimeSpan.FromSeconds(1));
        dbMessage.Status.Should().Be(Domain.Enums.MessageStatus.Read);
        dbMessage.ReadAt.Should().NotBeNull();
    }
}
