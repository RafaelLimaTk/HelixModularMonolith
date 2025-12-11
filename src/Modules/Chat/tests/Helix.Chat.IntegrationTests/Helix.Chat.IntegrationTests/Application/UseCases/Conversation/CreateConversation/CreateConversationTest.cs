using Microsoft.Extensions.Logging;
using Shared.Domain.Exceptions;
using UseCase = Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.IntegrationTests.Application.UseCases.Conversation.CreateConversation;

[Collection(nameof(CreateConversationTestFixture))]
public class CreateConversationTest(CreateConversationTestFixture fixture)
{
    private readonly CreateConversationTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(CreateConversation))]
    [Trait("Chat/Integration/Application", "CreateConversation - Use Cases")]
    public async Task CreateConversation()
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var useCase = new UseCase.CreateConversation(
            repository,
            unitOfWork
        );
        var input = new UseCase.CreateConversationInput(
            _fixture.GetValidTitle()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbConversation = await assertDbContext.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == output.Id);
        dbConversation.Should().NotBeNull();
        dbConversation!.Id.Should().Be(output.Id);
        dbConversation.Title.Should().Be(input.Title);
        dbConversation.CreatedAt.Should().BeCloseTo(output.CreatedAt, TimeSpan.FromSeconds(1));
        dbConversation.Participants.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(CreateConversationRaisesConversationCreatedEvent))]
    [Trait("Chat/Integration/Application", "CreateConversation - Use Cases")]
    public async Task CreateConversationRaisesConversationCreatedEvent()
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var useCase = new UseCase.CreateConversation(
            repository,
            unitOfWork
        );
        var input = new UseCase.CreateConversationInput(
            _fixture.GetValidTitle()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        outboxStoreMock.Verify(x => x.AppendAsync(
            It.Is<EventEnvelope>(e =>
                e.EventName == "ConversationCreated" &&
                e.ClrType.Contains("ConversationCreated") &&
                !string.IsNullOrWhiteSpace(e.Payload)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenTitleIsEmpty))]
    [Trait("Chat/Integration/Application", "CreateConversation - Use Cases")]
    public async Task ThrowWhenTitleIsEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var useCase = new UseCase.CreateConversation(
            repository,
            unitOfWork
        );
        var input = new UseCase.CreateConversationInput(
            string.Empty
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Title should not be null or empty");

        var assertDbContext = _fixture.CreateDbContext(true);
        var conversationsCount = await assertDbContext.Conversations.CountAsync();
        conversationsCount.Should().Be(0);
    }

    [Theory(DisplayName = nameof(ThrowWhenTitleIsLessThan3Characters))]
    [Trait("Chat/Integration/Application", "CreateConversation - Use Cases")]
    [InlineData("a")]
    [InlineData("ab")]
    public async Task ThrowWhenTitleIsLessThan3Characters(string invalidTitle)
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var useCase = new UseCase.CreateConversation(
            repository,
            unitOfWork
        );
        var input = new UseCase.CreateConversationInput(
            invalidTitle
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Title should be at least 3 characters long");

        var assertDbContext = _fixture.CreateDbContext(true);
        var conversationsCount = await assertDbContext.Conversations.CountAsync();
        conversationsCount.Should().Be(0);
    }

    [Theory(DisplayName = nameof(ThrowWhenTitleIsGreaterThanMaxLength))]
    [Trait("Chat/Integration/Application", "CreateConversation - Use Cases")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task ThrowWhenTitleIsGreaterThanMaxLength(int excess)
    {
        var dbContext = _fixture.CreateDbContext();
        var repository = new Repository.ConversationRepository(dbContext);
        var outboxStoreMock = new Mock<IOutboxStore>();
        var unitOfWork = new UnitOfWork(
            dbContext,
            outboxStoreMock.Object,
            new Mock<ILogger<UnitOfWork>>().Object
        );
        var useCase = new UseCase.CreateConversation(
            repository,
            unitOfWork
        );
        var maxLength = DomainEntity.Conversation.MAX_LENGTH;
        var invalidTitle = _fixture.GetLongTitle(maxLength + excess);
        var input = new UseCase.CreateConversationInput(
            invalidTitle
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage($"Title should be at most {maxLength} characters long");

        var assertDbContext = _fixture.CreateDbContext(true);
        var conversationsCount = await assertDbContext.Conversations.CountAsync();
        conversationsCount.Should().Be(0);
    }
}
