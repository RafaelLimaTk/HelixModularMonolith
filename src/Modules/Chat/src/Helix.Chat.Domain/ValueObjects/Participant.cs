using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.ValueObjects;
public sealed class Participant : ValueObject
{
    public Guid UserId { get; }
    public DateTime JoinedAt { get; }

    public Participant(Guid userId, DateTime joinedAt)
    {
        UserId = userId;
        JoinedAt = joinedAt;
    }

    public override bool Equals(ValueObject? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is not Participant otherParticipant) return false;

        return otherParticipant.UserId == UserId
            && otherParticipant.JoinedAt == JoinedAt;
    }

    protected override int GetCustomHashCode()
        => HashCode.Combine(UserId, JoinedAt);
}