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
        dbMessage!.Content.Should().Be(exampleMessage.Content);
        dbMessage.ConversationId.Should().Be(exampleMessage.ConversationId);
        dbMessage.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));
        dbMessage.SenderId.Should().Be(exampleMessage.SenderId);
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
        message.Content.Should().Be(exampleMessage.Content);
        message.ConversationId.Should().Be(exampleMessage.ConversationId);
        message.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));
        message.SenderId.Should().Be(exampleMessage.SenderId);
    }
}
