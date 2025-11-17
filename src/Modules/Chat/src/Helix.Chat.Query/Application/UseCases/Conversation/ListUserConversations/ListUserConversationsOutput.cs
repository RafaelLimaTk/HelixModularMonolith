using Helix.Chat.Query.Application.Common;

namespace Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;
public sealed class ListUserConversationsOutput(
    int page,
    int perPage,
    int total,
    IReadOnlyList<ConversationQueryModel> items)
    : PaginatedListOutput<ConversationQueryModel>(page, perPage, total, items)
{ }
