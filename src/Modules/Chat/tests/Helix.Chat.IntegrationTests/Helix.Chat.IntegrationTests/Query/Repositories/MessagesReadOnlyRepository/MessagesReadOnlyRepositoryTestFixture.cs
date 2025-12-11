namespace Helix.Chat.IntegrationTests.Query.Repositories.MessagesReadOnlyRepository;

[CollectionDefinition(nameof(MessagesReadOnlyRepositoryTestFixture))]
public class MessagesReadOnlyRepositoryTestFixtureCollection
    : ICollectionFixture<MessagesReadOnlyRepositoryTestFixture>
{ }

public class MessagesReadOnlyRepositoryTestFixture : QueryBaseFixture
{
    public MessageQueryModel GetExampleMessage(
        string? content = null,
        Guid? conversationId = null,
        Guid? senderId = null,
        DateTime? sentAt = null,
        DateTime? deliveredAt = null,
        DateTime? readAt = null)
    {
        var resolvedContent = content ?? Faker.Lorem.Sentence(10);
        var resolvedConversationId = conversationId ?? Guid.NewGuid();
        var resolvedSenderId = senderId ?? Guid.NewGuid();

        var sent = sentAt ?? DateTime.UtcNow.AddMinutes(-Faker.Random.Int(0, 120));
        var delivered = deliveredAt ?? (Faker.Random.Bool(0.7f)
            ? sent.AddSeconds(Faker.Random.Int(1, 60))
            : (DateTime?)null);
        var read = readAt ?? (delivered.HasValue && Faker.Random.Bool(0.5f)
            ? delivered.Value.AddSeconds(Faker.Random.Int(1, 300))
            : (DateTime?)null);

        return new MessageQueryModel
        {
            Id = Guid.NewGuid(),
            ConversationId = resolvedConversationId,
            SenderId = resolvedSenderId,
            Content = resolvedContent,
            SentAt = sent,
            DeliveredAt = delivered,
            ReadAt = read
        };
    }

    public List<MessageQueryModel> GetExampleMessagesList(
        int length = 10,
        Guid? commonConversationId = null)
    {
        var baseTime = DateTime.UtcNow;
        var conversationId = commonConversationId ?? Guid.NewGuid();

        return Enumerable.Range(0, length)
            .Select(index => GetExampleMessage(
                conversationId: conversationId,
                sentAt: baseTime.AddMilliseconds(index * 100),
                deliveredAt: baseTime.AddMilliseconds((index * 100) + 10),
                readAt: index % 2 == 0 ? baseTime.AddMilliseconds((index * 100) + 20) : null))
            .ToList();
    }

    public List<MessageQueryModel> GetExampleMessagesListByContent(List<string> contents)
        => contents.Select(content => GetExampleMessage(content: content)).ToList();

    public List<MessageQueryModel> GetExampleMessagesListByConversation(
        Guid conversationId,
        int length = 10)
        => GetExampleMessagesList(length, conversationId);

    public List<MessageQueryModel> CloneMessagesListOrdered(
        List<MessageQueryModel> messagesList,
        string orderBy,
        SearchOrder order)
    {
        var listClone = new List<MessageQueryModel>(messagesList);
        return ApplyOrdering(listClone, orderBy, order).ToList();
    }

    public List<MessageQueryModel> FilterOrderAndPaginate(
        List<MessageQueryModel> source,
        Func<MessageQueryModel, bool>? predicate = null,
        string orderBy = "sentAt",
        SearchOrder order = SearchOrder.Desc,
        int page = 1,
        int perPage = 10)
    {
        IEnumerable<MessageQueryModel> query = source;

        if (predicate != null)
            query = query.Where(predicate);

        var ordered = ApplyOrdering(query, orderBy, order);

        var skip = (Math.Max(1, page) - 1) * Math.Max(1, perPage);
        return ordered.Skip(skip).Take(Math.Max(0, perPage)).ToList();
    }

    private static IOrderedEnumerable<MessageQueryModel> ApplyOrdering(
        IEnumerable<MessageQueryModel> source,
        string orderBy,
        SearchOrder dir)
    {
        var orderKey = orderBy.Trim().ToLowerInvariant();
        return (orderKey, dir) switch
        {
            ("sentat", SearchOrder.Asc) => source.OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id),
            ("sentat", SearchOrder.Desc) => source.OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id),
            ("deliveredat", SearchOrder.Asc) => source.OrderBy(m => m.DeliveredAt)
                .ThenBy(m => m.SentAt),
            ("deliveredat", SearchOrder.Desc) => source.OrderByDescending(m => m.DeliveredAt)
                .ThenByDescending(m => m.SentAt),
            ("readat", SearchOrder.Asc) => source.OrderBy(m => m.ReadAt)
                .ThenBy(m => m.SentAt),
            ("readat", SearchOrder.Desc) => source.OrderByDescending(m => m.ReadAt)
                .ThenByDescending(m => m.SentAt),
            _ => source.OrderBy(m => m.SentAt).ThenBy(m => m.Id),
        };
    }

    public QuerySpecification<MessageQueryModel> BuildSpecificationForConversation(
        Guid conversationId,
        string orderBy = "sentAt",
        SearchOrder sortDirection = SearchOrder.Asc,
        int page = 1,
        int perPage = 10)
    {
        var spec = new QuerySpecification<MessageQueryModel>()
            .Where(m => m.ConversationId == conversationId)
            .PageSize(page, perPage);

        var orderKey = orderBy.Trim().ToLowerInvariant();
        spec = (orderKey, sortDirection) switch
        {
            ("sentat", SearchOrder.Asc) => spec.OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id),
            ("sentat", SearchOrder.Desc) => spec.OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id),
            ("deliveredat", SearchOrder.Asc) => spec.OrderBy(m => m.DeliveredAt!)
                .ThenBy(m => m.SentAt),
            ("deliveredat", SearchOrder.Desc) => spec.OrderByDescending(m => m.DeliveredAt!)
                .ThenByDescending(m => m.SentAt),
            ("readat", SearchOrder.Asc) => spec.OrderBy(m => m.ReadAt!)
                .ThenBy(m => m.SentAt),
            ("readat", SearchOrder.Desc) => spec.OrderByDescending(m => m.ReadAt!)
                .ThenByDescending(m => m.SentAt),
            _ => spec.OrderBy(m => m.SentAt).ThenBy(m => m.Id)
        };

        return spec;
    }
}
