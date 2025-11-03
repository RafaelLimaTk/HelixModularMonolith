using System.Linq.Expressions;

namespace Helix.Chat.UnitTests.Query.Common;
public sealed class Captor<TModel> where TModel : class
{
    public TModel? Model { get; set; }
    public Expression<Func<TModel, bool>>? Filter { get; set; }
}
