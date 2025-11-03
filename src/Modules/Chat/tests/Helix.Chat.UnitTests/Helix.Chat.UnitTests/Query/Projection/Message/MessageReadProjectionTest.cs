using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Message;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;

namespace Helix.Chat.UnitTests.Query.Projection.Message;

[Collection(nameof(QueryBaseFixture))]
public sealed class MessageReadProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpdateWithCorrectFilterAndUpdate))]
    [Trait("Chat/Query", "MessageRead - Projection")]
    public async Task ProjectCallsUpdateWithCorrectFilterAndUpdate()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageReadProjection(synchronizeDbMock.Object);

        var messageRead = new MessageRead(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            readerId: _fixture.NewId(),
            readAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        updateCaptor.Calls.Should().Be(1);
        updateCaptor.Upsert.Should().BeFalse();

        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<MessageQueryModel>(x => x.Id);
        var statusFieldName = Field<MessageQueryModel>(x => x.Status);

        filterDoc.ToString().Should().Contain(idFieldName);
        filterDoc.ToString().Should().Contain(statusFieldName);
        filterDoc.ToString().Should().Contain("$ne");

        var readAtFieldName = Field<MessageQueryModel>(x => x.ReadAt!);

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][statusFieldName].AsString.Should().Be("Read");

        updateDoc.Contains("$max").Should().BeTrue();
        updateDoc["$max"][readAtFieldName].ToUniversalTime()
            .Should().BeCloseTo(messageRead.ReadAt, TimeSpan.FromSeconds(1));

        synchronizeDbMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(UsesMaxAndDoesNotTouchDeliveredAt))]
    [Trait("Chat/Query", "MessageRead - Projection")]
    public async Task UsesMaxAndDoesNotTouchDeliveredAt()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageReadProjection(synchronizeDbMock.Object);
        var messageRead = new MessageRead(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            readerId: _fixture.NewId(),
            readAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        updateCaptor.Upsert.Should().BeFalse();
        var (_, updateDoc) = Render(updateCaptor);

        var readAtFieldName = Field<MessageQueryModel>(x => x.ReadAt!);
        var deliveredAtFieldName = Field<MessageQueryModel>(x => x.DeliveredAt!);

        updateDoc.Contains("$max").Should().BeTrue();
        updateDoc["$max"].AsBsonDocument.Contains(readAtFieldName).Should().BeTrue();

        var setDoc = updateDoc["$set"].AsBsonDocument;
        setDoc.Contains(deliveredAtFieldName).Should().BeFalse();
    }
}
