namespace Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;

public class ListUserConversations(IConversationsReadRepository conversationsReadRepository) : IListUserConversations
{
    private readonly IConversationsReadRepository _conversationsReadRepository = conversationsReadRepository;

    public async Task<ListUserConversationsOutput> Handle(
        ListUserConversationsInput input,
        CancellationToken cancellationToken)
    {
        var specification = new ListUserConversationsSpecification(
            input.UserId, input.Page, input.PerPage, input.Search, input.Sort, input.Dir);

        var searchResult = await _conversationsReadRepository.Search(specification, cancellationToken);
        var output = new ListUserConversationsOutput(
            page: searchResult.CurrentPage,
            perPage: searchResult.PerPage,
            total: searchResult.Total,
            items: searchResult.Items
        );
        return output;
    }
}
