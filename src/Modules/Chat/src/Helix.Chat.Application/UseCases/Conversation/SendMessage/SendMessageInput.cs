using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.SendMessage;
public sealed record SendMessageInput(
    Guid ConversationId,
    Guid SenderId,
    string Content
) : IRequest<SendMessageOutput>;
