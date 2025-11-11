using Shared.Query.Specifications.Interfaces;

namespace Shared.Query.Interfaces.SearchableRepository;

public interface IReadOnlyQueryRepository<TModel, TKey>
    where TModel : IQueryModel<TKey>
{
    Task<TModel?> Get(TKey id, CancellationToken ct);
    Task<SearchOutput<TModel>> Search(SearchInput request, CancellationToken ct);
    Task<SearchOutput<TModel>> Search(IQuerySpecification<TModel> spec, CancellationToken ct);
}