using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Projections.Message;

namespace Helix.Chat.IntegrationTests.Query.Projection.Message;

[Collection(nameof(MessageProjectionTestFixture))]
public class MessageSentProjectionTest(MessageProjectionTestFixture fixture)
{
    private readonly MessageProjectionTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(InsertEventMessageSent))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task InsertEventMessageSent()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var messageSent = _fixture.GetExampleMessageSentEvent();

        await projection.ProjectAsync(messageSent, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var savedMessage = await messagesCollection
            .Find(x => x.Id == messageSent.MessageId)
            .FirstOrDefaultAsync();
        savedMessage.Should().NotBeNull();
        savedMessage.Id.Should().Be(messageSent.MessageId);
        savedMessage.ConversationId.Should().Be(messageSent.ConversationId);
        savedMessage.SenderId.Should().Be(messageSent.SenderId);
        savedMessage.Content.Should().Be(messageSent.Content);
        savedMessage.SentAt.Should().BeCloseTo(messageSent.SentAt, TimeSpan.FromSeconds(1));
        savedMessage.Status.Should().Be(MessageStatus.Sent);
        savedMessage.DeliveredAt.Should().BeNull();
        savedMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(IsIdempotentDoesNotDuplicateOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task IsIdempotentDoesNotDuplicateOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var messageSent = _fixture.GetExampleMessageSentEvent();

        await projection.ProjectAsync(messageSent, CancellationToken.None);
        await projection.ProjectAsync(messageSent, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var allMessages = await messagesCollection
            .Find(x => x.Id == messageSent.MessageId)
            .ToListAsync();
        allMessages.Should().HaveCount(1);
    }

    [Fact(DisplayName = nameof(UpdatesContentAndSentAtOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task UpdatesContentAndSentAtOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var messageId = Guid.NewGuid();
        var newContent = _fixture.GetValidContent();
        var firstMessageSent = _fixture.GetExampleMessageSentEvent(messageId: messageId);
        await projection.ProjectAsync(firstMessageSent, CancellationToken.None);
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var firstSave = await messagesCollection
            .Find(x => x.Id == messageId)
            .FirstOrDefaultAsync();
        await Task.Delay(100);
        var secondMessageSent = new MessageSent(
            messageId,
            firstMessageSent.ConversationId,
            firstMessageSent.SenderId,
            newContent,
            DateTime.UtcNow);

        await projection.ProjectAsync(secondMessageSent, CancellationToken.None);

        var secondSave = await messagesCollection
            .Find(x => x.Id == messageId)
            .FirstOrDefaultAsync();
        secondSave.Should().NotBeNull();
        secondSave.Content.Should().Be(newContent);
        secondSave.SentAt.Should().BeCloseTo(secondMessageSent.SentAt, TimeSpan.FromSeconds(1));
        secondSave.ConversationId.Should().Be(firstMessageSent.ConversationId);
        secondSave.SenderId.Should().Be(firstMessageSent.SenderId);
        secondSave.Status.Should().Be(MessageStatus.Sent);
    }

    [Fact(DisplayName = nameof(PreservesDeliveredAtAndReadAtOnRetry))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task PreservesDeliveredAtAndReadAtOnRetry()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var messageId = Guid.NewGuid();
        var existingMessage = _fixture.GetExampleMessage(
            id: messageId,
            deliveredAt: DateTime.UtcNow.AddMinutes(-5),
            readAt: DateTime.UtcNow.AddMinutes(-2),
            status: MessageStatus.Read);
        await messagesCollection.InsertOneAsync(existingMessage);
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var messageSent = _fixture.GetExampleMessageSentEvent(messageId: messageId);

        await projection.ProjectAsync(messageSent, CancellationToken.None);

        var savedMessage = await messagesCollection
            .Find(x => x.Id == messageId)
            .FirstOrDefaultAsync();
        savedMessage.Should().NotBeNull();
        savedMessage.DeliveredAt.Should().BeCloseTo(existingMessage.DeliveredAt!.Value, TimeSpan.FromSeconds(1));
        savedMessage.ReadAt.Should().BeCloseTo(existingMessage.ReadAt!.Value, TimeSpan.FromSeconds(1));
        savedMessage.Status.Should().Be(MessageStatus.Sent);
    }

    [Fact(DisplayName = nameof(SetsAllRequiredFields))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task SetsAllRequiredFields()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var messageSent = _fixture.GetExampleMessageSentEvent();

        await projection.ProjectAsync(messageSent, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var savedMessage = await messagesCollection
            .Find(x => x.Id == messageSent.MessageId)
            .FirstOrDefaultAsync();
        savedMessage.Should().NotBeNull();
        savedMessage.Id.Should().Be(messageSent.MessageId);
        savedMessage.ConversationId.Should().Be(messageSent.ConversationId);
        savedMessage.SenderId.Should().Be(messageSent.SenderId);
        savedMessage.Content.Should().Be(messageSent.Content);
        savedMessage.SentAt.Should().BeCloseTo(messageSent.SentAt, TimeSpan.FromSeconds(1));
        savedMessage.Status.Should().Be(MessageStatus.Sent);
        savedMessage.DeliveredAt.Should().BeNull();
        savedMessage.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(HandlesMultipleMessagesInSameConversation))]
    [Trait("Chat/Integration/Query/Projections", "MessageSent - Projection")]
    public async Task HandlesMultipleMessagesInSameConversation()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var sync = _fixture.CreateSynchronizeDb(preserveData: true);
        var projection = new MessageSentProjection(sync);
        var conversationId = Guid.NewGuid();

        var message1 = _fixture.GetExampleMessageSentEvent(conversationId: conversationId);
        var message2 = _fixture.GetExampleMessageSentEvent(conversationId: conversationId);
        var message3 = _fixture.GetExampleMessageSentEvent(conversationId: conversationId);

        await projection.ProjectAsync(message1, CancellationToken.None);
        await projection.ProjectAsync(message2, CancellationToken.None);
        await projection.ProjectAsync(message3, CancellationToken.None);

        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var allMessages = await messagesCollection
            .Find(x => x.ConversationId == conversationId)
            .ToListAsync();
        allMessages.Should().HaveCount(3);
        allMessages.Should().OnlyContain(m => m.Status == MessageStatus.Sent);
    }
}