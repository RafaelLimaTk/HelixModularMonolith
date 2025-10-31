using MediatR;

namespace Helix.Chat.Application.UseCases.Conversation.SendMessage;
public interface ISendMessage
    : IRequestHandler<SendMessageInput, SendMessageOutput>
{ }
