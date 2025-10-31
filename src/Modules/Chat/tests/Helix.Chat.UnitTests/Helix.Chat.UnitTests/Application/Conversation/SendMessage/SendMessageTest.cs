using Helix.Chat.Application.UseCases.Conversation.SendMessage;
using Helix.Chat.Domain.Entities;
using Helix.Chat.UnitTests.Extensions.DateTime;
using UseCase = Helix.Chat.Application.UseCases.Conversation.SendMessage;

namespace Helix.Chat.UnitTests.Application.Conversation.SendMessage;
public class SendMessageTest(SendMessageTestFixture fixture) : IClassFixture<SendMessageTestFixture>
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
                It.Is<Message>(m =>
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
                It.IsAny<Message>(),
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
                It.IsAny<Message>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
