using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Helix.Chat.Query.Data.Conventions;
public static class MongoConventions
{
    private static int _initialized;
    public static void Apply()
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0) return;

        BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

        var objectDisc = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
        BsonSerializer.RegisterSerializer(new ObjectSerializer(objectDisc, GuidRepresentation.Standard));

        var pack = new ConventionPack {
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String),
            new SnakeCaseElementNameConvention()
        };
        ConventionRegistry.Register("chat_conventions", pack, _ => true);
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