using Helix.Chat.Domain.Events.Message;
using Helix.Chat.Query.Enums;

namespace Helix.Chat.IntegrationTests.Query.Projection.Message;

[CollectionDefinition(nameof(MessageProjectionTestFixture))]
public class MessageProjectionTestFixtureCollection
    : ICollectionFixture<MessageProjectionTestFixture>
{ }


public class MessageProjectionTestFixture
    : QueryBaseFixture
{
    public ISynchronizeDb CreateSynchronizeDb(bool preserveData = false)
    {
        var context = CreateReadDbContext(preserveData);
        return new MongoSynchronizeDb(context);
    }

    public string GetValidContent() => Faker.Lorem.Paragraph();

    public MessageQueryModel GetExampleMessage(
        Guid? id = null,
        Guid? conversationId = null,
        Guid? senderId = null,
        string? content = null,
        DateTime? sentAt = null,
        DateTime? deliveredAt = null,
        DateTime? readAt = null,
        string? status = null)
    {
        return new MessageQueryModel
        {
            Id = id ?? Guid.NewGuid(),
            ConversationId = conversationId ?? Guid.NewGuid(),
            SenderId = senderId ?? Guid.NewGuid(),
            Content = content ?? GetValidContent(),
            SentAt = sentAt ?? DateTime.UtcNow,
            DeliveredAt = deliveredAt,
            ReadAt = readAt,
            Status = status ?? MessageStatus.Sent
        };
    }

    public MessageSent GetExampleMessageSentEvent(
        Guid? messageId = null,
        Guid? conversationId = null,
        Guid? senderId = null,
        string? content = null,
        DateTime? sentAt = null)
    {
        return new MessageSent(
            messageId ?? Guid.NewGuid(),
            conversationId ?? Guid.NewGuid(),
            senderId ?? Guid.NewGuid(),
            content ?? GetValidContent(),
            sentAt ?? DateTime.UtcNow);
    }

    public MessageDelivered GetExampleMessageDeliveredEvent(
        Guid? messageId = null,
        Guid? conversationId = null,
        DateTime? deliveredAt = null)
    {
        return new MessageDelivered(
            messageId ?? Guid.NewGuid(),
            conversationId ?? Guid.NewGuid(),
            deliveredAt ?? DateTime.UtcNow);
    }

    public MessageRead GetExampleMessageReadEvent(
        Guid? messageId = null,
        Guid? conversationId = null,
        Guid? readerId = null,
        DateTime? readAt = null)
    {
        return new MessageRead(
            messageId ?? Guid.NewGuid(),
            conversationId ?? Guid.NewGuid(),
            readerId ?? Guid.NewGuid(),
            readAt ?? DateTime.UtcNow);
    }

    public async Task<MessageQueryModel> InsertMessageInDatabase(
        MessageQueryModel? message = null,
        bool preserveData = true)
    {
        var dbContext = CreateReadDbContext(preserveData);
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);

        var messageToInsert = message ?? GetExampleMessage();
        await messagesCollection.InsertOneAsync(messageToInsert);

        return messageToInsert;
    }
}
