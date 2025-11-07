using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly HttpClient _http;

    private const string ModelName = "gemma2:2b-instruct-q4_K_M";

    public record AnalysisRequest(
        string Language,
        string Message);
    
    public record ManyAnalysisRequest(
        string Language,
        List<string> Messages);

    public AnalyzeController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalysisRequest request)
    {
        string prompt =
            await System.IO.File.ReadAllTextAsync("Manifest/intro.md") +
            await System.IO.File.ReadAllTextAsync("Manifest/analysis-rules.md") +
            $"\nThe message is in {request.Language}"; 
        
        var payload = new
        {
            model = ModelName,
            format = "json",
            stream = false,
            prompt = prompt +
                     "\nDo not say anything other than your JSON response." +
                     "\nDo not change its structure." +
                     "\nDo not comment." +
                     "\n\nENTRY:\n" + "{ \"message\": \""+ request.Message + "\" }"
        };

        var res = await _http
            .PostAsJsonAsync("http://localhost:11434/api/generate", payload);

        res.EnsureSuccessStatusCode(); // Lève une exception si code ≠ 2xx

        var json = await res.Content
            .ReadFromJsonAsync<JsonElement>();
        
        var content = json
            .GetProperty("response")
            .GetString();

        return Ok(content);
    }

    [HttpPost]
    [Route("many")]
    public async Task<IActionResult> AnalyzeMany(
        [FromBody] ManyAnalysisRequest request)
    {
        string prompt =
            await System.IO.File.ReadAllTextAsync("Manifest/intro.md") +
            await System.IO.File.ReadAllTextAsync("Manifest/analysis-rules.md") +
            $"\nThe message is in {request.Language}";

        List<string> preparedMessages = request.Messages
            .Select(m => "{" + $"\"message\": \"{m}\"" + "}")
            .ToList();
        
        var payload = new
        {
            model = ModelName,
            format = "json",
            stream = false,
            prompt = prompt +
                     "\nDo not say anything other than your JSON response." +
                     "\nDo not change its structure." +
                     "\nDo not comment." +
                     "\n\nENTRY:\n" + "[ " + string.Join(',', preparedMessages) + " ]"
        };

        var res = await _http
            .PostAsJsonAsync("http://localhost:11434/api/generate", payload);

        res.EnsureSuccessStatusCode(); // Lève une exception si code ≠ 2xx

        var json = await res.Content
            .ReadFromJsonAsync<JsonElement>();
        
        var content = json
            .GetProperty("response")
            .GetString();

        return Ok(content);
    }
}