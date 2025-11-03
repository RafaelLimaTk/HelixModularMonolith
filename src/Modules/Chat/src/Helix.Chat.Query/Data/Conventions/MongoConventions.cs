using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
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
            new GuidStandardConvention(),
            new SnakeCaseElementNameConvention()
        };
        ConventionRegistry.Register("chat_conventions", pack, _ => true);
        _applied = true;
    }
}

internal sealed class GuidStandardConvention : IMemberMapConvention
{
    public string Name => "GuidStandard";
    public void Apply(BsonMemberMap m)
    {
        var t = Nullable.GetUnderlyingType(m.MemberType) ?? m.MemberType;
        if (t == typeof(Guid))
            m.SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}

internal sealed class SnakeCaseElementNameConvention : IMemberMapConvention
{
    public string Name => "SnakeCase";
    public void Apply(BsonMemberMap m)
    {
        var n = m.MemberName;
        var snake = n.ToSnakeCase();
        m.SetElementName(snake);
    }
}