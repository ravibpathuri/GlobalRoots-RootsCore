
namespace RootsCore.Models;

public class AiSettings
{
    public string Vendor { get; set; } = "mock"; // Default to mock if unspecified

    public AiVendorSettings Grok { get; set; } = new AiVendorSettings();
    public AiVendorSettings Claude { get; set; } = new AiVendorSettings();
    public AiVendorSettings Gemini { get; set; } = new AiVendorSettings();
    public AiVendorSettings OpenAI { get; set; } = new AiVendorSettings();
    public AzureAiSettings Azure { get; set; } = new AzureAiSettings();
}

public class AiVendorSettings
{
    public string ApiKey { get; set; } = string.Empty;
}

public class AzureAiSettings : AiVendorSettings
{
    public string Endpoint { get; set; } = string.Empty;
}