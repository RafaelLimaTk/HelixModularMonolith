using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.CreateConversation;
public sealed record CreateConversationInput(string Title)
    : IRequest<CreateConversationOutput>;