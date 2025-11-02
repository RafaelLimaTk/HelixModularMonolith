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
    [Trait("Chat/Application", "MarkAsDelivered - UseCase")]
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
        var input = new UseCase.MarkAsDeliveredInput(message.Id);

        var before = DateTime.UtcNow;
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow;

        output.Should().NotBeNull();
        output.MessageId.Should().Be(message.Id);
        output.Changed.Should().BeTrue();
        output.DeliveredAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        message.Status.Should().Be(MessageStatus.Delivered);
        message.DeliveredAt.Should().NotBeNull();
        messageRepositoryMock.Verify(x => x.Update(
            It.Is<DomainEntity.Message>(m =>
                m.Id == message.Id
                && m.Status == MessageStatus.Delivered
                && m.DeliveredAt >= before
                && m.DeliveredAt <= after
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
