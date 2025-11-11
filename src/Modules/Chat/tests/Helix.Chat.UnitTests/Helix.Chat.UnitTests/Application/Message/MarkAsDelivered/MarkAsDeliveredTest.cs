using Helix.Chat.Application.UseCases.Message.MarkAsDelivered;
using Helix.Chat.Domain.Enums;
using UseCase = Helix.Chat.Application.UseCases.Message.MarkAsDelivered;

namespace Helix.Chat.UnitTests.Application.Message.MarkAsDelivered;

[Collection(nameof(MarkAsDeliveredTestFixture))]
public class MarkAsDeliveredTest
{
    private readonly MarkAsDeliveredTestFixture _fixture;

    public MarkAsDeliveredTest(MarkAsDeliveredTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(MarkAsDeliveredPersistsAndReturnsChangedTrue))]
    [Trait("Chat/Application", "MarkAsDelivered - Use Cases")]
    public async Task MarkAsDeliveredPersistsAndReturnsChangedTrue()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var exampleConversation = _fixture.GetConversationExample(userIds: _fixture.GetParticipantIds());
        var senderId = exampleConversation.Participants.First().UserId;
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        messageRepositoryMock.Setup(x => x.Get(
                It.Is<Guid>(id => id == message.Id),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(message);
        conversationRepositoryMock.Setup(x => x.Get(
                It.Is<Guid>(id => id == exampleConversation.Id),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.MarkAsDelivered(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsDeliveredInput(message.Id);

        var before = DateTime.UtcNow;
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow;

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.Changed.Should().BeTrue();
        output.DeliveredAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        message.Status.Should().Be(MessageStatus.Delivered);
        message.DeliveredAt.Should().NotBeNull();
        exampleConversation.Events.Should().NotBeEmpty();
        exampleConversation.Events.OfType<MessageDelivered>().Should().NotBeEmpty();
        var @event = exampleConversation.Events.OfType<MessageDelivered>().First();
        @event.MessageId.Should().Be(message.Id);
        @event.ConversationId.Should().Be(exampleConversation.Id);
        @event.DeliveredAt.Should().Be(message.DeliveredAt!.Value);

        messageRepositoryMock.Verify(x => x.Update(
                It.Is<DomainEntity.Message>(m =>
                    m.Id == message.Id
                    && m.Status == MessageStatus.Delivered
                    && m.DeliveredAt >= before
                    && m.DeliveredAt <= after
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        conversationRepositoryMock.Verify(x => x.Update(
                It.Is<DomainEntity.Conversation>(c =>
                    c.Id == exampleConversation.Id
                    && c.Events.Any(e => e is MessageDelivered)
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenMessageNotFound))]
    [Trait("Chat/Application", "MarkAsDelivered - Use Cases")]
    public async Task ThrowWhenMessageNotFound()
    {
        var exampleMessageId = Guid.NewGuid();
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        messageRepositoryMock.Setup(x => x.Get(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new NotFoundException($"Message '{exampleMessageId}' not found."));
        var useCase = new UseCase.MarkAsDelivered(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsDeliveredInput(
            exampleMessageId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Message '{exampleMessageId}' not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenConversationNotFound))]
    [Trait("Chat/Application", "MarkAsDelivered - Use Cases")]
    public async Task ThrowWhenConversationNotFound()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
        var message = exampleConversation.SendMessage(senderId, _fixture.GetValidContent());
        messageRepositoryMock.Setup(x => x.Get(
                It.Is<Guid>(id => id == message.Id),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(message);
        conversationRepositoryMock.Setup(x => x.Get(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new NotFoundException($"Conversation '{exampleConversation.Id}' not found."));
        var useCase = new UseCase.MarkAsDelivered(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsDeliveredInput(
            message.Id
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{exampleConversation.Id}' not found.");
    }

    [Fact(DisplayName = nameof(DoesNotChangeStatusWhenAlreadyDelivered))]
    [Trait("Chat/Application", "MarkAsDelivered - Use Cases")]
    public async Task DoesNotChangeStatusWhenAlreadyDelivered()
    {
        var messageRepositoryMock = _fixture.GetMessageRepositoryMock();
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipantIds = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipantIds);
        var senderId = listParticipantIds.First();
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
        var useCase = new UseCase.MarkAsDelivered(
            messageRepositoryMock.Object,
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new MarkAsDeliveredInput(
            message.Id
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.DeliveredAt.Should().Be(message.DeliveredAt);
        output.Changed.Should().BeFalse();
        exampleConversation.Events.OfType<MessageDelivered>().Should().BeEmpty();
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
