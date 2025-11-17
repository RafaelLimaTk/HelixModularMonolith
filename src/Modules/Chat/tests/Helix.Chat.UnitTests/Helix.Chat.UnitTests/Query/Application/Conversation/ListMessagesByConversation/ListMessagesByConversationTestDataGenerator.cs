using Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListMessagesByConversation;

public class ListMessagesByConversationTestDataGenerator
{
    public static IEnumerable<object[]> GetInputsWithoutAllParameter(int times = 14)
    {
        var fixture = new ListMessagesByConversationTestFixture();
        var conversationId = fixture.NewId();
        var inputExample = ListMessagesByConversationTestFixture.GetExampleInput(conversationId);

        for (int i = 0; i < times; i++)
        {
            switch (i % 7)
            {
                case 0:
                    yield return new object[] {
                        new ListMessagesByConversationInput(conversationId)
                    };
                    break;
                case 1:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page
                        )
                    };
                    break;
                case 2:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage
                        )
                    };
                    break;
                case 3:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            search: inputExample.Search
                        )
                    };
                    break;
                case 4:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            search: inputExample.Search,
                            sort: inputExample.Sort
                        )
                    };
                    break;
                case 5:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page,
                            search: inputExample.Search,
                            sort: inputExample.Sort
                        )
                    };
                    break;
                case 6:
                    yield return new object[] {
                        new ListMessagesByConversationInput(
                            conversationId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            search: inputExample.Search,
                            sort: inputExample.Sort,
                            dir: inputExample.Dir
                        )
                    };
                    break;
            }
        }
    }
}
