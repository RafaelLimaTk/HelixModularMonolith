namespace Shared.Endpoints.ApiModels.Response;

public class ApiResponseList<TItemData>
    : ApiResponse<IReadOnlyList<TItemData>>
{
    public ApiResponseListMeta Meta { get; private set; }

    public ApiResponseList(
        IReadOnlyList<TItemData> data,
        int currentPage,
        int perPage,
        int total
    )
        : base(data)
    {
        Meta = new(currentPage, perPage, total);
    }
}
