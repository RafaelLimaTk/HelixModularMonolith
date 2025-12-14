using Helix.Chat.Query.Enums;

namespace Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.Common;

public class ConversationQueryUseCasesBaseFixture : QueryBaseFixture
{
    public string GetValidContent() => Faker.Lorem.Sentence();

    public MessageQueryModel GetExampleMessage(
        Guid conversationId,
        string? content = null,
        Guid? senderId = null,
        DateTime? sentAt = null,
        DateTime? deliveredAt = null,
        DateTime? readAt = null,
        string? status = null)
    {
        var sent = sentAt ?? DateTime.UtcNow;

        return new MessageQueryModel
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId ?? Guid.NewGuid(),
            Content = content ?? GetValidContent(),
            SentAt = sent,
            DeliveredAt = deliveredAt,
            ReadAt = readAt,
            Status = status ?? MessageStatus.Sent
        };
    }

    public List<MessageQueryModel> GetExampleMessagesListByContents(
        Guid conversationId,
        List<string>? contents = null)
    {
        var baseTime = DateTime.UtcNow;
        var messages = new List<MessageQueryModel>();
        contents ??= [
            GetValidContent(),
            GetValidContent(),
            GetValidContent(),
            GetValidContent(),
            GetValidContent()
        ];
        for (int i = 0; i < contents.Count; i++)
        {
            var sentAt = baseTime.AddMinutes(-i);
            var deliveredAt = i % 2 == 0 ? sentAt.AddSeconds(10) : (DateTime?)null;
            var readAt = i % 3 == 0 ? sentAt.AddSeconds(30) : (DateTime?)null;

            string status = readAt.HasValue ? MessageStatus.Read : deliveredAt.HasValue ? MessageStatus.Delivered : MessageStatus.Sent;
            messages.Add(GetExampleMessage(
                conversationId,
                content: contents[i],
                sentAt: sentAt,
                deliveredAt: deliveredAt,
                readAt: readAt,
                status: status
            ));
        }

        return messages;
    }

    public List<MessageQueryModel> CreateExampleMessagesList(Guid conversationId, int length = 5)
        => Enumerable.Range(0, length)
            .Select(_ => GetExampleMessage(conversationId))
            .ToList();
}
