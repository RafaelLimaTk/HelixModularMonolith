using DomainEntity = Helix.Chat.Domain.Entities;

namespace Helix.Chat.Application.UseCases.Conversation.CreateConversation;
public sealed record CreateConversationOutput(Guid Id, DateTime CreatedAt)
{
    public static CreateConversationOutput FromConversation(
        DomainEntity.Conversation conversation)
        => new(conversation.Id, conversation.CreatedAt);
}
