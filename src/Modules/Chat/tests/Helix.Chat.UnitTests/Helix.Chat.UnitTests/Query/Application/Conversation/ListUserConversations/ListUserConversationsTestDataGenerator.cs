using Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListUserConversations;

public class ListUserConversationsTestDataGenerator
{
    public static IEnumerable<object[]> GetInputsWithoutAllParameter(int times = 14)
    {
        var fixture = new ListUserConversationsTestFixture();
        var inputExample = fixture.GetExampleInput(Guid.NewGuid());
        for (int i = 0; i < times; i++)
        {
            switch (i % 7)
            {
                case 0:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(userId: inputExample.UserId)
                    };
                    break;
                case 1:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page
                        )
                    };
                    break;
                case 2:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage
                        )
                    };
                    break;
                case 3:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            search: inputExample.Search
                        )
                    };
                    break;
                case 4:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            search: inputExample.Search,
                            sort: inputExample.Sort
                        )
                    };
                    break;
                case 5:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page,
                            search: inputExample.Search,
                            sort: inputExample.Sort
                        )
                    };
                    break;
                case 6:
                    yield return new object[]
                    {
                        new ListUserConversationsInput(
                            userId: inputExample.UserId,
                            page: inputExample.Page,
                            perPage: inputExample.PerPage,
                            dir: inputExample.Dir
                        )
                    };
                    break;
            }
        }
    }
}
