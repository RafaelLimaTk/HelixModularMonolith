namespace Helix.Chat.Application.UseCases.Conversation.SendMessage;
public sealed record SendMessageOutput(Guid MessageId, DateTime SentAt)
{
    public static SendMessageOutput FromSendMessage(Guid messageId, DateTime sentAt)
        => new(messageId, sentAt);
}
