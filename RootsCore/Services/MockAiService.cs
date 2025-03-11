
namespace RootsCore.Services;

public class MockAiService : IAiService
{
    public Task<string> GeneratePromptAsync(string context)
    {
        return Task.FromResult($"Who is related to {context}?");
    }

    public Task<List<string>> SuggestPromptsAsync(List<string> missingData)
    {
        var suggestions = missingData.Select(d => $"What is the {d}?").ToList();
        return Task.FromResult(suggestions);
    }
}