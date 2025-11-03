using Helix.Chat.Query.Models;
using Helix.Chat.Query.Projections;
using Helix.Chat.UnitTests.Query.Common;
using Helix.Chat.UnitTests.Query.Projection.Common;
using System.Linq.Expressions;

namespace Helix.Chat.UnitTests.Query.Projection.Message;

[Collection(nameof(QueryBaseFixture))]
public sealed class MessageSentProjectionTest(QueryBaseFixture fixture) : ProjectionTestBase
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ProjectCallsUpsertWithCorrectModelAndFilter))]
    [Trait("Chat/Query", "MessageSent - Projection")]
    public async Task ProjectCallsUpsertWithCorrectModelAndFilter()
    {
        var (syncMock, cap) = CaptureUpsert<MessageQueryModel>();
        var projection = new MessageSentProjection(syncMock.Object);

        var @event = new MessageSent(
            messageId: _fixture.NewId(),
            conversationId: _fixture.NewId(),
            senderId: _fixture.NewId(),
            content: _fixture.AnyContent(),
            sentAt: DateTime.UtcNow);

        await projection.ProjectAsync(@event, CancellationToken.None);

        syncMock.Verify(s => s.UpsertAsync(
            It.IsAny<MessageQueryModel>(),
            It.IsAny<Expression<Func<MessageQueryModel, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        cap.Model.Should().NotBeNull();
        cap.Model!.Id.Should().Be(@event.MessageId);
        cap.Model.ConversationId.Should().Be(@event.ConversationId);
        cap.Model.SenderId.Should().Be(@event.SenderId);
        cap.Model.Content.Should().Be(@event.Content);
        cap.Model.SentAt.Should().BeCloseTo(@event.SentAt, TimeSpan.FromSeconds(1));
        cap.Model.Status.Should().Be("Sent");

        cap.Filter.Should().NotBeNull();
        var predicate = cap.Filter!.Compile();
        predicate(cap.Model).Should().BeTrue();
        predicate(new MessageQueryModel { Id = Guid.NewGuid() }).Should().BeFalse();
    }
}
