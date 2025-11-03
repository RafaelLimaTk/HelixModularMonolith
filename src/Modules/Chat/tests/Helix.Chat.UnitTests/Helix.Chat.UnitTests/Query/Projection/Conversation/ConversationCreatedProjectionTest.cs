using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Conversation;
using Helix.Chat.UnitTests.Extensions.DateTime;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;

namespace Helix.Chat.UnitTests.Query.Projection.Conversation;

[Collection(nameof(QueryBaseFixture))]
public sealed class ConversationCreatedProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpdateWithUpsertAndCorrectDoc))]
    [Trait("Chat/Query", "ConversationCreated - Projection")]
    public async Task ProjectCallsUpdateWithUpsertAndCorrectDoc()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<ConversationQueryModel>();
        var projection = new ConversationCreatedProjection(synchronizeDbMock.Object);

        var exampleTitle = _fixture.GetValidTitle();
        var conversationCreated = new ConversationCreated(
            conversationId: _fixture.NewId(),
            title: exampleTitle,
            createdAt: DateTime.UtcNow);

        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        updateCaptor.Calls.Should().Be(1);
        updateCaptor.Upsert.Should().BeTrue();

        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<ConversationQueryModel>(x => x.Id);
        var titleFieldName = Field<ConversationQueryModel>(x => x.Title);
        var createdAtFieldName = Field<ConversationQueryModel>(x => x.CreatedAt);
        var updatedAtFieldName = Field<ConversationQueryModel>(x => x.UpdatedAt);
        var participantsFieldName = Field<ConversationQueryModel>(x => x.ParticipantIds);

        filterDoc[idFieldName].AsGuid.Should().Be(conversationCreated.ConversationId);

        updateDoc.Contains("$setOnInsert").Should().BeTrue();
        updateDoc["$setOnInsert"][idFieldName].AsGuid.Should().Be(conversationCreated.ConversationId);
        updateDoc["$setOnInsert"][titleFieldName].AsString.Should().Be(exampleTitle);
        updateDoc["$setOnInsert"][createdAtFieldName].ToUniversalTime().TrimMilliseconds()
            .Should().Be(conversationCreated.CreatedAt.TrimMilliseconds());
        updateDoc["$setOnInsert"][participantsFieldName].Should().NotBeNull();

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][updatedAtFieldName].ToUniversalTime().TrimMilliseconds()
            .Should().Be(conversationCreated.CreatedAt.TrimMilliseconds());

        synchronizeDbMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(UpsertShape))]
    [Trait("Chat/Query", "ConversationCreated - Projection")]
    public async Task UpsertShape()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<ConversationQueryModel>();
        var projection = new ConversationCreatedProjection(synchronizeDbMock.Object);

        var exampleTitle = _fixture.GetValidTitle();
        var conversationCreated = new ConversationCreated(
            conversationId: _fixture.NewId(),
            title: exampleTitle,
            createdAt: DateTime.UtcNow);
        await projection.ProjectAsync(conversationCreated, CancellationToken.None);

        updateCaptor.Upsert.Should().BeTrue();
        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<ConversationQueryModel>(x => x.Id);
        var titleFieldName = Field<ConversationQueryModel>(x => x.Title);
        var createdAtFieldName = Field<ConversationQueryModel>(x => x.CreatedAt);
        var updatedAtFieldName = Field<ConversationQueryModel>(x => x.UpdatedAt);
        var participantsFieldName = Field<ConversationQueryModel>(x => x.ParticipantIds);

        filterDoc[idFieldName].AsGuid.Should().Be(conversationCreated.ConversationId);

        updateDoc.Contains("$setOnInsert").Should().BeTrue();
        var setOnInsertDoc = updateDoc["$setOnInsert"].AsBsonDocument;
        setOnInsertDoc[idFieldName].AsGuid.Should().Be(conversationCreated.ConversationId);
        setOnInsertDoc[titleFieldName].AsString.Should().Be(exampleTitle);
        setOnInsertDoc[createdAtFieldName].ToUniversalTime().TrimMilliseconds()
            .Should().Be(conversationCreated.CreatedAt.TrimMilliseconds());
        setOnInsertDoc.Contains(participantsFieldName).Should().BeTrue();

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][updatedAtFieldName].ToUniversalTime().TrimMilliseconds()
            .Should().Be(conversationCreated.CreatedAt.TrimMilliseconds());
    }
}