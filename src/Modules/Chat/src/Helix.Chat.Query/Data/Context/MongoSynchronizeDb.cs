using System.Linq.Expressions;

namespace Helix.Chat.Query.Data.Context;
public sealed class MongoSynchronizeDb(IReadDbContext ctx) : ISynchronizeDb
{
    private readonly IReadDbContext _ctx = ctx;
    private bool _disposed;

    public Task UpdateAsync<TQueryModel>(
        FilterDefinition<TQueryModel> filter,
        UpdateDefinition<TQueryModel> update,
        CancellationToken cancellationToken,
        bool upsert = false)
        where TQueryModel : IQueryModel
        => _ctx.GetCollection<TQueryModel>()
               .UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = upsert }, cancellationToken);

    public Task DeleteAsync<TQueryModel>(
        Expression<Func<TQueryModel, bool>> deleteFilter,
        CancellationToken cancellationToken)
        where TQueryModel : IQueryModel
        => _ctx.GetCollection<TQueryModel>()
               .DeleteManyAsync(deleteFilter, cancellationToken);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}