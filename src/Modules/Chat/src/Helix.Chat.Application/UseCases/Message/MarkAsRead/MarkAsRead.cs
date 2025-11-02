using Helix.Chat.Domain.Events.Conversation;
using Shared.Domain.Exceptions;

namespace Helix.Chat.Application.UseCases.Message.MarkAsRead;
public class MarkAsRead : IMarkAsRead
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsRead(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarkAsReadOutput> Handle(
        MarkAsReadInput request,
        CancellationToken cancellationToken)
    {
        var message = await _messageRepository.Get(request.MessageId, cancellationToken);
        var conversation = await _conversationRepository.Get(message.ConversationId, cancellationToken);

        if (!conversation.Participants.Any(p => p.UserId == request.ReaderId))
            throw new EntityValidationException("ReaderId must be a participant of the conversation");

        var changed = message.MarkAsRead();

        if (changed)
        {
            conversation.RaiseEvent(new MessageRead(
                message.Id,
                conversation.Id,
                request.ReaderId,
                message.ReadAt!.Value
            ));

            await _messageRepository.Update(message, cancellationToken);
            await _conversationRepository.Update(conversation, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);
        }

        return new MarkAsReadOutput(message.Id, message.ReadAt!.Value, changed);
    }
}
