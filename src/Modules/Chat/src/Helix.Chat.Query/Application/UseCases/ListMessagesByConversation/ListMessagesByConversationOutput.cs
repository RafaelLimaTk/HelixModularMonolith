using Helix.Chat.Query.Application.Common;

namespace Helix.Chat.Query.Application.UseCases.ListMessagesByConversation;
public sealed class ListMessagesByConversationOutput
    : PaginatedListOutput<MessageQueryModel>
{
    public ListMessagesByConversationOutput(
        int page,
        int perPage,
        int total,
        IReadOnlyList<MessageQueryModel> items)
        : base(page, perPage, total, items)
    { }
}