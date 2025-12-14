using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Query.Projections.Conversation;

namespace Helix.Chat.IntegrationTests.Query.Projection.Conversation;

[Collection(nameof(ConversationProjectionTestFixture))]
public class ConversationCreatedProjectionTest(ConversationProjectionTestFixture fixture)
{
    private readonly ConversationProjectionTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(InsertEventConversationCreated))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task InsertEventConversationCreated()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationId = Guid.NewGuid();
        var title = _fixture.GetValidTitle();
        var createdAt = DateTime.UtcNow;
        var conversationCreated = new ConversationCreated(conversationId, title, createdAt);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var savedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        savedConversation.Should().NotBeNull();
        savedConversation.Id.Should().Be(conversationId);
        savedConversation.Title.Should().Be(title);
        savedConversation.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        savedConversation.UpdatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        savedConversation.ParticipantIds.Should().NotBeNull();
        savedConversation.ParticipantIds.Should().BeEmpty();
        savedConversation.LastMessage.Should().BeNull();
    }

    [Fact(DisplayName = nameof(InitializesEmptyParticipantsList))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task InitializesEmptyParticipantsList()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationId = Guid.NewGuid();
        var conversationCreated = new ConversationCreated(
            conversationId,
            _fixture.GetValidTitle(),
            DateTime.UtcNow);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var savedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        savedConversation.Should().NotBeNull();
        savedConversation.ParticipantIds.Should().NotBeNull();
        savedConversation.ParticipantIds.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(IsIdempotentDoesNotDuplicateOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task IsIdempotentDoesNotDuplicateOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationId = Guid.NewGuid();
        var title = _fixture.GetValidTitle();
        var createdAt = DateTime.UtcNow;
        var conversationCreated = new ConversationCreated(conversationId, title, createdAt);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);
        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var allConversations = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .ToListAsync();
        allConversations.Should().HaveCount(1);
        allConversations[0].Title.Should().Be(title);
        allConversations[0].CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = nameof(UpdatesUpdatedAtEvenOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task UpdatesUpdatedAtEvenOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationId = Guid.NewGuid();
        var title = _fixture.GetValidTitle();
        var firstCreatedAt = DateTime.UtcNow;
        var firstEvent = new ConversationCreated(conversationId, title, firstCreatedAt);
        await projection.ProjectAsync(firstEvent, CancellationToken.None);
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var firstSave = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        await Task.Delay(100);
        var secondCreatedAt = DateTime.UtcNow;
        var secondEvent = new ConversationCreated(conversationId, title, secondCreatedAt);

        await projection.ProjectAsync(secondEvent, CancellationToken.None);

        var secondSave = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        secondSave.Should().NotBeNull();
        secondSave.Title.Should().Be(title);
        secondSave.CreatedAt.Should().BeCloseTo(firstCreatedAt, TimeSpan.FromSeconds(1));
        secondSave.UpdatedAt.Should().BeCloseTo(secondCreatedAt, TimeSpan.FromSeconds(1));
        secondSave.UpdatedAt.Should().BeAfter(firstSave.UpdatedAt);
    }

    [Fact(DisplayName = nameof(PreservesExistingParticipantsOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task PreservesExistingParticipantsOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var existingParticipants = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var existingConversation = _fixture.GetExampleConversation(
            id: conversationId,
            participantIds: existingParticipants);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationCreated = new ConversationCreated(
            conversationId,
            _fixture.GetValidTitle(),
            DateTime.UtcNow);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        var savedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        savedConversation.Should().NotBeNull();
        savedConversation.ParticipantIds.Should().HaveCount(2);
        savedConversation.ParticipantIds.Should().BeEquivalentTo(existingParticipants);
    }

    [Fact(DisplayName = nameof(ProjectConversationCreated_SetsAllRequiredFields))]
    [Trait("Chat/Integration/Query/Projections", "ConversationCreated - Projection")]
    public async Task ProjectConversationCreated_SetsAllRequiredFields()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ConversationCreatedProjection(sync);
        var conversationId = Guid.NewGuid();
        var title = _fixture.GetValidTitle();
        var createdAt = new DateTime();
        var conversationCreated = new ConversationCreated(conversationId, title, createdAt);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var savedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        savedConversation.Should().NotBeNull();
        savedConversation.Id.Should().Be(conversationId);
        savedConversation.Title.Should().Be(title);
        savedConversation.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        savedConversation.UpdatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        savedConversation.ParticipantIds.Should().NotBeNull().And.BeEmpty();
        savedConversation.LastMessage.Should().BeNull();
    }
}
