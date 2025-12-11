using Helix.Chat.Domain.Enums;
using Shared.Domain.Exceptions;
using UseCase = Helix.Chat.Application.UseCases.Conversation.SendMessage;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.SendMessage;

[Collection(nameof(SendMessageTestFixture))]
public class SendMessageTest(SendMessageTestFixture fixture)
{
    private readonly SendMessageTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(SendMessage))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task SendMessage()
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
        var conversation = _fixture.GetExampleConversation(participantIds: new List<Guid> { senderId });
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var content = _fixture.GetValidContent();
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            content
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().NotBeEmpty();
        output.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == output.MessageId);
        dbMessage.Should().NotBeNull();
        dbMessage!.ConversationId.Should().Be(conversation.Id);
        dbMessage.SenderId.Should().Be(senderId);
        dbMessage.Content.Should().Be(content);
        dbMessage.SentAt.Should().BeCloseTo(output.SentAt, TimeSpan.FromSeconds(1));
        dbMessage.Status.Should().Be(MessageStatus.Sent);
    }

    [Fact(DisplayName = nameof(SendMessageRaisesMessageSentEvent))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task SendMessageRaisesMessageSentEvent()
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
        var conversation = _fixture.GetExampleConversation(participantIds: [senderId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            _fixture.GetValidContent()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.Is<EventEnvelope>(e =>
                e.EventName == "MessageSent" &&
                e.ClrType.Contains("MessageSent") &&
                !string.IsNullOrWhiteSpace(e.Payload)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(SendMultipleMessages))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task SendMultipleMessages()
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
        var conversation = _fixture.GetExampleConversation(participantIds: new List<Guid> { senderId });
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );

        var messageIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var input = new UseCase.SendMessageInput(
                conversation.Id,
                senderId,
                _fixture.GetValidContent()
            );
            var output = await useCase.Handle(input, CancellationToken.None);
            messageIds.Add(output.MessageId);
        }

        var assertDbContext = _fixture.CreateDbContext(true);
        var messagesCount = await assertDbContext.Messages.CountAsync();
        messagesCount.Should().Be(5);
        var dbMessages = await assertDbContext.Messages
            .AsNoTracking()
            .ToListAsync();
        dbMessages.Select(m => m.Id).Should().BeEquivalentTo(messageIds);
        dbMessages.Should().OnlyContain(m => m.ConversationId == conversation.Id);
        dbMessages.Should().OnlyContain(m => m.SenderId == senderId);
    }

    [Fact(DisplayName = nameof(MultipleParticipantsSendMessages))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task MultipleParticipantsSendMessages()
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
        var participantIds = _fixture.GetParticipantIds(3);
        var conversation = _fixture.GetExampleConversation(participantIds: participantIds);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );

        foreach (var senderId in participantIds)
        {
            var input = new UseCase.SendMessageInput(
                conversation.Id,
                senderId,
                _fixture.GetValidContent()
            );
            await useCase.Handle(input, CancellationToken.None);
        }

        var assertDbContext = _fixture.CreateDbContext(true);
        var messagesCount = await assertDbContext.Messages.CountAsync();
        messagesCount.Should().Be(3);
        var dbMessages = await assertDbContext.Messages
            .AsNoTracking()
            .ToListAsync();
        dbMessages.Select(m => m.SenderId).Should().BeEquivalentTo(participantIds);
    }

    [Fact(DisplayName = nameof(ThrowWhenSenderIsNotParticipant))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task ThrowWhenSenderIsNotParticipant()
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
        var participantId = Guid.NewGuid();
        var conversation = _fixture.GetExampleConversation(participantIds: [participantId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var nonParticipantId = Guid.NewGuid();
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            nonParticipantId,
            _fixture.GetValidContent()
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("SenderId must be a participant of the conversation");

        var assertDbContext = _fixture.CreateDbContext(true);
        var messagesCount = await assertDbContext.Messages.CountAsync();
        messagesCount.Should().Be(0);
    }

    [Fact(DisplayName = nameof(ThrowWhenConversationNotFound))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task ThrowWhenConversationNotFound()
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
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var nonExistentId = Guid.NewGuid();
        var input = new UseCase.SendMessageInput(
            nonExistentId,
            Guid.NewGuid(),
            _fixture.GetValidContent()
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{nonExistentId}' not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenContentIsEmpty))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task ThrowWhenContentIsEmpty()
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
        var conversation = _fixture.GetExampleConversation(participantIds: [senderId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            string.Empty
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Content should not be null or empty");
        var assertDbContext = _fixture.CreateDbContext(true);
        var messagesCount = await assertDbContext.Messages.CountAsync();
        messagesCount.Should().Be(0);
    }

    [Theory(DisplayName = nameof(ThrowWhenContentIsGreaterThanMaxLength))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task ThrowWhenContentIsGreaterThanMaxLength(int excess)
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
        var conversation = _fixture.GetExampleConversation(participantIds: [senderId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);

        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var maxLength = DomainEntity.Message.MAX_LENGTH;
        var invalidContent = _fixture.GetLongContent(maxLength + excess);
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            invalidContent
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage($"Content should be at most {maxLength} characters long");

        var assertDbContext = _fixture.CreateDbContext(true);
        var messagesCount = await assertDbContext.Messages.CountAsync();
        messagesCount.Should().Be(0);
    }

    [Fact(DisplayName = nameof(SendMessageWithContentEqualToMaxLength))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task SendMessageWithContentEqualToMaxLength()
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
        var conversation = _fixture.GetExampleConversation(participantIds: [senderId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var maxLength = DomainEntity.Message.MAX_LENGTH;
        var content = _fixture.GetLongContent(maxLength)[..maxLength];
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            content
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == output.MessageId);
        dbMessage.Should().NotBeNull();
        dbMessage!.Content.Should().Be(content);
        dbMessage.Content.Length.Should().Be(maxLength);
    }

    [Fact(DisplayName = nameof(SendMessageTrimsContent))]
    [Trait("Chat/Integration/Application", "SendMessage - Use Cases")]
    public async Task SendMessageTrimsContent()
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
        var conversation = _fixture.GetExampleConversation(participantIds: [senderId]);
        await conversationRepository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.SendMessage(
            conversationRepository,
            messageRepository,
            unitOfWork
        );
        var contentWithSpaces = $"  {_fixture.GetValidContent()}  ";
        var expectedContent = contentWithSpaces.Trim();
        var input = new UseCase.SendMessageInput(
            conversation.Id,
            senderId,
            contentWithSpaces
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == output.MessageId);
        dbMessage.Should().NotBeNull();
        dbMessage!.Content.Should().Be(expectedContent);
        dbMessage.Content.Should().NotStartWith(" ");
        dbMessage.Content.Should().NotEndWith(" ");
    }
}
