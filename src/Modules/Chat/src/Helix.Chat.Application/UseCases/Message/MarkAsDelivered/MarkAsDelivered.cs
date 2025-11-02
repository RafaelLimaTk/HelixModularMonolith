using Helix.Chat.Domain.Events.Message;

namespace Helix.Chat.Application.UseCases.Message.MarkAsDelivered;
public sealed class MarkAsDelivered : IMarkAsDelivered
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsDelivered(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
        => (_messageRepository, _conversationRepository, _unitOfWork)
            = (messageRepository, conversationRepository, unitOfWork);

    public async Task<MarkAsDeliveredOutput> Handle(
        MarkAsDeliveredInput request,
        CancellationToken cancellationToken)
    {
        var message = await _messageRepository.Get(request.MessageId, cancellationToken);
        var conversation = await _conversationRepository.Get(message.ConversationId, cancellationToken);

        var changed = message.MarkAsDelivered();

        if (changed)
        {
            conversation.RaiseEvent(new MessageDelivered(
                message.Id,
                conversation.Id,
                message.DeliveredAt!.Value
            ));

            await _messageRepository.Update(message, cancellationToken);
            await _conversationRepository.Update(conversation, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);
        }

        return new MarkAsDeliveredOutput(
            message.Id,
            message.DeliveredAt!.Value,
            changed
        );
    }
}
