using System.Reflection;

namespace Helix.Chat.Query.Data;

public static class CollectionNames
{
    public const string Conversations = "conversations";
    public const string Messages = "messages";

    private static readonly Lazy<List<string>> _collectionNames = new(() =>
        typeof(CollectionNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Distinct()
            .OrderBy(x => x)
            .ToList()
    );

    public static List<string> ListCollectionNames()
        => _collectionNames.Value;
}