namespace Shared.Query.Interfaces.SearchableRepository;
public class SearchResponse<TModel>
{
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<TModel> Items { get; set; }
    public SearchResponse(
        int currentPage,
        int perPage,
        int total,
        IReadOnlyList<TModel> items)
    {
        CurrentPage = currentPage;
        PerPage = perPage;
        Total = total;
        Items = items;
    }
}
