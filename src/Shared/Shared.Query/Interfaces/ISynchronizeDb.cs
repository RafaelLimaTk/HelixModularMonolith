using System.Linq.Expressions;

namespace Shared.Query.Interfaces;
public interface ISynchronizeDb : IDisposable
{
    /// Upserts a query model into the database.
    Task UpsertAsync<TQueryModel>(
        TQueryModel queryModel,
        Expression<Func<TQueryModel, bool>> upsertFilter,
        CancellationToken cancellationToken)
        where TQueryModel : IQueryModel;

    /// Deletes query models that match the filter.
    Task DeleteAsync<TQueryModel>(
        Expression<Func<TQueryModel, bool>> deleteFilter,
        CancellationToken cancellationToken)
        where TQueryModel : IQueryModel;
}