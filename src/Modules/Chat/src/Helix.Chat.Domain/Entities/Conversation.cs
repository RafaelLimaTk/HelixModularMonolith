using Helix.Chat.Domain.ValueObjects;
using Shared.Domain.Exceptions;
using Shared.Domain.SeedWorks;
using Shared.Domain.Validations;

namespace Helix.Chat.Domain.Entities;
public sealed class Conversation : AggregateRoot
{
    private readonly HashSet<Guid> _participantIds = new();
    private readonly List<Participant> _participants = new();

    public string Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();

    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 128;

    public Conversation(string title)
    {
        Title = title.Trim();
        CreatedAt = DateTime.UtcNow;
        Validate();
    }

    public bool AddParticipant(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new EntityValidationException("UserId should not be null");

        if (!_participantIds.Add(userId)) return false;
        _participants.Add(new Participant(userId, DateTime.UtcNow));
        Validate();
        return true;
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Title, nameof(Title));
        DomainValidation.MinLength(Title, MIN_LENGTH, nameof(Title));
        DomainValidation.MaxLength(Title, MAX_LENGTH, nameof(Title));
    }
}