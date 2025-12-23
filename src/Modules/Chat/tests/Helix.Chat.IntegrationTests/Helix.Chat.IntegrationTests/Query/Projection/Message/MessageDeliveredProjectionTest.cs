using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Projections.Message;

namespace Helix.Chat.IntegrationTests.Query.Projection.Message;

[Collection(nameof(MessageProjectionTestFixture))]
public class MessageDeliveredProjectionTest(MessageProjectionTestFixture fixture)
{
    private readonly MessageProjectionTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(UpdatesMessageToDelivered))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task UpdatesMessageToDelivered()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var existingMessage = await _fixture.InsertMessageInDatabase();
        var projection = new MessageDeliveredProjection(sync);
        var deliveredAt = DateTime.UtcNow;
        var messageDelivered = new MessageDelivered(
            existingMessage.Id,
            existingMessage.ConversationId,
            deliveredAt);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == existingMessage.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.DeliveredAt.Should().BeCloseTo(deliveredAt, TimeSpan.FromSeconds(1));
        updatedMessage.Status.Should().Be(MessageStatus.Delivered);
        updatedMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(DoesNotCreateMessageIfNotExists))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task DoesNotCreateMessageIfNotExists()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageDeliveredProjection(sync);
        var nonExistentMessageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageDelivered = new MessageDelivered(nonExistentMessageId, conversationId, DateTime.UtcNow);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var message = await messagesCollection
            .Find(x => x.Id == nonExistentMessageId)
            .FirstOrDefaultAsync();
        message.Should().BeNull();
    }

    [Fact(DisplayName = nameof(IsIdempotentForMessageDelivered))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task IsIdempotentForMessageDelivered()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var existingMessage = await _fixture.InsertMessageInDatabase();
        var projection = new MessageDeliveredProjection(sync);
        var deliveredAt = DateTime.UtcNow;
        var messageDelivered = new MessageDelivered(
            existingMessage.Id,
            existingMessage.ConversationId,
            deliveredAt);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);
        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var allMessages = await messagesCollection
            .Find(x => x.Id == existingMessage.Id)
            .ToListAsync();
        allMessages.Should().HaveCount(1);
    }

    [Fact(DisplayName = nameof(UpdatesOnlyDeliveredAtAndStatus))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task UpdatesOnlyDeliveredAtAndStatus()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);

        var originalMessage = await _fixture.InsertMessageInDatabase();
        var originalSentAt = originalMessage.SentAt;
        var originalContent = originalMessage.Content;

        var projection = new MessageDeliveredProjection(sync);
        var deliveredAt = DateTime.UtcNow;
        var messageDelivered = new MessageDelivered(
            originalMessage.Id,
            originalMessage.ConversationId,
            deliveredAt);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == originalMessage.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.DeliveredAt.Should().BeCloseTo(deliveredAt, TimeSpan.FromSeconds(1));
        updatedMessage.Status.Should().Be(MessageStatus.Delivered);
        updatedMessage.SentAt.Should().BeCloseTo(originalSentAt, TimeSpan.FromSeconds(1));
        updatedMessage.Content.Should().Be(originalContent);
        updatedMessage.ConversationId.Should().Be(originalMessage.ConversationId);
        updatedMessage.SenderId.Should().Be(originalMessage.SenderId);
    }

    [Fact(DisplayName = nameof(CanTransitionFromSentToDelivered))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task CanTransitionFromSentToDelivered()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var message = await _fixture.InsertMessageInDatabase();
        var projection = new MessageDeliveredProjection(sync);
        var deliveredAt = DateTime.UtcNow;
        var messageDelivered = new MessageDelivered(
            message.Id,
            message.ConversationId,
            deliveredAt);

        await projection.ProjectAsync(messageDelivered, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == message.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.Status.Should().Be(MessageStatus.Delivered);
        updatedMessage.DeliveredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(OverwritesDeliveredAtOnSubsequentEvents))]
    [Trait("Chat/Integration/Query/Projections", "MessageDelivered - Projection")]
    public async Task OverwritesDeliveredAtOnSubsequentEvents()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var message = await _fixture.InsertMessageInDatabase();
        var projection = new MessageDeliveredProjection(sync);
        var firstDeliveredAt = DateTime.UtcNow.AddMinutes(-10);
        var firstEvent = new MessageDelivered(message.Id, message.ConversationId, firstDeliveredAt);
        await projection.ProjectAsync(firstEvent, CancellationToken.None);

        await Task.Delay(100);
        var secondDeliveredAt = DateTime.UtcNow;
        var secondEvent = new MessageDelivered(message.Id, message.ConversationId, secondDeliveredAt);

        await projection.ProjectAsync(secondEvent, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var updatedMessage = await messagesCollection
            .Find(x => x.Id == message.Id)
            .FirstOrDefaultAsync();
        updatedMessage.Should().NotBeNull();
        updatedMessage.DeliveredAt.Should().BeCloseTo(secondDeliveredAt, TimeSpan.FromSeconds(1));
        updatedMessage.DeliveredAt.Should().BeAfter(firstDeliveredAt);
    }
}