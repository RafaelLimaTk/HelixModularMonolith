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
}
