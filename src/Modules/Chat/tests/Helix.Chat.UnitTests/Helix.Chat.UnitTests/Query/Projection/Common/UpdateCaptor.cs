using MongoDB.Driver;
using Shared.Query.Interfaces;

namespace Helix.Chat.UnitTests.Query.Projection.Common;
public sealed class UpdateCaptor<TModel> where TModel : IQueryModel
{
    public FilterDefinition<TModel>? Filter { get; set; }
    public UpdateDefinition<TModel>? Update { get; set; }
    public bool Upsert { get; set; }
    public int Calls { get; set; }
}
