using Helix.Chat.Query.Data.Context;
using MongoDB.Bson;

namespace Helix.Chat.Query.Data.Repositories;
internal sealed class MessagesReadOnlyRepository(IChatReadDbContext ctx) : IMessagesReadRepository
{
    private readonly IMongoCollection<MessageQueryModel> _col
        = ctx.GetCollection<MessageQueryModel>("messages");

    public async Task<MessageQueryModel?> Get(Guid id, CancellationToken ct)
        => await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<SearchResponse<MessageQueryModel>> Search(
        SearchRequest request, CancellationToken ct)
    {
        var (page, perPage) = Normalize(request.Page, request.PerPage);

        var filter = BuildFilter(request.Search);
        var sort = BuildSort(request.OrderBy, request.Order);

        var total = await _col.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _col.Find(filter)
            .Sort(sort)
            .Skip((page - 1) * perPage)
            .Limit(perPage)
            .ToListAsync(ct);

        return new SearchResponse<MessageQueryModel>(page, perPage, (int)total, items);
    }

    public async Task<SearchResponse<MessageQueryModel>> Search(
        IQuerySpecification<MessageQueryModel> spec, CancellationToken ct)
    {
        var filter = spec.Criteria is null
            ? FilterDefinition<MessageQueryModel>.Empty
            : Builders<MessageQueryModel>.Filter.Where(spec.Criteria);

        var sorts = spec.Orders?.Select(o =>
            o.Descending
                ? Builders<MessageQueryModel>.Sort.Descending(o.KeySelector)
                : Builders<MessageQueryModel>.Sort.Ascending(o.KeySelector))
            ?? Enumerable.Empty<SortDefinition<MessageQueryModel>>();

        var sort = sorts.Any()
            ? Builders<MessageQueryModel>.Sort.Combine(sorts)
            : Builders<MessageQueryModel>.Sort.Ascending(x => x.SentAt);

        var (page, perPage) = Normalize(spec.Page, spec.PerPage);

        var total = await _col.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _col.Find(filter)
            .Sort(sort)
            .Skip((page - 1) * perPage)
            .Limit(perPage)
            .ToListAsync(ct);

        return new SearchResponse<MessageQueryModel>(page, perPage, (int)total, items);
    }

    private static (int page, int perPage) Normalize(int page, int perPage)
    {
        page = page <= 0 ? 1 : page;
        perPage = perPage <= 0 ? 10 : perPage;
        return (page, perPage);
    }

    private static FilterDefinition<MessageQueryModel> BuildFilter(string? search)
    {
        var f = Builders<MessageQueryModel>.Filter;

        if (string.IsNullOrWhiteSpace(search))
            return FilterDefinition<MessageQueryModel>.Empty;

        if (Guid.TryParse(search, out var conversationId))
            return f.Eq(x => x.ConversationId, conversationId);

        var rx = new BsonRegularExpression(search, "i");
        return f.Regex(x => x.Content, rx);
    }

    private static SortDefinition<MessageQueryModel> BuildSort(string? orderBy, SearchOrder order)
    {
        var s = (orderBy ?? "sentAt").Trim();
        var desc = order == SearchOrder.Desc;
        var sb = Builders<MessageQueryModel>.Sort;

        return s.ToLowerInvariant() switch
        {
            "sentat" => desc ? sb.Descending(x => x.SentAt) : sb.Ascending(x => x.SentAt),
            "deliveredat" => desc ? sb.Descending(x => x.DeliveredAt) : sb.Ascending(x => x.DeliveredAt),
            "readat" => desc ? sb.Descending(x => x.ReadAt) : sb.Ascending(x => x.ReadAt),
            _ => desc ? sb.Descending(x => x.SentAt) : sb.Ascending(x => x.SentAt),
        };
    }
}
