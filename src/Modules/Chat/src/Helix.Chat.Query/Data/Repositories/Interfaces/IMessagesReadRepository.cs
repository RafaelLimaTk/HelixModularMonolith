namespace Helix.Chat.Query.Data.Repositories.Interfaces;
public interface IMessagesReadRepository
    : IReadOnlyQueryRepository<MessageQueryModel, Guid>
{ }