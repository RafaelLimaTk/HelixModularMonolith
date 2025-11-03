using System.Linq.Expressions;

namespace Shared.Query.Specifications;

public sealed record OrderExpression<TModel>(
    Expression<Func<TModel, object>> KeySelector,
    bool Descending
);
