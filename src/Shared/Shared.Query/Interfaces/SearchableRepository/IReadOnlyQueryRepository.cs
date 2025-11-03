using Shared.Query.Specifications.Interfaces;

namespace Shared.Query.Interfaces.SearchableRepository;

public interface IReadOnlyQueryRepository<TModel, TKey>
    where TModel : IQueryModel<TKey>
{
    Task<TModel?> Get(TKey id, CancellationToken ct);
    Task<SearchResponse<TModel>> Search(SearchRequest request, CancellationToken ct);
    Task<SearchResponse<TModel>> Search(IQuerySpecification<TModel> spec, CancellationToken ct);
}