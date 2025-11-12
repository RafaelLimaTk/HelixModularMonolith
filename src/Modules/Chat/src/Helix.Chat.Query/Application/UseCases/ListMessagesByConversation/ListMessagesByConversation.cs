namespace Helix.Chat.Query.Application.UseCases.ListMessagesByConversation;
public class ListMessagesByConversation(IMessagesReadRepository messagesReadRepository) : IListMessagesByConversation
{
    private readonly IMessagesReadRepository _messagesReadRepository = messagesReadRepository;

    public async Task<ListMessagesByConversationOutput> Handle(
        ListMessagesByConversationInput input,
        CancellationToken cancellationToken)
    {
        var specification = new ListMessagesByConversationSpecification(
            input.ConversationId, input.Page, input.PerPage, input.Search, input.Sort, input.Dir);

        var searchResult = await _messagesReadRepository.Search(specification, cancellationToken);
        var output = new ListMessagesByConversationOutput(
            page: searchResult.CurrentPage,
            perPage: searchResult.PerPage,
            total: searchResult.Total,
            items: searchResult.Items
        );
        return output;
    }
}
