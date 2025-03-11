
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using RootsCore.Data;
using RootsCore.Services;

namespace RootsCore.Controllers;

[ApiController]
[Route("api/prompt")]
public class PromptController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    private readonly IAiService _aiService;

    public PromptController(Neo4jContext neo4j, IAiService aiService)
    {
        _neo4j = neo4j;
        _aiService = aiService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GeneratePrompt([FromBody] GenerateRequest request)
    {
        if (!_neo4j.IsConnected) return StatusCode(503, "Database unavailable");
        var prompt = await _aiService.GeneratePromptAsync(request.Context);
        return Ok(new { prompt });
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> SuggestPrompts()
    {
        if (!_neo4j.IsConnected) return StatusCode(503, "Database unavailable");

        var session = _neo4j.Session;
        var result = await session.RunAsync(
            "MATCH (p:Person) WHERE p.birth_date IS NULL OR NOT EXISTS((p)-[:PARENT_OF]-()) " +
            "RETURN p.name + ' has no birth date' AS missing_birth, " +
            "p.name + ' has no children' AS missing_children LIMIT 10"
        );
        var missingData = new List<string>();
        await result.ForEachAsync(record =>
        {
            if (record["missing_birth"] != null) missingData.Add(record["missing_birth"].As<string>());
            if (record["missing_children"] != null) missingData.Add(record["missing_children"].As<string>());
        });

        var suggestions = await _aiService.SuggestPromptsAsync(missingData);
        return Ok(new { suggestions });
    }
}

public class GenerateRequest
{
    public string Context { get; set; }
}