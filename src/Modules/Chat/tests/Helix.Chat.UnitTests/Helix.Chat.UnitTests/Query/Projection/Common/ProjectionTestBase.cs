using Helix.Chat.UnitTests.Query.Common;
using Shared.Query.Interfaces;
using System.Linq.Expressions;

namespace Helix.Chat.UnitTests.Query.Projection.Common;

public abstract class ProjectionTestBase
{
    protected (Mock<ISynchronizeDb> mock, Captor<TModel> cap) CaptureUpsert<TModel>()
        where TModel : class, IQueryModel
    {
        var mock = new Mock<ISynchronizeDb>(MockBehavior.Strict);
        var cap = new Captor<TModel>();

        mock.Setup(s => s.UpsertAsync(
                It.IsAny<TModel>(),
                It.IsAny<Expression<Func<TModel, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<TModel, Expression<Func<TModel, bool>>, CancellationToken>((m, f, _) =>
            {
                cap.Model = m;
                cap.Filter = f;
            })
            .Returns(Task.CompletedTask);

        return (mock, cap);
    }
}