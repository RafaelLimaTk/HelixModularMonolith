using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Conversation;
using Helix.Chat.UnitTests.Extensions.DateTime;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;
using MongoDB.Bson;

namespace Helix.Chat.UnitTests.Query.Projection.Conversation;

[Collection(nameof(QueryBaseFixture))]
public sealed class ParticipantAddedProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpdateWithAddToSetAndUpdatedAt))]
    [Trait("Chat/Query", "ParticipantAdded - Projection")]
    public async Task ProjectCallsUpdateWithAddToSetAndUpdatedAt()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<ConversationQueryModel>();
        var projection = new ParticipantAddedProjection(synchronizeDbMock.Object);

        var participantAdded = new ParticipantAdded(
            conversationId: _fixture.NewId(),
            userId: _fixture.NewId(),
            joinedAt: DateTime.UtcNow);

        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        updateCaptor.Calls.Should().Be(1);
        updateCaptor.Upsert.Should().BeFalse();

        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<ConversationQueryModel>(x => x.Id);
        var participantsFieldName = Field<ConversationQueryModel>(x => x.ParticipantIds);
        var updatedAtFieldName = Field<ConversationQueryModel>(x => x.UpdatedAt);

        filterDoc[idFieldName].AsGuid.Should().Be(participantAdded.ConversationId);

        updateDoc.Contains("$addToSet").Should().BeTrue();
        var addValue = updateDoc["$addToSet"][participantsFieldName];

        addValue.BsonType.Should().NotBe(BsonType.Document);
        addValue.AsGuid.Should().Be(participantAdded.UserId);

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][updatedAtFieldName].ToUniversalTime().TrimMilliseconds()
            .Should().Be(participantAdded.JoinedAt.TrimMilliseconds());

        synchronizeDbMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(SetsOnlyUpdatedAtAndAddToSet))]
    [Trait("Chat/Query", "ParticipantAdded - Projection")]
    public async Task SetsOnlyUpdatedAtAndAddToSet()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<ConversationQueryModel>();
        var projection = new ParticipantAddedProjection(synchronizeDbMock.Object);

        var participantAdded = new ParticipantAdded(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var (_, updateDoc) = Render(updateCaptor);

        var updatedAtFieldName = Field<ConversationQueryModel>(x => x.UpdatedAt);
        var titleFieldName = Field<ConversationQueryModel>(x => x.Title);
        var lastMessageFieldName = Field<ConversationQueryModel>(x => x.LastMessage!);

        updateDoc.Contains("$set").Should().BeTrue();
        var set = updateDoc["$set"].AsBsonDocument;
        set.Names.Should().Contain(updatedAtFieldName);
        set.Names.Should().NotContain(titleFieldName);
        set.Names.Should().NotContain(lastMessageFieldName);

        updateDoc.Contains("$addToSet").Should().BeTrue();
    }

    [Fact(DisplayName = nameof(AddsCorrectUserId))]
    [Trait("Chat/Query", "ParticipantAdded - Projection")]
    public async Task AddsCorrectUserId()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<ConversationQueryModel>();
        var projection = new ParticipantAddedProjection(synchronizeDbMock.Object);

        var participantAdded = new ParticipantAdded(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        await projection.ProjectAsync(participantAdded, CancellationToken.None);

        var (_, updateDoc) = Render(updateCaptor);

        var participantsFieldName = Field<ConversationQueryModel>(x => x.ParticipantIds);
        var addVal = updateDoc["$addToSet"][participantsFieldName];
        if (addVal.IsBsonDocument && addVal.AsBsonDocument.Contains("$each"))
            addVal.AsBsonDocument["$each"].AsBsonArray.Should().Contain(x => x.AsGuid == participantAdded.UserId);
        else
            addVal.AsGuid.Should().Be(participantAdded.UserId);
    }
}