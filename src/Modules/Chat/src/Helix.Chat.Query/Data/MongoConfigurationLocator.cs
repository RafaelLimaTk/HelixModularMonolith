using Helix.Chat.Query.Utils;

namespace Helix.Chat.Query.Data;

public static class MongoConfigurationLocator
{
    public static IReadDbConfiguration[] FindAllConfigurations()
    {
        var assembly = AssemblyRef.Value;

        return assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                       !t.IsInterface &&
                       typeof(IReadDbConfiguration).IsAssignableFrom(t))
            .Select(t => (IReadDbConfiguration)Activator.CreateInstance(t)!)
            .ToArray();
    }

    public static Dictionary<Type, string> GetCollectionMappings()
        => FindAllConfigurations()
            .ToDictionary(c => c.ModelType, c => c.CollectionName);
}
