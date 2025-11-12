namespace Helix.Chat.Query.Models;
public sealed class ConversationQueryModel : IQueryModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<Guid> ParticipantIds { get; init; } = new();
    public MessageSnapshot? LastMessage { get; init; }

    public sealed record MessageSnapshot(Guid MessageId, string Content, DateTime SentAt, string Status);
}
