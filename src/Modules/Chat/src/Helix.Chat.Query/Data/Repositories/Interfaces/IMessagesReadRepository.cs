using Helix.Chat.Query.Models;
using Shared.Query.Interfaces.SearchableRepository;

namespace Helix.Chat.Query.Data.Repositories.Interfaces;
public interface IMessagesReadRepository
    : IReadOnlyQueryRepository<MessageQueryModel, Guid>
{ }