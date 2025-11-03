using MongoDB.Driver;
using System.Linq.Expressions;

namespace Shared.Query.Interfaces;
public interface ISynchronizeDb : IDisposable
{
    Task UpdateAsync<TQueryModel>(
        FilterDefinition<TQueryModel> filter,
        UpdateDefinition<TQueryModel> update,
        CancellationToken cancellationToken,
        bool upsert = false)
        where TQueryModel : IQueryModel;

    Task DeleteAsync<TQueryModel>(
        Expression<Func<TQueryModel, bool>> deleteFilter,
        CancellationToken cancellationToken)
        where TQueryModel : IQueryModel;
}