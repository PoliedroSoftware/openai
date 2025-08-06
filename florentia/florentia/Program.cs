using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var envFiles = new[]
{
    ".env",                     
    $".env.{environment}",      
    ".env.local",               
    $".env.{environment}.local" 
};

foreach (var file in envFiles)
{
    if (File.Exists(file))
    {
        Env.Load(file);
        Console.WriteLine($"Cargando variables de entorno desde: {file}");
    }
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? openAIToken = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(openAIToken))
{
    openAIToken = builder.Configuration["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(openAIToken))
    {
        throw new InvalidOperationException(
            "El token de OpenAI no está configurado. Establece la variable de entorno 'OPENAI_API_KEY' " +
            "en el archivo .env o configúrala en appsettings.json.");
    }
}

string openAIModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-3.5-turbo";
string maxTokensStr = Environment.GetEnvironmentVariable("OPENAI_MAX_TOKENS") ?? "100";
string temperatureStr = Environment.GetEnvironmentVariable("OPENAI_TEMPERATURE") ?? "0.7";

int maxTokens = int.TryParse(maxTokensStr, out var tokens) ? tokens : 100;
double temperature = double.TryParse(temperatureStr, out var temp) ? temp : 0.7;

builder.Services.AddSingleton<OpenAIService>(new OpenAIService(openAIToken, openAIModel, maxTokens, temperature));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapPost("/chat", async (ChatRequest request, OpenAIService openAIService) => 
{
    try
    {
        var aiResponse = await openAIService.GetResponseAsync(request.userMessage);
        return Results.Ok(new { 
            request.userMessage, 
            aiResponse
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al procesar la solicitud",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("ChatWithAI")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();

public class OpenAIService
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly double _temperature;
    private readonly HttpClient _httpClient;
    private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(string apiKey, string model, int maxTokens, double temperature)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey), "Se requiere un token de API válido");
        _model = model;
        _maxTokens = maxTokens;
        _temperature = temperature;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            throw new ArgumentException("El mensaje no puede estar vacío", nameof(prompt));
        }

        var systemPrompt = "Eres Florentia, una profesora de inglés para niños. Siempre responde en español, usa lenguaje sencillo, fácil de entender y tono amigable.";
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = prompt }
        };
        
        var requestBody = new
        {
            model = _model,
            messages,
            max_tokens = _maxTokens,
            temperature = _temperature
        };
        
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync(OpenAiUrl, content);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var completion = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            
            return completion?.Trim() ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con OpenAI API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Error al procesar la respuesta de OpenAI: {ex.Message}", ex);
        }
    }
}

public class ChatRequest
{
    public required string userMessage { get; set; }
}
