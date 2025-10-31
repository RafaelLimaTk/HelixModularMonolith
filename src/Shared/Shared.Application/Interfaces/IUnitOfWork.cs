namespace Shared.Application.Interfaces;
public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken);
}