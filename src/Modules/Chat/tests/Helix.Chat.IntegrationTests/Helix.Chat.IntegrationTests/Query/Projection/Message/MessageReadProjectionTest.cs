using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Projections.Message;

namespace Helix.Chat.IntegrationTests.Query.Projection.Message;

[Collection(nameof(MessageProjectionTestFixture))]
public class MessageReadProjectionTest(MessageProjectionTestFixture fixture)
{
    private readonly MessageProjectionTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(UpdatesMessageToRead))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task UpdatesMessageToRead()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var existingMessage = await _fixture.InsertMessageInDatabase(
            _fixture.GetExampleMessage(
                deliveredAt: DateTime.UtcNow.AddMinutes(-5),
                status: MessageStatus.Delivered));
        var projection = new MessageReadProjection(sync);
        var readAt = DateTime.UtcNow;
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(
            existingMessage.Id,
            existingMessage.ConversationId,
            readerId,
            readAt);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == existingMessage.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.ReadAt.Should().BeCloseTo(readAt, TimeSpan.FromSeconds(1));
        updatedMessage.Status.Should().Be(MessageStatus.Read);
        updatedMessage.DeliveredAt.Should().BeCloseTo(existingMessage.DeliveredAt!.Value, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = nameof(CanTransitionFromSentDirectlyToRead))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task CanTransitionFromSentDirectlyToRead()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var existingMessage = await _fixture.InsertMessageInDatabase();
        var projection = new MessageReadProjection(sync);
        var readAt = DateTime.UtcNow;
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(
            existingMessage.Id,
            existingMessage.ConversationId,
            readerId,
            readAt);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == existingMessage.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.ReadAt.Should().BeCloseTo(readAt, TimeSpan.FromSeconds(1));
        updatedMessage.Status.Should().Be(MessageStatus.Read);
        updatedMessage.DeliveredAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(DoesNotCreateMessageIfNotExists))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task DoesNotCreateMessageIfNotExists()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageReadProjection(sync);
        var nonExistentMessageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(
            nonExistentMessageId,
            conversationId,
            readerId,
            DateTime.UtcNow);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var message = await messagesCollection
            .Find(x => x.Id == nonExistentMessageId)
            .FirstOrDefaultAsync();
        message.Should().BeNull();
    }

    [Fact(DisplayName = nameof(IsIdempotentForMessageRead))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task IsIdempotentForMessageRead()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var existingMessage = await _fixture.InsertMessageInDatabase();
        var projection = new MessageReadProjection(sync);
        var readAt = DateTime.UtcNow;
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(
            existingMessage.Id,
            existingMessage.ConversationId,
            readerId,
            readAt);

        await projection.ProjectAsync(messageRead, CancellationToken.None);
        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var allMessages = await messagesCollection
            .Find(x => x.Id == existingMessage.Id)
            .ToListAsync();
        allMessages.Should().HaveCount(1);
    }

    [Fact(DisplayName = nameof(UpdatesOnlyReadAtAndStatus))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task UpdatesOnlyReadAtAndStatus()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var originalMessage = await _fixture.InsertMessageInDatabase(
            _fixture.GetExampleMessage(
                deliveredAt: DateTime.UtcNow.AddMinutes(-5),
                status: MessageStatus.Delivered));
        var originalSentAt = originalMessage.SentAt;
        var originalContent = originalMessage.Content;
        var originalDeliveredAt = originalMessage.DeliveredAt;
        var projection = new MessageReadProjection(sync);
        var readAt = DateTime.UtcNow;
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(
            originalMessage.Id,
            originalMessage.ConversationId,
            readerId,
            readAt);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == originalMessage.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.ReadAt.Should().BeCloseTo(readAt, TimeSpan.FromSeconds(1));
        updatedMessage.Status.Should().Be(MessageStatus.Read);
        updatedMessage.SentAt.Should().BeCloseTo(originalSentAt, TimeSpan.FromSeconds(1));
        updatedMessage.Content.Should().Be(originalContent);
        updatedMessage.DeliveredAt.Should().BeCloseTo(originalDeliveredAt!.Value, TimeSpan.FromSeconds(1));
        updatedMessage.ConversationId.Should().Be(originalMessage.ConversationId);
        updatedMessage.SenderId.Should().Be(originalMessage.SenderId);
    }

    [Fact(DisplayName = nameof(ReadAtShouldBeAfterOrEqualDeliveredAt))]
    [Trait("Chat/Integration/Query/Projections", "MessageRead - Projection")]
    public async Task ReadAtShouldBeAfterOrEqualDeliveredAt()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var deliveredAt = DateTime.UtcNow.AddMinutes(-5);
        var message = await _fixture.InsertMessageInDatabase(
            _fixture.GetExampleMessage(
                deliveredAt: deliveredAt,
                status: MessageStatus.Delivered));
        var projection = new MessageReadProjection(sync);
        var readAt = deliveredAt.AddMinutes(1);
        var readerId = Guid.NewGuid();
        var messageRead = new MessageRead(message.Id, message.ConversationId, readerId, readAt);

        await projection.ProjectAsync(messageRead, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>();
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == message.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.ReadAt.Should().BeCloseTo(readAt, TimeSpan.FromSeconds(1));
        updatedMessage.DeliveredAt.Should().BeCloseTo(deliveredAt, TimeSpan.FromSeconds(1));
        updatedMessage.ReadAt.Should().BeAfter(updatedMessage.DeliveredAt.Value);
    }
}