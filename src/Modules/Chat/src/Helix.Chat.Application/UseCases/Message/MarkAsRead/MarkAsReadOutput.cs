namespace Helix.Chat.Application.UseCases.Message.MarkAsRead;
public sealed record MarkAsReadOutput(
    Guid MessageId,
    DateTime ReadAt,
    bool Changed
);
