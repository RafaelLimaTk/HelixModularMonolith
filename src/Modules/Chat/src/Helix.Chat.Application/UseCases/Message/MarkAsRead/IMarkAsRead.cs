using MediatR;

namespace Helix.Chat.Application.UseCases.Message.MarkAsRead;
public interface IMarkAsRead
    : IRequestHandler<MarkAsReadInput, MarkAsReadOutput>
{ }
