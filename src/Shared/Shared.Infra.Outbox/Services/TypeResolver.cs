using Microsoft.Extensions.Logging;
using Shared.Domain.SeedWorks;
using Shared.Infra.Outbox.Interfaces;
using System.Collections.Concurrent;
using System.Reflection;

namespace Shared.Infra.Outbox.Services;

public sealed class TypeResolver(ILogger<TypeResolver> logger) : ITypeResolver
{
    private readonly ConcurrentDictionary<string, Type> _typeCache = new();
    private readonly ILogger<TypeResolver> _logger = logger;

    public Type? ResolveType(string typeName)
    {
        if (_typeCache.TryGetValue(typeName, out var cachedType))
        {
            return cachedType;
        }

        var type = Type.GetType(typeName, throwOnError: false);

        if (type is not null)
        {
            _typeCache.TryAdd(typeName, type);
            return type;
        }

        var simpleTypeName = typeName.Contains(',')
            ? typeName.Split(',')[0].Trim()
            : typeName;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(simpleTypeName, throwOnError: false);
            if (type is not null && typeof(DomainEvent).IsAssignableFrom(type))
            {
                _typeCache.TryAdd(typeName, type);
                _logger.LogDebug(
                    "Resolved type '{TypeName}' from assembly '{AssemblyName}'",
                    simpleTypeName,
                    assembly.GetName().Name);
                return type;
            }
        }

        _logger.LogWarning("Could not resolve type '{TypeName}'", typeName);
        return null;
    }

    public void RegisterType<T>() where T : DomainEvent
    {
        var type = typeof(T);
        var fullName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        _typeCache.TryAdd(fullName, type);
        _typeCache.TryAdd(type.FullName ?? type.Name, type);
        _typeCache.TryAdd(type.Name, type);
    }

    public void RegisterAssembly(Assembly assembly)
    {
        var eventTypes = assembly.GetTypes()
            .Where(t => typeof(DomainEvent).IsAssignableFrom(t) &&
                       !t.IsAbstract &&
                       !t.IsInterface);

        foreach (var type in eventTypes)
        {
            var fullName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
            _typeCache.TryAdd(fullName, type);
            _typeCache.TryAdd(type.FullName ?? type.Name, type);
            _typeCache.TryAdd(type.Name, type);
        }

        _logger.LogInformation(
            "Registered {Count} domain event types from assembly '{AssemblyName}'",
            eventTypes.Count(),
            assembly.GetName().Name);
    }
}