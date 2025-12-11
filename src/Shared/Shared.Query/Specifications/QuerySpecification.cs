using Shared.Query.Specifications.Interfaces;
using System.Linq.Expressions;

namespace Shared.Query.Specifications;

public class QuerySpecification<TModel> : IQuerySpecification<TModel>
{
    private readonly List<OrderExpression<TModel>> _orders = new();

    public Expression<Func<TModel, bool>>? Criteria { get; private set; }
    public IReadOnlyList<OrderExpression<TModel>> Orders => _orders;
    public int Page { get; private set; } = 1;
    public int PerPage { get; private set; } = 10;

    public QuerySpecification<TModel> Where(Expression<Func<TModel, bool>> criteria)
    {
        Criteria = criteria;
        return this;
    }

    public QuerySpecification<TModel> OrderBy(Expression<Func<TModel, object>> key)
    {
        _orders.Add(new(key, false));
        return this;
    }

    public QuerySpecification<TModel> OrderByDescending(Expression<Func<TModel, object>> key)
    {
        _orders.Add(new(key, true));
        return this;
    }

    public QuerySpecification<TModel> ThenBy(Expression<Func<TModel, object>> key)
    {
        _orders.Add(new(key, false));
        return this;
    }

    public QuerySpecification<TModel> ThenByDescending(Expression<Func<TModel, object>> key)
    {
        _orders.Add(new(key, true));
        return this;
    }

    public QuerySpecification<TModel> PageSize(int page, int perPage)
    {
        Page = page <= 0 ? 1 : page;
        PerPage = perPage <= 0 ? 10 : perPage;
        return this;
    }
}
