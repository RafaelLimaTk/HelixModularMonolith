using Helix.Chat.Domain.Enums;

namespace Helix.Chat.UnitTests.Domain.Entities.Message;
public class MessageTest(MessageTestFixture fixture) : IClassFixture<MessageTestFixture>
{
    private readonly MessageTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void Instantiate()
    {
        var conversationId = _fixture.GetValidConversationId();
        var senderId = _fixture.GetValidSenderId();
        var content = _fixture.GetValidContent();

        var entity = new DomainEntity.Message(conversationId, senderId, content);

        entity.Id.Should().NotBe(Guid.Empty);
        entity.ConversationId.Should().Be(conversationId);
        entity.SenderId.Should().Be(senderId);
        entity.Content.Should().Be(content);
        entity.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        entity.Status.Should().Be(MessageStatus.Sent);
        entity.DeliveredAt.Should().BeNull();
        entity.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(InstantiateThrowWhenSenderIdEmpty))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void InstantiateThrowWhenSenderIdEmpty()
    {
        var conversationId = _fixture.GetValidConversationId();
        var emptySender = Guid.Empty;
        var content = _fixture.GetValidContent();

        Action action = () => new DomainEntity.Message(conversationId, emptySender, content);

        action.Should().Throw<EntityValidationException>()
            .WithMessage("SenderId should not be null");
    }

    [Fact(DisplayName = nameof(InstantiateThrowWhenConversationIdEmpty))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void InstantiateThrowWhenConversationIdEmpty()
    {
        var emptyConv = Guid.Empty;
        var senderId = _fixture.GetValidSenderId();
        var content = _fixture.GetValidContent();

        Action action = () => new DomainEntity.Message(emptyConv, senderId, content);

        action.Should().Throw<EntityValidationException>()
            .WithMessage("ConversationId should not be null");
    }

    [Fact(DisplayName = nameof(InstantiateThrowWhenContentEmpty))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void InstantiateThrowWhenContentEmpty()
    {
        var conversationId = _fixture.GetValidConversationId();
        var senderId = _fixture.GetValidSenderId();

        Action action = () => new DomainEntity.Message(conversationId, senderId, string.Empty);

        action.Should().Throw<EntityValidationException>()
            .WithMessage("Content should not be null or empty");
    }

    [Fact(DisplayName = nameof(InstantiateWhenContentEqualToMaxLength))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void InstantiateWhenContentEqualToMaxLength()
    {
        var conversationId = _fixture.GetValidConversationId();
        var senderId = _fixture.GetValidSenderId();
        var content = _fixture.GetLongContent(DomainEntity.Message.MAX_LENGTH);

        Action act = () => new DomainEntity.Message(conversationId, senderId, content);
        act.Should().NotThrow();
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenContentIsGreaterThan10_000Characters))]
    [Trait("Chat/Domain", "Message - Entities")]
    [MemberData(nameof(GetTitlesWithLessThan10_000Characters), parameters: 6)]
    public void InstantiateErrorWhenContentIsGreaterThan10_000Characters(string content)
    {
        var conversationId = _fixture.GetValidConversationId();
        var senderId = _fixture.GetValidSenderId();

        Action action = ()
            => new DomainEntity.Message(conversationId, senderId, content);

        action.Should().Throw<EntityValidationException>()
            .WithMessage($"Content should be at most {DomainEntity.Message.MAX_LENGTH} characters long");
    }

    public static IEnumerable<object[]> GetTitlesWithLessThan10_000Characters(int numberOfTests = 6)
    {
        var fixture = new MessageTestFixture();
        var rnd = new Random();

        for (int i = 0; i < numberOfTests; i++)
        {
            var extra = (i == 0) ? 1 : rnd.Next(20, 255);
            var len = DomainEntity.Message.MAX_LENGTH + extra;
            yield return new object[] { fixture.GetLongContent(len) };
        }
    }

    [Fact(DisplayName = nameof(MarkAsDeliveredWhenFromSentChangesStatusAndTimestamp))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void MarkAsDeliveredWhenFromSentChangesStatusAndTimestamp()
    {
        var entity = new DomainEntity.Message(
            _fixture.GetValidConversationId(),
            _fixture.GetValidSenderId(),
            _fixture.GetValidContent()
        );

        var changed = entity.MarkAsDelivered();

        changed.Should().BeTrue();
        entity.Status.Should().Be(MessageStatus.Delivered);
        entity.DeliveredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(MarkAsDeliveredWhenAlreadyDeliveredReturnsFalse))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void MarkAsDeliveredWhenAlreadyDeliveredReturnsFalse()
    {
        var entity = new DomainEntity.Message(
            _fixture.GetValidConversationId(),
            _fixture.GetValidSenderId(),
            _fixture.GetValidContent());
        entity.MarkAsDelivered();

        var changed = entity.MarkAsDelivered();

        changed.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(MarkAsReadWhenFromDeliveredChangesStatusAndTimestamp))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void MarkAsReadWhenFromDeliveredChangesStatusAndTimestamp()
    {
        var entity = new DomainEntity.Message(
            _fixture.GetValidConversationId(),
            _fixture.GetValidSenderId(),
            _fixture.GetValidContent());
        entity.MarkAsDelivered();

        var changed = entity.MarkAsRead();

        changed.Should().BeTrue();
        entity.Status.Should().Be(MessageStatus.Read);
        entity.ReadAt.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(MarkAsReadThrowWhenFromSentState))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void MarkAsReadThrowWhenFromSentState()
    {
        var entity = new DomainEntity.Message(
            _fixture.GetValidConversationId(),
            _fixture.GetValidSenderId(),
            _fixture.GetValidContent());

        Action action = () => entity.MarkAsRead();

        action.Should().Throw<EntityValidationException>()
            .WithMessage("Cannot mark as read from Sent state");
    }

    [Fact(DisplayName = nameof(MarkAsReadWhenAlreadyReadReturnsFalse))]
    [Trait("Chat/Domain", "Message - Entities")]
    public void MarkAsReadWhenAlreadyReadReturnsFalse()
    {
        var entity = new DomainEntity.Message(
            _fixture.GetValidConversationId(),
            _fixture.GetValidSenderId(),
            _fixture.GetValidContent());
        entity.MarkAsDelivered();
        entity.MarkAsRead();

        var changed = entity.MarkAsRead();

        changed.Should().BeFalse();
    }
}
