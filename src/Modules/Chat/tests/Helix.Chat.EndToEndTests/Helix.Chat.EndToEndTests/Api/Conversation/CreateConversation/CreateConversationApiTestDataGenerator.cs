namespace Helix.Chat.EndToEndTests.Api.Conversation.CreateConversation;

public class CreateConversationApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs()
    {
        var fixture = new CreateConversationApiTestFixture();
        var invalidInputsList = new List<object[]>();
        var totalInvalidCases = 3;

        for (int index = 0; index < totalInvalidCases; index++)
        {
            switch (index % totalInvalidCases)
            {
                case 0:
                    var input1 = fixture.GetInvalidInput();
                    input1 = input1 with { Title = fixture.GetInvalidTitleTooShort() };
                    invalidInputsList.Add(new object[]
                    {
                        input1,
                        "Title should be at least 3 characters long"
                    });
                    break;
                case 1:
                    var input2 = fixture.GetInvalidInput();
                    input2 = input2 with { Title = fixture.GetInvalidTitleTooLong() };
                    invalidInputsList.Add(new object[]
                    {
                        input2,
                        "Title should be at most 128 characters long"
                    });
                    break;
                case 2:
                    var input3 = fixture.GetInvalidInput();
                    invalidInputsList.Add(new object[]
                    {
                        input3,
                        "Title should not be null or empty"
                    });
                    break;
            }
        }

        return invalidInputsList;
    }
}
