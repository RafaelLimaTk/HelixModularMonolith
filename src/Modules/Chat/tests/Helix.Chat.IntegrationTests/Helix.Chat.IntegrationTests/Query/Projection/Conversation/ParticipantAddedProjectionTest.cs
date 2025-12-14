using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Query.Projections.Conversation;

namespace Helix.Chat.IntegrationTests.Query.Projection.Conversation;

[Collection(nameof(ConversationProjectionTestFixture))]
public class ParticipantAddedProjectionTest(ConversationProjectionTestFixture fixture)
{
    private readonly ConversationProjectionTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(AddsUserToParticipantsList))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task AddsUserToParticipantsList()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var existingConversation = _fixture.GetExampleConversation(id: conversationId);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(conversationId, userId, joinedAt);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.ParticipantIds.Should().Contain(userId);
        updatedConversation.ParticipantIds.Should().HaveCount(1);
    }

    [Fact(DisplayName = nameof(UpdatesUpdatedAtTimestamp))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task UpdatesUpdatedAtTimestamp()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var originalUpdatedAt = DateTime.UtcNow.AddMinutes(-10);
        var existingConversation = _fixture.GetExampleConversation(
            id: conversationId,
            updatedAt: originalUpdatedAt);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(conversationId, userId, joinedAt);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.UpdatedAt.Should().BeCloseTo(joinedAt, TimeSpan.FromSeconds(1));
        updatedConversation.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact(DisplayName = nameof(IsIdempotentDoesNotDuplicateParticipant))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task IsIdempotentDoesNotDuplicateParticipant()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var existingConversation = _fixture.GetExampleConversation(id: conversationId);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(conversationId, userId, joinedAt);
        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.ParticipantIds.Should().Contain(userId);
        updatedConversation.ParticipantIds.Should().HaveCount(1);
        updatedConversation.ParticipantIds.Count(p => p == userId).Should().Be(1);
    }

    [Fact(DisplayName = nameof(AddsMultipleParticipants))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task AddsMultipleParticipants()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var existingConversation = _fixture.GetExampleConversation(id: conversationId);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;

        await projection.ProjectAsync(new ParticipantAdded(conversationId, user1Id, joinedAt), CancellationToken.None);
        await projection.ProjectAsync(new ParticipantAdded(conversationId, user2Id, joinedAt), CancellationToken.None);
        await projection.ProjectAsync(new ParticipantAdded(conversationId, user3Id, joinedAt), CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.ParticipantIds.Should().HaveCount(3);
        updatedConversation.ParticipantIds.Should().Contain(user1Id);
        updatedConversation.ParticipantIds.Should().Contain(user2Id);
        updatedConversation.ParticipantIds.Should().Contain(user3Id);
    }

    [Fact(DisplayName = nameof(DoesNotModifyOtherFields))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task DoesNotModifyOtherFields()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var originalTitle = _fixture.GetValidTitle();
        var originalCreatedAt = DateTime.UtcNow.AddDays(-1);
        var existingConversation = _fixture.GetExampleConversation(
            id: conversationId,
            title: originalTitle,
            createdAt: originalCreatedAt);
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(conversationId, userId, joinedAt);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.Title.Should().Be(originalTitle);
        updatedConversation.CreatedAt.Should().BeCloseTo(originalCreatedAt, TimeSpan.FromSeconds(1));
        updatedConversation.LastMessage.Should().BeNull();
    }

    [Fact(DisplayName = nameof(DoesNotFailWhenConversationDoesNotExist))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task DoesNotFailWhenConversationDoesNotExist()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var nonExistentConversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(nonExistentConversationId, userId, joinedAt);

        var act = async () =>
            await projection.ProjectAsync(participantAdded, CancellationToken.None);

        await act.Should().NotThrowAsync();
        var conversation = await conversationsCollection
            .Find(x => x.Id == nonExistentConversationId)
            .FirstOrDefaultAsync();
        conversation.Should().BeNull();
    }

    [Fact(DisplayName = nameof(AddsToExistingParticipantsList))]
    [Trait("Chat/Integration/Query/Projections", "ParticipantAdded - Projection")]
    public async Task AddsToExistingParticipantsList()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var conversationId = Guid.NewGuid();
        var existingParticipant1 = Guid.NewGuid();
        var existingParticipant2 = Guid.NewGuid();
        var existingConversation = _fixture.GetExampleConversation(
            id: conversationId,
            participantIds: new List<Guid> { existingParticipant1, existingParticipant2 });
        await conversationsCollection.InsertOneAsync(existingConversation);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new ParticipantAddedProjection(sync);
        var newUserId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;
        var participantAdded = new ParticipantAdded(conversationId, newUserId, joinedAt);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var updatedConversation = await conversationsCollection
            .Find(x => x.Id == conversationId)
            .FirstOrDefaultAsync();
        updatedConversation.Should().NotBeNull();
        updatedConversation.ParticipantIds.Should().HaveCount(3);
        updatedConversation.ParticipantIds.Should().Contain(existingParticipant1);
        updatedConversation.ParticipantIds.Should().Contain(existingParticipant2);
        updatedConversation.ParticipantIds.Should().Contain(newUserId);
    }
}
