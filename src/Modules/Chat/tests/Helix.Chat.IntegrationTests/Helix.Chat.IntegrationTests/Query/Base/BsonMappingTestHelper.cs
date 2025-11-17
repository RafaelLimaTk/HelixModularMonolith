using Helix.Chat.Query.Data.Configurations;
using Helix.Chat.Query.Data.Conventions;

namespace Helix.Chat.IntegrationTests.Query.Base;

internal static class BsonMappingTestHelper
{
    private static int _init;

    public static void EnsureMappingsRegistered()
    {
        if (Interlocked.Exchange(ref _init, 1) == 1) return;

        MongoConventions.Apply();

        new MessageConfiguration().ConfigureClassMap();
        new ConversationConfiguration().ConfigureClassMap();
    }
}
