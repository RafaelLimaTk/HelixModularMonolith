using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Message;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;

namespace Helix.Chat.UnitTests.Query.Projection.Message;

[Collection(nameof(QueryBaseFixture))]
public sealed class MessageDeliveredProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpdateWithCorrectFilterAndUpdate))]
    [Trait("Chat/Query", "MessageDelivered - Projection")]
    public async Task ProjectCallsUpdateWithCorrectFilterAndUpdate()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageDeliveredProjection(synchronizeDbMock.Object);

        var messageDelivered = new MessageDelivered(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            deliveredAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        updateCaptor.Calls.Should().Be(1);
        updateCaptor.Upsert.Should().BeFalse();

        var (filterDoc, updateDoc) = Render(updateCaptor);

        var idFieldName = Field<MessageQueryModel>(x => x.Id);
        var statusFieldName = Field<MessageQueryModel>(x => x.Status);
        filterDoc.ToString().Should().Contain(idFieldName);
        filterDoc.ToString().Should().Contain(statusFieldName);
        filterDoc.ToString().Should().Contain("$ne");

        var deliveredAtFieldName = Field<MessageQueryModel>(x => x.DeliveredAt!);

        updateDoc.Contains("$set").Should().BeTrue();
        updateDoc["$set"][statusFieldName].AsString.Should().Be("Delivered");

        updateDoc.Contains("$max").Should().BeTrue();
        updateDoc["$max"][deliveredAtFieldName].ToUniversalTime()
            .Should().BeCloseTo(messageDelivered.DeliveredAt, TimeSpan.FromSeconds(1));

        synchronizeDbMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(UsesMaxAndDoesNotTouchImmutableFields))]
    [Trait("Chat/Query", "MessageDelivered - Projection")]
    public async Task UsesMaxAndDoesNotTouchImmutableFields()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageDeliveredProjection(synchronizeDbMock.Object);
        var messageDelivered = new MessageDelivered(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            deliveredAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        updateCaptor.Upsert.Should().BeFalse();
        var (_, updateDoc) = Render(updateCaptor);

        var deliveredAtFieldName = Field<MessageQueryModel>(x => x.DeliveredAt!);
        var conversationIdFieldName = Field<MessageQueryModel>(x => x.ConversationId);
        var senderIdFieldName = Field<MessageQueryModel>(x => x.SenderId);
        var contentFieldName = Field<MessageQueryModel>(x => x.Content);

        updateDoc.Contains("$max").Should().BeTrue();
        updateDoc["$max"].AsBsonDocument.Contains(deliveredAtFieldName).Should().BeTrue();

        var set = updateDoc["$set"].AsBsonDocument;
        set.Contains(conversationIdFieldName).Should().BeFalse();
        set.Contains(senderIdFieldName).Should().BeFalse();
        set.Contains(contentFieldName).Should().BeFalse();
    }

    [Fact(DisplayName = nameof(DoesNotRegressAfterRead))]
    [Trait("Chat/Query", "MessageDelivered - Projection")]
    public async Task DoesNotRegressAfterRead()
    {
        var (synchronizeDbMock, updateCaptor) = CaptureUpdate<MessageQueryModel>();
        var projection = new MessageDeliveredProjection(synchronizeDbMock.Object);
        var messageDelivered = new MessageDelivered(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            deliveredAt: DateTime.UtcNow);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var (filterDoc, _) = Render(updateCaptor);
        var statusFieldName = Field<MessageQueryModel>(x => x.Status);
        filterDoc.ToString().Should().Contain(statusFieldName).And.Contain("$ne");
    }
}
