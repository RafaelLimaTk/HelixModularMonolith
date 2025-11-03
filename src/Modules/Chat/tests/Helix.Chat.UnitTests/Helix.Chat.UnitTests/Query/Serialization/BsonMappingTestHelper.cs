using Helix.Chat.Query.Data.Conventions;
using Helix.Chat.Query.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Helix.Chat.UnitTests.Query.Serialization;

internal static class BsonMappingTestHelper
{
    private static volatile bool _done;
    private static readonly object _lock = new();

    public static void EnsureMappingsRegistered()
    {
        if (_done) return;

        lock (_lock)
        {
            if (_done) return;

            MongoConventions.Apply();

            if (!BsonClassMap.IsClassMapRegistered(typeof(MessageQueryModel)))
            {
                BsonClassMap.RegisterClassMap<MessageQueryModel>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(x => x.Id)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ConversationQueryModel)))
            {
                BsonClassMap.RegisterClassMap<ConversationQueryModel>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(x => x.Id)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ConversationQueryModel.MessageSnapshot)))
            {
                BsonClassMap.RegisterClassMap<ConversationQueryModel.MessageSnapshot>(cm =>
                {
                    cm.AutoMap();
                });
            }

            _done = true;
        }
    }
}