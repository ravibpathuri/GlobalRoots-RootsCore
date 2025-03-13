
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RootsCore.Models;

namespace RootsCore.Services;

public class GrokAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public GrokAiService(IHttpClientFactory factory, IOptions<AiSettings> aiSettings)
    {
        var settings = aiSettings.Value;
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.xai.com/v1/"); // Hypothetical
        var apiKey = settings.Grok.ApiKey ?? Environment.GetEnvironmentVariable("GROK_API_KEY");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GeneratePromptAsync(string context)
    {
        var requestBody = new
        {
            model = "grok-3",
            max_tokens = 50,
            messages = new[]
            {
                new { role = "system", content = "You are a genealogy assistant for GlobalRoots. Generate a location-specific family query." },
                new { role = "user", content = context }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GrokResponse>(responseJson);
        return result.Choices[0].Message.Content.Trim();
    }

    public async Task<List<string>> SuggestPromptsAsync(List<string> missingData)
    {
        var suggestions = new List<string>();
        foreach (var data in missingData)
        {
            var requestBody = new
            {
                model = "grok-3",
                max_tokens = 50,
                messages = new[]
                {
                    new { role = "system", content = "You are a genealogy assistant for GlobalRoots. Suggest a prompt for missing family data tied to a location." },
                    new { role = "user", content = $"Missing: {data}" }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GrokResponse>(responseJson);
            suggestions.Add(result.Choices[0].Message.Content.Trim());
        }
        return suggestions;
    }
}

public class GrokResponse
{
    public GrokChoice[] Choices { get; set; }
}

public class GrokChoice
{
    public GrokMessage Message { get; set; }
}

public class GrokMessage
{
    public string Content { get; set; }
}