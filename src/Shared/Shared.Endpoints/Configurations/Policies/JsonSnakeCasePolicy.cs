using Shared.Endpoints.Extensions.String;
using System.Text.Json;

namespace Shared.Endpoints.Configurations.Policies;

public sealed class JsonSnakeCasePolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => name.ToSnakeCase();
}