namespace GcfOtdrParser.Services;

using GcfOtdrParser.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class OtdrApiLCient
{
    private const string DESCOPETOKENKEY = "descopeToken";
    private const string DESCOPEURLKEY = "descopeUrl";
    private const string DESCOPEUSERKEY = "descopeUser";
    private const string DESCOPEPASSWORDKEY = "descopePassword";
    private const string OTDRAPIURLKEY = "otdrApiUrl";

    private readonly string descopeToken;
    private readonly string descopeUrl;
    private readonly string descopeUser;
    private readonly string descopePassword;
    private readonly string otdrApiUrl;
    private readonly ILogger _logger;

    private readonly HttpClient _httpClient = new ();
    private string sessionJwt = string.Empty;

    public OtdrApiLCient(ILogger logger)
    {
        descopeToken = GetSecretValue(DESCOPETOKENKEY);
        descopeUser = GetSecretValue(DESCOPEUSERKEY);
        descopePassword = GetSecretValue(DESCOPEPASSWORDKEY);
        descopeUrl = GetSecretValue(DESCOPEURLKEY).TrimEnd('/');
        otdrApiUrl = GetSecretValue(OTDRAPIURLKEY).TrimEnd('/');
        _logger = logger;
    }

    public async Task<string> DescopeLogin()
    {
        _logger.LogDebug("Descope Login {DescopeUrl}", descopeUrl);

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {descopeToken}");

        var login = new { loginId = descopeUser, password = descopePassword };
        var response = await _httpClient.PostAsJsonAsync(descopeUrl, login);
        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            sessionJwt = JsonSerializer.Deserialize<JsonElement>(responseContent).GetProperty("sessionJwt").GetString();
            return sessionJwt;
        }
        else
        {
            _logger.LogError("Descope login failed with status code {StatusCode}", response.StatusCode);
        }

        return string.Empty;
    }

    public async Task<bool> PostAtdEntry(AtdEntry atdEntry)
    {

        _logger.LogDebug("Posting AtdEntry to {Url}", otdrApiUrl);

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {sessionJwt}");
        var response = await _httpClient.PostAsJsonAsync($"{otdrApiUrl}/atd", atdEntry);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    private static string GetSecretValue(string key)
    {
        switch (key)
        {
            case DESCOPETOKENKEY:
                return Environment.GetEnvironmentVariable("DESCOPE_TOKEN") ?? string.Empty;
            case DESCOPEURLKEY:
                return Environment.GetEnvironmentVariable("DESCOPE_URL") ?? string.Empty;
            case DESCOPEUSERKEY:
                return Environment.GetEnvironmentVariable("DESCOPE_USER") ?? string.Empty;
            case DESCOPEPASSWORDKEY:
                return Environment.GetEnvironmentVariable("DESCOPE_PASSWORD") ?? string.Empty;
            case OTDRAPIURLKEY:
                return Environment.GetEnvironmentVariable("OTDR_API_URL") ?? string.Empty;
            default:
                return string.Empty;
        }
    }
}
