using Helix.Chat.Domain.Entities;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.Domain.Interfaces;
public interface IMessageRepository : IRepository
{
    Task Insert(Message message, CancellationToken cancellationToken);
    Task<Message> Get(Guid id, CancellationToken cancellationToken);
    Task Update(Message message, CancellationToken cancellationToken);
}
