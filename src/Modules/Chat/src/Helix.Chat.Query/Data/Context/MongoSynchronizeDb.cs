using MongoDB.Driver;
using Shared.Query.Infrastructure;
using Shared.Query.Interfaces;
using System.Linq.Expressions;

namespace Helix.Chat.Query.Data.Context;
public sealed class MongoSynchronizeDb(IReadDbContext ctx) : ISynchronizeDb
{
    private readonly IReadDbContext _ctx = ctx;
    private bool _disposed;

    public Task UpsertAsync<TQueryModel>(
        TQueryModel queryModel,
        Expression<Func<TQueryModel, bool>> upsertFilter,
        CancellationToken cancellationToken)
        where TQueryModel : IQueryModel
    {
        var col = _ctx.GetCollection<TQueryModel>();
        var update = UpdatePatchBuilder.Build(queryModel);
        return col.UpdateOneAsync(upsertFilter, update,
            new UpdateOptions { IsUpsert = true }, cancellationToken);
    }

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