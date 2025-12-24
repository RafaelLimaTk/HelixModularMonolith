using Microsoft.AspNetCore.WebUtilities;
using Shared.Endpoints.Configurations.Policies;
using System.Text;
using System.Text.Json;

namespace Helix.Chat.EndToEndTests.Base;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _defaultSerializerOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _defaultSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new JsonSnakeCasePolicy(),
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<(HttpResponseMessage?, TOutput?)> Post<TOutput>(
        string url,
        object input)
        where TOutput : class
    {
        var response = await _httpClient.PostAsync(
            url,
            new StringContent(
                JsonSerializer.Serialize(
                    input,
                    _defaultSerializerOptions
                ),
                Encoding.UTF8,
                "application/json"
            )
        );

        var output = await GetOutput<TOutput>(response);
        return (response, output);
    }

    public async Task<(HttpResponseMessage?, TOutput?)> Get<TOutput>(
        string route,
        object? queryStringParameterObject = null
    ) where TOutput : class
    {
        var url = PrepareGetRoute(route, queryStringParameterObject);
        var response = await _httpClient.GetAsync(url);
        var output = await GetOutput<TOutput>(response);
        return (response, output);
    }

    public async Task<(HttpResponseMessage?, TOutput?)> Delete<TOutput>(
        string route
    ) where TOutput : class
    {
        var response = await _httpClient.DeleteAsync(route);
        var output = await GetOutput<TOutput>(response);
        return (response, output);
    }

    public async Task<(HttpResponseMessage?, TOutput?)> Put<TOutput>(
        string route,
        object input
    ) where TOutput : class
    {
        var response = await _httpClient.PutAsync(
            route,
            new StringContent(
                JsonSerializer.Serialize(
                    input,
                    _defaultSerializerOptions
                ),
                Encoding.UTF8,
                "application/json"
            )
        );

        var output = await GetOutput<TOutput>(response);
        return (response, output);
    }

    private async Task<TOutput?> GetOutput<TOutput>(HttpResponseMessage response)
        where TOutput : class
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        TOutput? output = null;
        if (string.IsNullOrWhiteSpace(responseContent) == false)
            output = JsonSerializer.Deserialize<TOutput>(
                responseContent,
                _defaultSerializerOptions
            );

        return output;
    }

    private string PrepareGetRoute(
        string route,
        object? queryStringParameterObject
    )
    {
        if (queryStringParameterObject is null)
            return route;

        var parameters = JsonSerializer.Serialize(
            queryStringParameterObject,
            _defaultSerializerOptions
        );
        var parametersDictionary = Newtonsoft.Json.JsonConvert
            .DeserializeObject<Dictionary<string, object>>(parameters);

        var stringParametersDictionary = parametersDictionary?
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.ToString()
            );

        return QueryHelpers.AddQueryString(route, stringParametersDictionary!);
    }
}