using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RootsCore.Models;

namespace RootsCore.Services;

public class ChatGptAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public ChatGptAiService(IHttpClientFactory factory, IOptions<AiSettings> aiSettings)
    {
        var settings = aiSettings.Value;
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        var apiKey = settings.OpenAI.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GeneratePromptAsync(string context)
    {
        var requestBody = new
        {
            model = "gpt-4o",
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
        var result = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);
        return result.Choices[0].Message.Content.Trim();
    }

    public async Task<List<string>> SuggestPromptsAsync(List<string> missingData)
    {
        var suggestions = new List<string>();
        foreach (var data in missingData)
        {
            var requestBody = new
            {
                model = "gpt-4o",
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
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);
            suggestions.Add(result.Choices[0].Message.Content.Trim());
        }
        return suggestions;
    }
}

public class OpenAiResponse
{
    public OpenAiChoice[] Choices { get; set; }
}

public class OpenAiChoice
{
    public OpenAiMessage Message { get; set; }
}

public class OpenAiMessage
{
    public string Content { get; set; }
}