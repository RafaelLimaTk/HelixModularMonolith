namespace Shared.Endpoints.ApiModels.Response;

public class ApiResponseListMeta
{
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }

    public ApiResponseListMeta()
    { }

    public ApiResponseListMeta(int currentPage, int perPage, int total)
    {
        CurrentPage = currentPage;
        PerPage = perPage;
        Total = total;
    }
}
