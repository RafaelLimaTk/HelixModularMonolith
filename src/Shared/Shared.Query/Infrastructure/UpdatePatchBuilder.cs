using MongoDB.Driver;
using Shared.Query.Interfaces;
using System.Collections.Concurrent;
using System.Reflection;

namespace Shared.Query.Infrastructure;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PatchAlwaysAttribute : Attribute { }

public static class UpdatePatchBuilder
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propsCache = new();

    public static UpdateDefinition<T> Build<T>(T model) where T : IQueryModel
    {
        var props = _propsCache.GetOrAdd(typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        var ub = Builders<T>.Update;
        var updates = new List<UpdateDefinition<T>>();

        var idProp = props.FirstOrDefault(p => p.Name == "Id")
                    ?? throw new InvalidOperationException($"{typeof(T).Name} precisa da propriedade pública 'Id'.");

        var idVal = idProp.GetValue(model)
                    ?? throw new InvalidOperationException($"'Id' não pode ser nulo em {typeof(T).Name}.");

        if (IsDefault(idVal, idProp.PropertyType))
            throw new InvalidOperationException($"'Id' precisa estar preenchido em {typeof(T).Name}.");

        updates.Add(ub.SetOnInsert(idProp.Name, idVal));

        foreach (var p in props)
        {
            if (p == idProp) continue;

            var value = p.GetValue(model);
            var always = p.GetCustomAttribute<PatchAlwaysAttribute>() != null;

            if (!always && IsDefault(value, p.PropertyType)) continue;

            updates.Add(ub.Set(p.Name, value));
        }

        return updates.Count == 1 ? updates[0] : ub.Combine(updates);
    }

    private static bool IsDefault(object? value, Type type)
    {
        if (value is null) return true;

        if (type == typeof(string)) return string.IsNullOrWhiteSpace((string)value);
        if (type == typeof(Guid)) return (Guid)value == Guid.Empty;
        if (type == typeof(DateTime)) return (DateTime)value == default;
        if (type.IsEnum) return Convert.ToInt64(value) == 0;

        if (type.IsValueType)
        {
            var def = Activator.CreateInstance(type);
            return value.Equals(def);
        }

        if (value is System.Collections.IEnumerable e)
        {
            var it = e.GetEnumerator();
            var hasAny = it.MoveNext();
            (it as IDisposable)?.Dispose();
            return !hasAny;
        }

        return false;
    }
}