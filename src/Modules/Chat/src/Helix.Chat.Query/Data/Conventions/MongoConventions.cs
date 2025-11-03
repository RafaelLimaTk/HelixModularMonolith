using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Shared.Query.Extensions;

namespace Helix.Chat.Query.Data.Conventions;
public static class MongoConventions
{
    static bool _applied;
    public static void Apply()
    {
        if (_applied) return;
        var pack = new ConventionPack {
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String),
            new SnakeCaseElementNameConvention()
        };
        ConventionRegistry.Register("chat_conventions", pack, _ => true);
        _applied = true;
    }
}

sealed class SnakeCaseElementNameConvention : IMemberMapConvention
{
    public string Name => "SnakeCase";
    public void Apply(BsonMemberMap m)
    {
        var n = m.MemberName;
        var snake = n.ToSnakeCase();
        m.SetElementName(snake);
    }
}