using Helix.Chat.Domain.ValueObjects;

namespace Helix.Chat.UnitTests.Domain.ValueObject;
public class ParticipantTest : BaseFixture
{
    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Chat/Domain", "Participant - ValueObjects")]
    public void Instantiate()
    {
        var userId = Faker.Random.Guid();
        var joinedAt = DateTime.UtcNow;

        var participant = new Participant(userId, joinedAt);

        participant.UserId.Should().Be(userId);
        participant.JoinedAt.Should().Be(joinedAt);
    }

    [Fact(DisplayName = nameof(EqualsProperty))]
    [Trait("Chat/Domain", "Participant - ValueObjects")]
    public void EqualsProperty()
    {
        var userId = Faker.Random.Guid();
        var joinedAt = DateTime.UtcNow;

        var participant = new Participant(userId, joinedAt);
        var sameParticipant = new Participant(userId, joinedAt);

        var isItEqual = participant.Equals(sameParticipant);

        isItEqual.Should().BeTrue();
    }

    [Theory(DisplayName = nameof(DifferentProperty))]
    [Trait("Chat/Domain", "Participant - ValueObjects")]
    [MemberData(nameof(GetDifferentValues), parameters: 6)]
    public void DifferentProperty(Participant differentParticipant)
    {
        var participant = new Participant(Guid.NewGuid(), DateTime.UtcNow);

        var isItEqual = participant.Equals(differentParticipant);

        isItEqual.Should().BeFalse();
    }

    public static IEnumerable<object[]> GetDifferentValues(int numberOfTests = 6)
    {
        var faker = new Faker();
        for (int i = 0; i < numberOfTests; i++)
        {
            var userId = Guid.NewGuid();
            var joinedAt = DateTime.UtcNow.AddMinutes(faker.Random.Int(-10000, 10000));
            yield return new object[] { new Participant(userId, joinedAt) };
        }
    }
}
