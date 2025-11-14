namespace Helix.Chat.IntegrationTests.Infra.Data.EF.Repositories.ConversationRepository;

[Collection(nameof(ConversationRepositoryTestFixture))]
public class ConversationRepositoryTest(ConversationRepositoryTestFixture fixture)
{
    private readonly ConversationRepositoryTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Chat/Integration/Infra.Data", "ConversationRepository - Repositories")]
    public async Task Insert()
    {
        HelixChatDbContext dbContext = _fixture.CreateDbContext();
        var participantsList = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: participantsList);
        var conversationRepository = new Repository.ConversationRepository(dbContext);

        await conversationRepository.Insert(exampleConversation, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbConversation = await assertsDbContext.Conversations
            .FindAsync(exampleConversation.Id);
        dbConversation.Should().NotBeNull();
        dbConversation.Title.Should().Be(exampleConversation.Title);
        dbConversation.CreatedAt.Should().Be(exampleConversation.CreatedAt);
        dbConversation.Participants.Should()
            .HaveCount(exampleConversation.Participants.Count);
        dbConversation.Participants
            .Select(participant => (participant.UserId, participant.JoinedAt))
            .Should()
            .BeEquivalentTo(
                exampleConversation.Participants
                    .Select(participant => (participant.UserId, participant.JoinedAt)));
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Chat/Integration/Infra.Data", "ConversationRepository - Repositories")]
    public async Task Get()
    {
        var participantsList = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: participantsList);
        using var dbContext = _fixture.CreateDbContext();
        await dbContext.Conversations.AddAsync(exampleConversation);
        await dbContext.SaveChangesAsync();
        var conversationRepository =
            new Repository.ConversationRepository(_fixture.CreateDbContext(true));

        var conversation = await conversationRepository.Get(exampleConversation.Id, CancellationToken.None);

        conversation.Should().NotBeNull();
        conversation.Id.Should().Be(exampleConversation.Id);
        conversation.Title.Should().Be(exampleConversation.Title);
        conversation.CreatedAt.Should().Be(exampleConversation.CreatedAt);
        conversation.Participants.Should()
            .HaveCount(exampleConversation.Participants.Count);
        conversation.Participants
            .Select(participant => (participant.UserId, participant.JoinedAt))
            .Should()
            .BeEquivalentTo(
                exampleConversation.Participants
                    .Select(participant => (participant.UserId, participant.JoinedAt)));
    }

    [Fact(DisplayName = nameof(GetThrowIfNotFind))]
    [Trait("Chat/Integration/Infra.Data", "ConversationRepository - Repositories")]
    public async Task GetThrowIfNotFind()
    {
        var conversationRepository =
            new Repository.ConversationRepository(_fixture.CreateDbContext(true));
        var nonExistentId = Guid.NewGuid();

        Func<Task> action = async () =>
            await conversationRepository.Get(nonExistentId, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{nonExistentId}' not found.");
    }
}
