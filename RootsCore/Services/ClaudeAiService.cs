
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RootsCore.Models;

namespace RootsCore.Services;

public class ClaudeAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public ClaudeAiService(IHttpClientFactory factory, IOptions<AiSettings> aiSettings)
    {
        var settings = aiSettings.Value;
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        var apiKey = settings.Claude.ApiKey ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GeneratePromptAsync(string context)
    {
        var requestBody = new
        {
            model = "claude-3-5-sonnet-20241022",
            max_tokens = 50,
            messages = new[]
            {
                new { role = "system", content = "You are a genealogy assistant for GlobalRoots. Generate a location-specific family query." },
                new { role = "user", content = context }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("messages", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ClaudeResponse>(responseJson);
        return result.Content[0].Text.Trim();
    }

    public async Task<List<string>> SuggestPromptsAsync(List<string> missingData)
    {
        var suggestions = new List<string>();
        foreach (var data in missingData)
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 50,
                messages = new[]
                {
                    new { role = "system", content = "You are a genealogy assistant for GlobalRoots. Suggest a prompt for missing family data tied to a location." },
                    new { role = "user", content = $"Missing: {data}" }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ClaudeResponse>(responseJson);
            suggestions.Add(result.Content[0].Text.Trim());
        }
        return suggestions;
    }
}

public class ClaudeResponse
{
    public ClaudeContent[] Content { get; set; }
}

public class ClaudeContent
{
    public string Text { get; set; }
}