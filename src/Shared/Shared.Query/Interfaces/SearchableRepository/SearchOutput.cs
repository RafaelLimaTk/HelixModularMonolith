namespace Shared.Query.Interfaces.SearchableRepository;
public class SearchOutput<TModel>
{
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<TModel> Items { get; set; }
    public SearchOutput(
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
