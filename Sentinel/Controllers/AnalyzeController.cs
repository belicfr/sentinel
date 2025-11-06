using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sentinel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly HttpClient _http;

    public record AnalysisRequest(
        string Language,
        string Message);

    public AnalyzeController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalysisRequest request)
    {
        String prompt =
            await System.IO.File.ReadAllTextAsync("Manifest/intro.md") +
            await System.IO.File.ReadAllTextAsync("Manifest/analysis-rules.md") +
            $"\nThe message is in {request.Language}"; 
        
        var payload = new
        {
            model = "mistral:7b",
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

        res.EnsureSuccessStatusCode();

        var json = await res.Content
            .ReadFromJsonAsync<JsonElement>();
        
        var content = json
            .GetProperty("response")
            .GetString();

        return Ok(content);
    }
}