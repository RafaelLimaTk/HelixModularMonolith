namespace Helix.Chat.Application.UseCases.Message.MarkAsDelivered;
public sealed record MarkAsDeliveredOutput(
    Guid MessageId,
    DateTime DeliveredAt,
    bool Changed
);
