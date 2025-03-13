using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RootsCore.Models;

namespace RootsCore.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public GeminiAiService(IHttpClientFactory factory, IOptions<AiSettings> aiSettings)
    {
        var settings = aiSettings.Value;
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        var apiKey = settings.Gemini.ApiKey ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GeneratePromptAsync(string context)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = "You are a genealogy assistant for GlobalRoots. Generate a location-specific family query." },
                        new { text = context }
                    }
                }
            },
            generationConfig = new { maxOutputTokens = 50 }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("models/gemini-1.5-flash:generateContent", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);
        return result.Candidates[0].Content.Parts[0].Text.Trim();
    }

    public async Task<List<string>> SuggestPromptsAsync(List<string> missingData)
    {
        var suggestions = new List<string>();
        foreach (var data in missingData)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = "You are a genealogy assistant for GlobalRoots. Suggest a prompt for missing family data tied to a location." },
                            new { text = $"Missing: {data}" }
                        }
                    }
                },
                generationConfig = new { maxOutputTokens = 50 }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("models/gemini-1.5-flash:generateContent", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);
            suggestions.Add(result.Candidates[0].Content.Parts[0].Text.Trim());
        }
        return suggestions;
    }
}

public class GeminiResponse
{
    public GeminiCandidate[] Candidates { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent Content { get; set; }
}

public class GeminiContent
{
    public GeminiPart[] Parts { get; set; }
}

public class GeminiPart
{
    public string Text { get; set; }
}