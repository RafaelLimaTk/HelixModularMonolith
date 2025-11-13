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
        var dbConversation = await assertsDbContext
            .Conversations.FindAsync(exampleConversation.Id);
        dbConversation.Should().NotBeNull();
        dbConversation.Title.Should().Be(exampleConversation.Title);
        dbConversation.CreatedAt.Should().Be(exampleConversation.CreatedAt);
        var dbParticipants = assertsDbContext.ConversationsParticipants
            .Where(relation => relation.ConversationId == exampleConversation.Id)
            .ToList();
        dbParticipants.Should().HaveCount(exampleConversation.Participants.Count);
        dbParticipants.Select(participant => (participant.UserId, participant.JoinedAt)).Should()
            .BeEquivalentTo(exampleConversation.Participants.Select(participant => (participant.UserId, participant.JoinedAt)));
    }
}
