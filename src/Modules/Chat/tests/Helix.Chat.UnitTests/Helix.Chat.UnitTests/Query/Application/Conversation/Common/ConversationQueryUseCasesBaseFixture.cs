using Helix.Chat.Query.Data.Repositories.Interfaces;
using Helix.Chat.Query.Enums;
using Helix.Chat.Query.Models;
using Helix.Chat.UnitTests.Query.Common;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.Common;

public class ConversationQueryUseCasesBaseFixture : QueryBaseFixture
{
    public Mock<IConversationsReadRepository> GetConversationReadRepositoryMock()
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
}
