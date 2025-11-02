using MediatR;

namespace Helix.Chat.Application.UseCases.Message.MarkAsDelivered;
public sealed record MarkAsDeliveredInput(Guid MessageId) : IRequest<MarkAsDeliveredOutput>;
