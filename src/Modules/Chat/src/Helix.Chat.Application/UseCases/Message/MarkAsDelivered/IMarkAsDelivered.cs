using MediatR;

namespace Helix.Chat.Application.UseCases.Message.MarkAsDelivered;
public interface IMarkAsDelivered
    : IRequestHandler<MarkAsDeliveredInput, MarkAsDeliveredOutput>
{ }
