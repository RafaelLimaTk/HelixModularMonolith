using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Domain.ValueObjects;
using Shared.Domain.Exceptions;
using Shared.Domain.SeedWorks;
using Shared.Domain.Validations;

namespace Helix.Chat.Domain.Entities;

public sealed class Conversation : AggregateRoot
{
    private readonly List<Participant> _participants = [];

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
        RaiseEvent(new ConversationCreated(Id, Title, CreatedAt));
    }

    public bool AddParticipant(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new EntityValidationException("UserId should not be empty");

        if (_participants.Any(p => p.UserId == userId)) return false;
        var joinedAt = DateTime.UtcNow;
        _participants.Add(new Participant(userId, joinedAt));
        RaiseEvent(new ParticipantAdded(Id, userId, joinedAt));
        Validate();
        return true;
    }

    public Message SendMessage(Guid senderId, string content)
    {
        if (!_participants.Any(p => p.UserId == senderId))
            throw new EntityValidationException("SenderId must be a participant of the conversation");

        var message = new Message(this.Id, senderId, content);

        RaiseEvent(new MessageSent(
            message.Id,
            this.Id,
            senderId,
            message.Content,
            message.SentAt));

        return message;
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Title, nameof(Title));
        DomainValidation.MinLength(Title, MIN_LENGTH, nameof(Title));
        DomainValidation.MaxLength(Title, MAX_LENGTH, nameof(Title));
    }
}