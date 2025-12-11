using UseCase = Helix.Chat.Application.UseCases.Conversation.AddParticipant;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.AddParticipant;

[Collection(nameof(AddParticipantTestFixture))]
public class AddParticipantTest(AddParticipantTestFixture fixture)
{
    private readonly AddParticipantTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(AddParticipant))]
    [Trait("Chat/Integration/Application", "AddParticipant - Use Cases")]
    public async Task AddParticipant()
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var conversation = _fixture.GetExampleConversation();
        await repository.Insert(conversation, CancellationToken.None);
        await unitOfWork.Commit(CancellationToken.None);
        outboxStoreMock.Reset();
        var useCase = new UseCase.AddParticipant(
            repository,
            unitOfWork
        );
        var userId = Guid.NewGuid();
        var input = new UseCase.AddParticipantInput(
            conversation.Id,
            userId
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.ConversationId.Should().Be(conversation.Id);
        output.UserId.Should().Be(userId);
        output.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        output.Added.Should().BeTrue();
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbConversation = await assertDbContext.Conversations
            .Include(c => c.Participants)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversation.Id);
        dbConversation.Should().NotBeNull();
        dbConversation!.Participants.Should().HaveCount(1);
        dbConversation.Participants.First().UserId.Should().Be(userId);
        dbConversation.Participants.First().JoinedAt.Should().BeCloseTo(output.JoinedAt, TimeSpan.FromSeconds(1));
    }
}
