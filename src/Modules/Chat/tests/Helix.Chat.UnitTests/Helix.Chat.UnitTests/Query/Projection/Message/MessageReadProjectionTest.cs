using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections.Message;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;
using System.Linq.Expressions;

namespace Helix.Chat.UnitTests.Query.Projection.Message;
[Collection(nameof(QueryBaseFixture))]
public sealed class MessageReadProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpsertWithCorrectModelAndFilter))]
    [Trait("Chat/Query", "MessageRead - Projection")]
    public async Task ProjectCallsUpsertWithCorrectModelAndFilter()
    {
        var (syncMock, cap) = CaptureUpsert<MessageQueryModel>();
        var projection = new MessageReadProjection(syncMock.Object);

        var before = DateTime.UtcNow;
        var e = new MessageRead(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            readerId: _fixture.NewId(),
            readAt: DateTime.UtcNow);
        var after = DateTime.UtcNow;

        await projection.ProjectAsync(e, CancellationToken.None);

        syncMock.Verify(s => s.UpsertAsync(
            It.IsAny<MessageQueryModel>(),
            It.IsAny<Expression<Func<MessageQueryModel, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        cap.Model.Should().NotBeNull();
        cap.Model!.Id.Should().Be(e.MessageId);
        cap.Model.Status.Should().Be("Read");
        cap.Model.ReadAt.Should().BeAfter(before).And.BeBefore(after.AddSeconds(1));
        cap.Model.ConversationId.Should().Be(e.ConversationId);

        cap.Filter.Should().NotBeNull();
        var predicate = cap.Filter!.Compile();
        predicate(cap.Model).Should().BeTrue();
        predicate(new MessageQueryModel { Id = Guid.NewGuid() }).Should().BeFalse();
    }
}
