namespace Helix.Chat.Application.UseCases.Conversation.AddParticipant;
public class AddParticipant : IAddParticipant
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddParticipant(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
        => (_conversationRepository, _unitOfWork)
            = (conversationRepository, unitOfWork);

    public async Task<AddParticipantOutput> Handle(
        AddParticipantInput request,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.Get(request.ConversationId, cancellationToken);
        var added = conversation.AddParticipant(request.UserId);

        var participant = conversation.Participants
            .First(p => p.UserId == request.UserId);

        await _conversationRepository.Update(conversation, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return new AddParticipantOutput(
            conversation.Id,
            participant.UserId,
            participant.JoinedAt,
            added);
    }
}
