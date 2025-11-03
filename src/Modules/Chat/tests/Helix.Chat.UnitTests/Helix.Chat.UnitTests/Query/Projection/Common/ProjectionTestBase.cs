using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Shared.Query.Interfaces;
using System.Linq.Expressions;

namespace Helix.Chat.UnitTests.Query.Projection.Common;

public abstract class ProjectionTestBase
{
    protected (Mock<ISynchronizeDb> synchronizeDbMock, UpdateCaptor<TModel> updateCaptor) CaptureUpdate<TModel>()
        where TModel : IQueryModel
    {
        var synchronizeDbMock = new Mock<ISynchronizeDb>(MockBehavior.Strict);
        var updateCaptor = new UpdateCaptor<TModel>();

        synchronizeDbMock.Setup(s => s.UpdateAsync(
                It.IsAny<FilterDefinition<TModel>>(),
                It.IsAny<UpdateDefinition<TModel>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .Callback<FilterDefinition<TModel>, UpdateDefinition<TModel>, CancellationToken, bool>(
                (filter, update, _, upsert) =>
                {
                    updateCaptor.Filter = filter;
                    updateCaptor.Update = update;
                    updateCaptor.Upsert = upsert;
                    updateCaptor.Calls++;
                })
            .Returns(Task.CompletedTask);

        return (synchronizeDbMock, updateCaptor);
    }

    protected static (BsonDocument filter, BsonDocument update) Render<TModel>(UpdateCaptor<TModel> updateCaptor)
        where TModel : IQueryModel
    {
        var serializer = BsonSerializer.LookupSerializer<TModel>();
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var renderArgs = new RenderArgs<TModel>(serializer, serializerRegistry);
        var renderedFilter = updateCaptor.Filter!.Render(renderArgs).AsBsonDocument;
        var renderedUpdate = updateCaptor.Update!.Render(renderArgs).AsBsonDocument;
        return (renderedFilter, renderedUpdate);
    }

    protected static string Field<TModel>(Expression<Func<TModel, object>> expr)
    {
        var expressionBody = expr.Body is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert
            ? unaryExpr.Operand : expr.Body;

        var memberName = (expressionBody as MemberExpression)?.Member.Name
            ?? throw new InvalidOperationException("Expression must be a member access expression (e.g., x => x.PropertyName).");

        var classMap = BsonClassMap.LookupClassMap(typeof(TModel));
        return classMap.GetMemberMap(memberName).ElementName;
    }
}