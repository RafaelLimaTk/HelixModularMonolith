namespace Helix.Chat.Query.Data.Repositories.Interfaces;
public interface IConversationsReadRepository
    : IReadOnlyQueryRepository<ConversationQueryModel, Guid>
{ }
