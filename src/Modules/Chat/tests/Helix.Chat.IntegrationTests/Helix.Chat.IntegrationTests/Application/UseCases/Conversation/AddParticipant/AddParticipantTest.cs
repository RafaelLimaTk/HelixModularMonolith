using Shared.Domain.Exceptions;
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

    [Fact(DisplayName = nameof(AddParticipantRaisesParticipantAddedEvent))]
    [Trait("Chat/Integration/Application", "AddParticipant - Use Cases")]
    public async Task AddParticipantRaisesParticipantAddedEvent()
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
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.Is<EventEnvelope>(e =>
                e.EventName == "ParticipantAdded" &&
                e.ClrType.Contains("ParticipantAdded") &&
                !string.IsNullOrWhiteSpace(e.Payload)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(AddMultipleParticipants))]
    [Trait("Chat/Integration/Application", "AddParticipant - Use Cases")]
    public async Task AddMultipleParticipants()
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
        var userIds = _fixture.GetParticipantIds(5);

        foreach (var userId in userIds)
        {
            var input = new UseCase.AddParticipantInput(
                conversation.Id,
                userId
            );
            var output = await useCase.Handle(input, CancellationToken.None);
            output.Added.Should().BeTrue();
        }

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbConversation = await assertDbContext.Conversations
            .Include(c => c.Participants)
            .AsNoTracking()
            .FirstAsync(c => c.Id == conversation.Id);
        dbConversation.Participants.Should().HaveCount(5);
        dbConversation.Participants.Select(p => p.UserId)
            .Should().BeEquivalentTo(userIds);
    }

    [Fact(DisplayName = nameof(ThrowWhenUserIdIsEmpty))]
    [Trait("Chat/Integration/Application", "AddParticipant - Use Cases")]
    public async Task ThrowWhenUserIdIsEmpty()
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
        var useCase = new UseCase.AddParticipant(
            repository,
            unitOfWork
        );
        var input = new UseCase.AddParticipantInput(
            conversation.Id,
            Guid.Empty
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("UserId should not be empty");

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbConversation = await assertDbContext.Conversations
            .Include(c => c.Participants)
            .AsNoTracking()
            .FirstAsync(c => c.Id == conversation.Id);
        dbConversation.Participants.Should().BeEmpty();
    }
}
