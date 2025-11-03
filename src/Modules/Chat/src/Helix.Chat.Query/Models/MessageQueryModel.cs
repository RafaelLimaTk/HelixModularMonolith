using Shared.Query.Interfaces;

namespace Helix.Chat.Query.Models;
public sealed class MessageQueryModel : IQueryModel
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string Content { get; init; } = default!;
    public DateTime SentAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public string Status { get; init; } = "Sent";
}