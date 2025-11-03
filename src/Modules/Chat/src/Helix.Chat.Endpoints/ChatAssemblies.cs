using System.Reflection;

namespace Helix.Chat.Endpoints;
public static class ChatAssemblies
{
    public static readonly Assembly[] All =
    [
        Utils.AssemblyRef.Value,
        Application.Utils.AssemblyRef.Value,
        Query.Utils.AssemblyRef.Value,
    ];
}