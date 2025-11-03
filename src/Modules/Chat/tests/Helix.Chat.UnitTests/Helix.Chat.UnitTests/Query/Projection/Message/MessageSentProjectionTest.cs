using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Message;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;
using MongoDB.Driver;

namespace Helix.Chat.UnitTests.Query.Projection.Message;

[Collection(nameof(QueryBaseFixture))]
public sealed class MessageSentProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpdateWithUpsertAndCorrectDoc))]
    [Trait("Chat/Query", "MessageSent - Projection")]
    public async Task ProjectCallsUpdateWithUpsertAndCorrectDoc()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageSentProjection(synchronizeDbMock.Object);
        var messageSent = new MessageSent(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            senderId: _fixture.NewId(),
            content: _fixture.AnyContent(),
            sentAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageSent, CancellationToken.None);

        updateCaptor.Calls.Should().Be(1);
        updateCaptor.Upsert.Should().BeTrue();

        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<MessageQueryModel>(x => x.Id);
        filterDoc[idFieldName].AsGuid.Should().Be(messageSent.MessageId);

        var conversationFieldName = Field<MessageQueryModel>(x => x.ConversationId);
        var senderFieldName = Field<MessageQueryModel>(x => x.SenderId);
        var contentFieldName = Field<MessageQueryModel>(x => x.Content);
        var sentAtFieldName = Field<MessageQueryModel>(x => x.SentAt);
        var statusFieldName = Field<MessageQueryModel>(x => x.Status);

        updateDoc.Contains("$setOnInsert").Should().BeTrue("upsert requires Id on insert");
        updateDoc["$setOnInsert"][idFieldName].AsGuid.Should().Be(messageSent.MessageId);

        bool HasOperatorField(string op, string field) =>
            updateDoc.Contains(op) && updateDoc[op].AsBsonDocument.Contains(field);

        (HasOperatorField("$setOnInsert", conversationFieldName) || HasOperatorField("$set", conversationFieldName)).Should().BeTrue();
        (HasOperatorField("$setOnInsert", senderFieldName) || HasOperatorField("$set", senderFieldName)).Should().BeTrue();

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][contentFieldName].AsString.Should().Be(messageSent.Content);
        updateDoc["$set"][statusFieldName].AsString.Should().Be("Sent");
        updateDoc["$set"][sentAtFieldName].ToUniversalTime().Should().BeCloseTo(messageSent.SentAt, TimeSpan.FromSeconds(1));
        synchronizeDbMock.Verify(s => s.UpdateAsync(
            It.IsAny<FilterDefinition<MessageQueryModel>>(),
            It.IsAny<UpdateDefinition<MessageQueryModel>>(),
            It.IsAny<CancellationToken>(),
            true), Times.Once);
        synchronizeDbMock.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = nameof(DoesNotSetDeliveredOrReadAt))]
    [Trait("Chat/Query", "MessageSent - Projection")]
    public async Task DoesNotSetDeliveredOrReadAt()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageSentProjection(synchronizeDbMock.Object);
        var messageSent = new MessageSent(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            senderId: _fixture.NewId(),
            content: _fixture.AnyContent(),
            sentAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageSent, CancellationToken.None);

        updateCaptor.Upsert.Should().BeTrue();
        var (_, updateDoc) = Render(updateCaptor);
        var deliveredAtFieldName = Field<MessageQueryModel>(x => x.DeliveredAt!);
        var readAtFieldName = Field<MessageQueryModel>(x => x.ReadAt!);
        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"].AsBsonDocument.Contains(deliveredAtFieldName).Should().BeFalse();
        updateDoc["$set"].AsBsonDocument.Contains(readAtFieldName).Should().BeFalse();
    }
}
