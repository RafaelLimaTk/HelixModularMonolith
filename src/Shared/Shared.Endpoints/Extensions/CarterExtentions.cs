using Carter;
using Microsoft.Extensions.DependencyInjection;
using Shared.Endpoints.Configurations.Policies;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Shared.Endpoints.Extensions;

public static class CarterExtentions
{
    public static IServiceCollection AddCarterWithAssemblies
        (this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddCarter(configurator: config =>
        {
            foreach (var assembly in assemblies)
            {
                var modules = assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ICarterModule))).ToArray();

                config.WithModules(modules);
            }
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = new JsonSnakeCasePolicy();
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }
}