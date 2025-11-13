using Helix.Chat.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Helix.Chat.Infra.Data.EF.Repositories;
public class ConversationRepository : IConversationRepository
{
    private readonly HelixChatDbContext _context;
    private DbSet<Conversation> _conversations => _context.Set<Conversation>();
    private DbSet<ConversationsParticipants> _conversationsParticipants
        => _context.Set<ConversationsParticipants>();

    public ConversationRepository(HelixChatDbContext context)
        => _context = context;

    public async Task Insert(Conversation conversation, CancellationToken cancellationToken)
    {
        await _conversations.AddAsync(conversation, cancellationToken);

        if (conversation.Participants.Count > 0)
        {
            var relations = conversation.Participants
                .Select(participant => new ConversationsParticipants(
                    conversation.Id,
                    participant.UserId,
                    participant.JoinedAt
                ));
            await _conversationsParticipants.AddRangeAsync(relations, cancellationToken);
        }
    }

    public Task<Conversation> Get(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Update(Conversation aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Delete(Conversation aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
