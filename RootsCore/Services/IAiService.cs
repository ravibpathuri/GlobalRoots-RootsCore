// RootsCore/Services/IAiService.cs
namespace RootsCore.Services
{
    public interface IAiService
    {
        Task<string> GeneratePromptAsync(string context);
        Task<List<string>> SuggestPromptsAsync(List<string> missingData);
    }
}