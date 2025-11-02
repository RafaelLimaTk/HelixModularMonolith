using MediatR;

namespace Helix.Chat.Application.UseCases.Message.MarkAsRead;
public sealed record MarkAsReadInput(
    Guid MessageId,
    Guid ReaderId)
: IRequest<MarkAsReadOutput>;
