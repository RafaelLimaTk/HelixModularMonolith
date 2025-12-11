using Shared.Domain.SeedWorks;
using System.Reflection;

namespace Shared.Infra.Outbox.Interfaces;

public interface ITypeResolver
{
    Type? ResolveType(string typeName);
    void RegisterType<T>() where T : DomainEvent;
    void RegisterAssembly(Assembly assembly);
}
