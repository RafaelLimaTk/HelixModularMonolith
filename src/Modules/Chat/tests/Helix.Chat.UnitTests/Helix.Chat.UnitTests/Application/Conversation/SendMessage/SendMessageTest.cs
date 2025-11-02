using Helix.Chat.Application.UseCases.Conversation.SendMessage;
using Helix.Chat.Domain.Enums;
using Helix.Chat.UnitTests.Extensions.DateTime;
using UseCase = Helix.Chat.Application.UseCases.Conversation.SendMessage;

namespace Helix.Chat.UnitTests.Application.Conversation.SendMessage;

[Collection(nameof(SendMessageTestFixture))]
public class SendMessageTest(SendMessageTestFixture fixture)
{
    private readonly SendMessageTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(SendMessage))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    public async Task SendMessage()
    {
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var onlyParticipant = listParticipants.First();
        var request = new SendMessageInput(
            exampleConversation.Id,
            onlyParticipant,
            _fixture.GetValidContent()
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        var response = await useCase.Handle(request, CancellationToken.None);

        response.Should().NotBeNull();
        response.MessageId.Should().NotBeEmpty();
        response.SentAt.Should().BeCloseTo(DateTime.UtcNow.TrimMilliseconds(), TimeSpan.FromSeconds(2));
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once
        );
        conversationRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(id => id == request.ConversationId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        messageRepositoryMock.Verify(
            x => x.Insert(
                It.Is<DomainEntity.Message>(m =>
                    m.ConversationId == request.ConversationId &&
                    m.SenderId == request.SenderId &&
                    m.Content == request.Content
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ThrowIfNotFoundConversation))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    public async Task ThrowIfNotFoundConversation()
    {
        var exampleId = Guid.NewGuid();
        var request = new SendMessageInput(
            ConversationId: exampleId,
            SenderId: Guid.NewGuid(),
            _fixture.GetValidContent()
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == request.ConversationId),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new NotFoundException($"Conversation '{exampleId}' not found."));
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        Func<Task> act = async ()
            => await useCase.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{exampleId}' not found.");
        conversationRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(id => id == request.ConversationId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        messageRepositoryMock.Verify(
            x => x.Insert(
                It.IsAny<DomainEntity.Message>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact(DisplayName = nameof(ThrowIfSenderIsNotParticipant))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    public async Task ThrowIfSenderIsNotParticipant()
    {
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var nonParticipantSenderId = Guid.NewGuid();
        while (listParticipants.Contains(nonParticipantSenderId))
            nonParticipantSenderId = Guid.NewGuid();
        var request = new SendMessageInput(
            exampleConversation.Id,
            nonParticipantSenderId,
            _fixture.GetValidContent()
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == request.ConversationId),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        Func<Task> act = async ()
            => await useCase.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("SenderId must be a participant of the conversation");

        conversationRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(id => id == request.ConversationId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        messageRepositoryMock.Verify(
            x => x.Insert(
                It.IsAny<DomainEntity.Message>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory(DisplayName = nameof(ThrowIfContentIsGreaterThanMaxLength))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    [MemberData(nameof(GetContentGreaterThanMaxLength), parameters: 6)]
    public async Task ThrowIfContentIsGreaterThanMaxLength(string tooLongContent)
    {
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var sender = listParticipants.First();
        var request = new SendMessageInput(
            exampleConversation.Id,
            sender,
            tooLongContent
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == request.ConversationId),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        Func<Task> act = async ()
            => await useCase.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<EntityValidationException>()
            .WithMessage($"Content should be at most {DomainEntity.Message.MAX_LENGTH} characters long");

        conversationRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(id => id == request.ConversationId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        messageRepositoryMock.Verify(
            x => x.Insert(
                It.IsAny<DomainEntity.Message>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    public static IEnumerable<object[]> GetContentGreaterThanMaxLength(int numberOfTests = 6)
    {
        var fixture = new SendMessageTestFixture();
        var rnd = new Random();

        for (int testIndex = 0; testIndex < numberOfTests; testIndex++)
        {
            var extra = (testIndex == 0) ? 1 : rnd.Next(1, 128);
            var len = DomainEntity.Message.MAX_LENGTH + extra;
            yield return new object[] { fixture.GetLongContent(len) };
        }
    }

    [Fact(DisplayName = nameof(InsertedMessageHasSentStatus))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    public async Task InsertedMessageHasSentStatus()
    {
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var sender = listParticipants.First();
        var request = new SendMessageInput(
            exampleConversation.Id,
            sender,
            _fixture.GetValidContent()
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == request.ConversationId),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        DomainEntity.Message? capturedMessage = null;
        messageRepositoryMock.Setup(x => x.Insert(
            It.IsAny<DomainEntity.Message>(),
            It.IsAny<CancellationToken>()
        )).Callback<DomainEntity.Message, CancellationToken>((m, ct) => capturedMessage = m)
          .Returns(Task.CompletedTask);
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        var response = await useCase.Handle(request, CancellationToken.None);

        capturedMessage.Should().NotBeNull();
        capturedMessage.Id.Should().Be(response.MessageId);
        capturedMessage.ConversationId.Should().Be(exampleConversation.Id);
        capturedMessage.SenderId.Should().Be(request.SenderId);
        capturedMessage.Content.Should().Be(request.Content);
        capturedMessage.Status.Should().Be(MessageStatus.Sent);

        messageRepositoryMock.Verify(
            x => x.Insert(
                It.IsAny<DomainEntity.Message>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }


    [Fact(DisplayName = nameof(SendMessageRaiseMessageSentDomainEvent))]
    [Trait("Chat/Application", "SendMessage - Use Cases")]
    public async Task SendMessageRaiseMessageSentDomainEvent()
    {
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var sender = listParticipants.First();
        var request = new SendMessageInput(
            exampleConversation.Id,
            sender,
            _fixture.GetValidContent()
        );
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == request.ConversationId),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.SendMessage(
            conversationRepositoryMock.Object,
            messageRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        var before = DateTime.UtcNow.TrimMilliseconds();
        var response = await useCase.Handle(request, CancellationToken.None);
        var after = DateTime.UtcNow.TrimMilliseconds();

        response.Should().NotBeNull();
        response.MessageId.Should().NotBeEmpty();
        response.SentAt.TrimMilliseconds().Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        exampleConversation.Events.Should().NotBeNull();
        exampleConversation.Events.OfType<MessageSent>().Should().HaveCount(1);
        var @event = exampleConversation.Events.OfType<MessageSent>().First();
        @event.MessageId.Should().Be(response.MessageId);
        @event.ConversationId.Should().Be(exampleConversation.Id);
        @event.SenderId.Should().Be(sender);
        @event.Content.Should().Be(request.Content);
        @event.SentAt.TrimMilliseconds().Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        conversationRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(id => id == request.ConversationId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        messageRepositoryMock.Verify(
            x => x.Insert(
                It.Is<DomainEntity.Message>(m => m.Id == response.MessageId),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
