using Helix.Chat.Query.Data.Context;
using Helix.Chat.Query.Data.Repositories.Interfaces;
using Helix.Chat.Query.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Query.Interfaces.SearchableRepository;
using Shared.Query.Specifications.Interfaces;

namespace Helix.Chat.Query.Data.Repositories;
public sealed class ConversationsReadOnlyRepository(IChatReadDbContext ctx)
        : IConversationsReadRepository
{
    private readonly IMongoCollection<ConversationQueryModel> _col
        = ctx.GetCollection<ConversationQueryModel>("conversations");

    public async Task<ConversationQueryModel?> Get(Guid id, CancellationToken ct)
        => await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<SearchResponse<ConversationQueryModel>> Search(
        SearchRequest request, CancellationToken ct)
    {
        var (page, perPage) = Normalize(request.Page, request.PerPage);
        var filter = BuildFilter(request.Search);
        var sort = BuildSort(request.OrderBy, request.Order);

        var total = await _col.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _col.Find(filter)
            .Sort(sort).Skip((page - 1) * perPage).Limit(perPage)
            .ToListAsync(ct);

        return new SearchResponse<ConversationQueryModel>(page, perPage, (int)total, items);
    }

    public async Task<SearchResponse<ConversationQueryModel>> Search(
        IQuerySpecification<ConversationQueryModel> spec, CancellationToken ct)
    {
        var filter = spec.Criteria is null
            ? FilterDefinition<ConversationQueryModel>.Empty
            : Builders<ConversationQueryModel>.Filter.Where(spec.Criteria);

        var sorts = spec.Orders?.Select(o =>
            o.Descending
                ? Builders<ConversationQueryModel>.Sort.Descending(o.KeySelector)
                : Builders<ConversationQueryModel>.Sort.Ascending(o.KeySelector))
            ?? Enumerable.Empty<SortDefinition<ConversationQueryModel>>();

        var sort = sorts.Any()
            ? Builders<ConversationQueryModel>.Sort.Combine(sorts)
            : Builders<ConversationQueryModel>.Sort.Descending(x => x.UpdatedAt);

        var (page, perPage) = Normalize(spec.Page, spec.PerPage);

        var total = await _col.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _col.Find(filter)
            .Sort(sort).Skip((page - 1) * perPage).Limit(perPage)
            .ToListAsync(ct);

        return new SearchResponse<ConversationQueryModel>(page, perPage, (int)total, items);
    }

    private static (int page, int perPage) Normalize(int page, int perPage)
        => (page <= 0 ? 1 : page, perPage <= 0 ? 10 : perPage);

    private static FilterDefinition<ConversationQueryModel> BuildFilter(string? search)
    {
        var f = Builders<ConversationQueryModel>.Filter;
        if (string.IsNullOrWhiteSpace(search))
            return FilterDefinition<ConversationQueryModel>.Empty;

        if (Guid.TryParse(search, out var userId))
            return f.AnyEq(x => x.ParticipantIds, userId);

        var rx = new BsonRegularExpression(search, "i");
        return f.Regex(x => x.Title, rx);
    }

    private static SortDefinition<ConversationQueryModel> BuildSort(string? orderBy, SearchOrder order)
    {
        var s = (orderBy ?? "updatedAt").Trim();
        var desc = order == SearchOrder.Desc;
        var sb = Builders<ConversationQueryModel>.Sort;

        return s.ToLowerInvariant() switch
        {
            "title" => desc ? sb.Descending(x => x.Title) : sb.Ascending(x => x.Title),
            "createdat" => desc ? sb.Descending(x => x.CreatedAt) : sb.Ascending(x => x.CreatedAt),
            "updatedat" => desc ? sb.Descending(x => x.UpdatedAt) : sb.Ascending(x => x.UpdatedAt),
            _ => desc ? sb.Descending(x => x.UpdatedAt) : sb.Ascending(x => x.UpdatedAt),
        };
    }
}
