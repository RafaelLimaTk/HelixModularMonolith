using Helix.Chat.Query.Data.Repositories.Interfaces;
using Helix.Chat.Query.Enums;
using Helix.Chat.UnitTests.Query.Common;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.Common;

public class ConversationQueryUseCasesBaseFixture : QueryBaseFixture
{
    public Mock<IConversationsReadRepository> GetConversationReadRepositoryMock()
        => new();

    public Mock<IMessagesReadRepository> GetMessagesReadRepositoryMock()
        => new();

    public ConversationQueryModel CreateConversationQueryModel(Guid userId, string? title = null)
        => new()
        {
            Id = NewId(),
            Title = title ?? GetValidTitle(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ParticipantIds = [userId, NewId()],
            LastMessage = new ConversationQueryModel.MessageSnapshot(
                NewId(),
                AnyContent(),
                DateTime.UtcNow,
                MessageStatus.Sent
            )
        };

    public List<ConversationQueryModel> CreateExampleConversationsList(Guid userId, int count)
        => Enumerable.Range(0, count)
            .Select(_ => CreateConversationQueryModel(userId))
            .ToList();

    public MessageQueryModel CreateMessageQueryModel(
        Guid conversationId,
        Guid? senderId = null,
        string? content = null,
        DateTime? sentAt = null,
        DateTime? deliveredAt = null,
        DateTime? readAt = null,
        string? status = null)
        => new MessageQueryModel
        {
            Id = NewId(),
            ConversationId = conversationId,
            SenderId = senderId ?? NewId(),
            Content = content ?? AnyContent(),
            SentAt = sentAt ?? DateTime.UtcNow,
            DeliveredAt = deliveredAt,
            ReadAt = readAt,
            Status = status ?? MessageStatus.Sent
        };

    public List<MessageQueryModel> CreateExampleMessagesList(Guid conversationId, int length = 5)
        => Enumerable.Range(0, length)
            .Select(_ => CreateMessageQueryModel(conversationId))
            .ToList();
}
