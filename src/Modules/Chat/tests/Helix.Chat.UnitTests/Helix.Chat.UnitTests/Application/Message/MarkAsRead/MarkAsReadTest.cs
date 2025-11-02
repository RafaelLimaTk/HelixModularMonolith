using Helix.Chat.Application.UseCases.Message.MarkAsRead;
using Helix.Chat.Domain.Enums;
using Helix.Chat.Domain.Events.Conversation;
using UseCase = Helix.Chat.Application.UseCases.Message.MarkAsRead;

namespace Helix.Chat.UnitTests.Application.Message.MarkAsRead;

[Collection(nameof(MarkAsReadTestFixture))]
public class MarkAsReadTest(MarkAsReadTestFixture fixture)
{
    private readonly MarkAsReadTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(MarkAsReadPersistsAndRaisesEvent))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task MarkAsReadPersistsAndRaisesEvent()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var readerId = listParticipantIds.Last();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        messageRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == message.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(message);
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            message.Id,
            readerId
        );

        var before = DateTime.UtcNow;
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow;

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.ReadAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        output.Changed.Should().BeTrue();
        message.SenderId.Should().Be(senderId);
        message.Status.Should().Be(MessageStatus.Read);
        message.DeliveredAt.Should().NotBeNull();
        message.ReadAt.Should().NotBeNull();
        exampleConversation.Events.Should().NotBeEmpty();
        exampleConversation.Events.OfType<MessageRead>().Should().NotBeEmpty();
        messageRepositoryMock.Verify(x => x.Update(
            It.Is<DomainEntity.Message>(m =>
                m.Id == message.Id
                && m.Status == MessageStatus.Read
                && m.ReadAt >= before && m.ReadAt <= after
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        conversationRepositoryMock.Verify(x => x.Update(
            It.Is<DomainEntity.Conversation>(c =>
                c.Id == exampleConversation.Id
                && c.Participants.Any(p =>
                    p.UserId == readerId
                )
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenMessageNotFound))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task ThrowWhenMessageNotFound()
    {
        var exampleMessageId = Guid.NewGuid();
        var exampleReaderId = Guid.NewGuid();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        messageRepositoryMock.Setup(x => x.Get(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new NotFoundException($"Message '{exampleMessageId}' not found."));
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            exampleMessageId,
            exampleReaderId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Message '{exampleMessageId}' not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenConversationNotFound))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task ThrowWhenConversationNotFound()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var readerId = listParticipantIds.Last();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        messageRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == message.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(message);
        conversationRepositoryMock.Setup(x => x.Get(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new NotFoundException($"Conversation '{exampleConversation.Id}' not found."));
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            message.Id,
            readerId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{exampleConversation.Id}' not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenReaderIsNotParticipant))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task ThrowWhenReaderIsNotParticipant()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var invalidReaderId = Guid.NewGuid();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        messageRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == message.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(message);
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            message.Id,
            invalidReaderId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("ReaderId must be a participant of the conversation");
    }

    [Fact(DisplayName = nameof(ThrowWhenMarkAsReadFromSentState))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task ThrowWhenMarkAsReadFromSentState()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var readerId = listParticipantIds.Last();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        messageRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == message.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(message);
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            message.Id,
            readerId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Cannot mark as read from Sent state");
    }

    [Fact(DisplayName = nameof(DoesNotChangeStatusWhenAlreadyRead))]
    [Trait("Chat/Application", "MarkAsRead - UseCase")]
    public async Task DoesNotChangeStatusWhenAlreadyRead()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var readerId = listParticipantIds.Last();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        message.MarkAsDelivered();
        message.MarkAsRead();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        messageRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == message.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(message);
        var useCase = new UseCase.MarkAsRead(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsReadInput(
            message.Id,
            readerId
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.ReadAt.Should().Be(message.ReadAt);
        output.Changed.Should().BeFalse();
        exampleConversation.Events.OfType<MessageRead>().Should().BeEmpty();
        messageRepositoryMock.Verify(x => x.Update(
            It.IsAny<DomainEntity.Message>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        conversationRepositoryMock.Verify(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
