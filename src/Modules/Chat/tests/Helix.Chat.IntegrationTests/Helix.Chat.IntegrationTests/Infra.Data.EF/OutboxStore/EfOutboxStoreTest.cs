namespace Helix.Chat.IntegrationTests.Infra.Data.EF.OutboxStore;

[Collection(nameof(EfOutboxStoreTestFixture))]
public class EfOutboxStoreTest(EfOutboxStoreTestFixture fixture)
{
    private readonly EfOutboxStoreTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(AppendAsync))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task AppendAsync()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxStore = new EfOutboxStore(dbContext);
        var envelope = _fixture.CreateEnvelope();

        await outboxStore.AppendAsync(envelope, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertsDbContext.OutboxMessages
            .FirstOrDefaultAsync(message => message.Type == envelope.ClrType);
        dbMessage.Should().NotBeNull();
        dbMessage.Payload.Should().Be(envelope.Payload);
        dbMessage.OccurredOnUtc.Should().BeCloseTo(envelope.OccurredOnUtc, TimeSpan.FromSeconds(1));
        dbMessage.ProcessedOnUtc.Should().BeNull();
    }

    [Fact(DisplayName = nameof(TakeBatchAsync))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task TakeBatchAsync()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxStore = new EfOutboxStore(dbContext);
        var unprocessedMessages = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateOutboxMessage(processed: false))
            .ToList();
        var processedMessages = Enumerable.Range(0, 3)
            .Select(_ => _fixture.CreateOutboxMessage(processed: true))
            .ToList();
        var listMessages = unprocessedMessages.Concat(processedMessages).ToList();
        await dbContext.OutboxMessages.AddRangeAsync(listMessages);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var batch = await outboxStore.TakeBatchAsync(10, CancellationToken.None);

        batch.Should().HaveCount(unprocessedMessages.Count);
        batch.Select(message => message.Id).Should()
            .BeEquivalentTo(unprocessedMessages.Select(message => message.Id));
    }

    [Fact(DisplayName = nameof(LimitsResultToRequestedMaxTakeBatch))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task LimitsResultToRequestedMaxTakeBatch()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxStore = new EfOutboxStore(dbContext);
        var unprocessedMessages = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateOutboxMessage(processed: false))
            .ToList();
        await dbContext.OutboxMessages.AddRangeAsync(unprocessedMessages);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var maxTake = 3;
        var batch = await outboxStore.TakeBatchAsync(maxTake, CancellationToken.None);

        batch.Should().HaveCount(maxTake);
        batch.Select(m => m.Id).Should()
            .BeEquivalentTo(
                unprocessedMessages
                    .OrderBy(m => m.Id)
                    .Take(maxTake)
                    .Select(m => m.Id));
    }

    [Fact(DisplayName = nameof(ReturnsEmptyWhenAllMessagesAreProcessed))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task ReturnsEmptyWhenAllMessagesAreProcessed()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxStore = new EfOutboxStore(dbContext);
        var processedMessages = Enumerable.Range(0, 5)
            .Select(_ => _fixture.CreateOutboxMessage(processed: true))
            .ToList();
        await dbContext.OutboxMessages.AddRangeAsync(processedMessages);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var batch = await outboxStore.TakeBatchAsync(10, CancellationToken.None);

        batch.Should().NotBeNull();
        batch.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(MarksMessageAsProcessedSuccessfully))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task MarksMessageAsProcessedSuccessfully()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxMessage = _fixture.CreateOutboxMessage(processed: false);
        var outboxStore = new EfOutboxStore(dbContext);
        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await outboxStore.MarkProcessedAsync(outboxMessage.Id, CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertsDbContext.OutboxMessages
            .FirstAsync(message => message.Id == outboxMessage.Id);
        dbMessage.Should().NotBeNull();
        dbMessage.ProcessedOnUtc.Should().NotBeNull();
        dbMessage.Error.Should().BeNull();
    }

    [Fact(DisplayName = nameof(MarksMessageAsFailedWithErrorMessage))]
    [Trait("Chat/Integration/Infra.Data", "EfOutboxStore - Outbox")]
    public async Task MarksMessageAsFailedWithErrorMessage()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var outboxMessage = _fixture.CreateOutboxMessage(processed: false);
        var outboxStore = new EfOutboxStore(dbContext);
        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var errorMessage = "Processing failed due to an error.";

        await outboxStore.MarkFailedAsync(outboxMessage.Id, errorMessage, CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertsDbContext.OutboxMessages
            .FirstAsync(message => message.Id == outboxMessage.Id);
        dbMessage.Should().NotBeNull();
        dbMessage.ProcessedOnUtc.Should().BeNull();
        dbMessage.Error.Should().Be(errorMessage);
    }
}
