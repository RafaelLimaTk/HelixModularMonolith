namespace Helix.Chat.Application.UseCases.Conversation.SendMessage;
public class SendMessage : ISendMessage
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessage(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
        => (_conversationRepository, _messageRepository, _unitOfWork)
            = (conversationRepository, messageRepository, unitOfWork);

    public async Task<SendMessageOutput> Handle(SendMessageInput request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.Get(request.ConversationId, cancellationToken);
        var message = conversation.SendMessage(request.SenderId, request.Content);

        await _messageRepository.Insert(message, cancellationToken);
        await _conversationRepository.Update(conversation, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return SendMessageOutput.FromSendMessage(message.Id, message.SentAt);
    }
}
