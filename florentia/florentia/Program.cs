using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string openAIToken = "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"; // Reemplaza esto con tu token de API OpenAI
builder.Services.AddSingleton<OpenAIService>(new OpenAIService(openAIToken));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/chat", async (ChatRequest request, OpenAIService openAIService) =>{
    var aiResponse = await openAIService.GetResponseAsync(request.userMessage);
    return Results.Ok(new { request.userMessage, aiResponse });
})
.WithName("ChatWithAI")
.WithOpenApi();

app.Run();

public class OpenAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 100,
            temperature = 0.7
        };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(OpenAiUrl, content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var completion = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return completion?.Trim() ?? string.Empty;
    }
}

public class ChatRequest
{
    public string userMessage { get; set; }
}
