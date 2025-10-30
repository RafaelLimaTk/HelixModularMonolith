using Shared.Domain.Exceptions;

namespace Helix.Chat.UnitTests.Domain.Entities.Conversation;

public class ConversationTest(ConversationTestFixture fixture) : IClassFixture<ConversationTestFixture>
{
    private readonly ConversationTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void Instantiate()
    {
        var validTitle = _fixture.GetValidTitle();

        var aggregate = new DomainEntity.Conversation(validTitle);

        aggregate.Id.Should().NotBe(Guid.Empty);
        aggregate.Title.Should().Be(validTitle);
        aggregate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        aggregate.Participants.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(InstantiateThrowWhenNameEmpty))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void InstantiateThrowWhenNameEmpty()
    {
        Action action = ()
            => new DomainEntity.Conversation(string.Empty);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Title should not be null or empty");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenTitleIsLessThan3Characters))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    [MemberData(nameof(GetTitlesWithLessThan3Characters), parameters: 10)]
    public void InstantiateErrorWhenTitleIsLessThan3Characters(string shortTitle)
    {
        Action action = ()
            => new DomainEntity.Conversation(shortTitle);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Title should be at least 3 characters long");
    }

    public static IEnumerable<object[]> GetTitlesWithLessThan3Characters(int numberOfTests = 6)
    {
        var fixture = new ConversationTestFixture();
        for (int i = 0; i < numberOfTests; i++)
        {
            var len = (i % 2 == 0) ? 1 : 2;
            yield return new object[] { fixture.GetShortTitle(len) };
        }
    }

    [Fact(DisplayName = nameof(InstantiateWhenEqualToMaxLength))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void InstantiateWhenEqualToMaxLength()
    {
        var title = _fixture.GetLongTitle(DomainEntity.Conversation.MAX_LENGTH);

        Action action = () => new DomainEntity.Conversation(title);
        action.Should().NotThrow();

        var conv = new DomainEntity.Conversation(title);
        conv.Title.Should().Be(title);
        conv.Id.Should().NotBe(Guid.Empty);
        conv.Participants.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenTitleIsLessThan128Characters))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    [MemberData(nameof(GetTitlesWithLessThan128Characters), parameters: 6)]
    public void InstantiateErrorWhenTitleIsLessThan128Characters(string tooLongTitle)
    {
        Action action = ()
            => new DomainEntity.Conversation(tooLongTitle);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"Title should be at most {DomainEntity.Conversation.MAX_LENGTH} characters long");
    }

    public static IEnumerable<object[]> GetTitlesWithLessThan128Characters(int numberOfTests = 6)
    {
        var fixture = new ConversationTestFixture();
        var rnd = new Random();

        for (int i = 0; i < numberOfTests; i++)
        {
            var extra = (i == 0) ? 1 : rnd.Next(1, 50);
            var len = DomainEntity.Conversation.MAX_LENGTH + extra;
            yield return new object[] { fixture.GetLongTitle(len) };
        }
    }

    [Fact(DisplayName = nameof(AddParticipantWhenFirstTimeReturnsTrueAndAddsParticipant))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void AddParticipantWhenFirstTimeReturnsTrueAndAddsParticipant()
    {
        var aggregate = new DomainEntity.Conversation(_fixture.GetValidTitle());
        var validUserId = _fixture.GetValidUserId();

        var result = aggregate.AddParticipant(validUserId);

        result.Should().BeTrue();
        aggregate.Participants.Should().HaveCount(1);
        aggregate.Participants.First().UserId.Should().Be(validUserId);
    }

    [Fact(DisplayName = nameof(AddParticipantWhenDuplicateUserReturnsFalseAndDoesNotDuplicate))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void AddParticipantWhenDuplicateUserReturnsFalseAndDoesNotDuplicate()
    {
        var aggregate = new DomainEntity.Conversation(_fixture.GetValidTitle());
        var validUserId = _fixture.GetValidUserId();
        aggregate.AddParticipant(validUserId);

        var resultDuplicate = aggregate.AddParticipant(validUserId);

        resultDuplicate.Should().BeFalse();
        aggregate.Participants.Should().HaveCount(1);
    }

    [Fact(DisplayName = nameof(AddParticipantThrowWhenUserIdIsEmpty))]
    [Trait("Chat/Domain", "Conversation - Aggregates")]
    public void AddParticipantThrowWhenUserIdIsEmpty()
    {
        var aggregate = new DomainEntity.Conversation(_fixture.GetValidTitle());
        var emptyUserId = Guid.Empty;

        var act = () => aggregate.AddParticipant(emptyUserId);

        act.Should().Throw<EntityValidationException>()
           .WithMessage("UserId should not be null");
    }
}
