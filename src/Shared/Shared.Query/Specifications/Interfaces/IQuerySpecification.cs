using System.Linq.Expressions;

namespace Shared.Query.Specifications.Interfaces;

public interface IQuerySpecification<TModel>
{
    Expression<Func<TModel, bool>>? Criteria { get; }
    IReadOnlyList<OrderExpression<TModel>> Orders { get; }
    int Page { get; }
    int PerPage { get; }
}
