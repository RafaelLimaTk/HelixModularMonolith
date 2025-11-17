namespace Helix.Chat.IntegrationTests.Infra.Data.EF.Repositories.MessageRepository;

[Collection(nameof(MessageRepositoryTestFixture))]
public class MessageRepositoryTest(MessageRepositoryTestFixture fixture)
{
    private readonly MessageRepositoryTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Chat/Integration/Infra.Data", "MessageRepository - Repositories")]
    public async Task Insert()
    {
        var exampleMessage = _fixture.GetMessageExample();
        await using var context = _fixture.CreateDbContext();
        var messageRepository = new Repository.MessageRepository(context);

        await messageRepository.Insert(exampleMessage, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        await using var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .FindAsync(exampleMessage.Id, CancellationToken.None);
        dbMessage.Should().NotBeNull();
        dbMessage.Id.Should().NotBeEmpty();
        dbMessage.Id.Should().Be(exampleMessage.Id);
        dbMessage.ConversationId.Should().Be(exampleMessage.ConversationId);
        dbMessage.SenderId.Should().Be(exampleMessage.SenderId);
        dbMessage.Content.Should().Be(exampleMessage.Content);
        dbMessage.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));
        dbMessage.Status.Should().Be(exampleMessage.Status);
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Chat/Integration/Infra.Data", "MessageRepository - Repositories")]
    public async Task Get()
    {
        var exampleMessage = _fixture.GetMessageExample();
        await using var context = _fixture.CreateDbContext();
        await context.Messages.AddAsync(exampleMessage);
        await context.SaveChangesAsync();
        var messageRepository =
            new Repository.MessageRepository(_fixture.CreateDbContext(true));

        var message = await messageRepository.Get(exampleMessage.Id, CancellationToken.None);

        message.Should().NotBeNull();
        message.Id.Should().Be(exampleMessage.Id);
        message.ConversationId.Should().Be(exampleMessage.ConversationId);
        message.SenderId.Should().Be(exampleMessage.SenderId);
        message.Content.Should().Be(exampleMessage.Content);
        message.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));
        message.Status.Should().Be(exampleMessage.Status);
    }

    [Fact(DisplayName = nameof(GetThrowIfNotFound))]
    [Trait("Chat/Integration/Infra.Data", "MessageRepository - Repositories")]
    public async Task GetThrowIfNotFound()
    {
        await using var context = _fixture.CreateDbContext();
        var messageRepository =
            new Repository.MessageRepository(context);
        var nonExistentMessageId = Guid.NewGuid();

        Func<Task> act = async () =>
            await messageRepository.Get(nonExistentMessageId, CancellationToken.None);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Message '{nonExistentMessageId}' not found.");
    }

    [Fact(DisplayName = nameof(Update))]
    public async Task Update()
    {
        var exampleMessage = _fixture.GetMessageExample();
        await using var context = _fixture.CreateDbContext();
        await context.Messages.AddAsync(exampleMessage);
        await context.SaveChangesAsync();
        var dbContextAct = _fixture.CreateDbContext(true);
        var messageRepository =
            new Repository.MessageRepository(dbContextAct);
        var savedMessage = await messageRepository
            .Get(exampleMessage.Id, CancellationToken.None);
        savedMessage.MarkAsDelivered();
        var expectedStatus = savedMessage.Status;
        var expectedDeliveredAt = savedMessage.DeliveredAt;

        await messageRepository.Update(savedMessage, CancellationToken.None);
        await dbContextAct.SaveChangesAsync(CancellationToken.None);

        await using var assertDbContext = _fixture.CreateDbContext(true);
        var dbMessage = await assertDbContext.Messages
            .FindAsync(exampleMessage.Id, CancellationToken.None);
        dbMessage.Should().NotBeNull();
        dbMessage.Id.Should().Be(exampleMessage.Id);
        dbMessage.ConversationId.Should().Be(exampleMessage.ConversationId);
        dbMessage.SenderId.Should().Be(exampleMessage.SenderId);
        dbMessage.Content.Should().Be(exampleMessage.Content);
        dbMessage.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));
        dbMessage.Status.Should().Be(expectedStatus);
        dbMessage.DeliveredAt.Should().BeCloseTo(expectedDeliveredAt!.Value, TimeSpan.FromSeconds(1));
    }
}
