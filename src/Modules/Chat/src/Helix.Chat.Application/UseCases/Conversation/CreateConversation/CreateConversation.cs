using DomainEntity = Helix.Chat.Domain.Entities;

namespace Helix.Chat.Application.UseCases.Conversation.CreateConversation;
public class CreateConversation : ICreateConversation
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversation(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
        => (_conversationRepository, _unitOfWork)
            = (conversationRepository, unitOfWork);

    public async Task<CreateConversationOutput> Handle(CreateConversationInput request, CancellationToken cancellationToken)
    {
        var conversation = new DomainEntity.Conversation(request.Title);
        await _conversationRepository.Insert(conversation, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return CreateConversationOutput.FromConversation(conversation);
    }
}
