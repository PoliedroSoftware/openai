using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace github_ai
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                // Get the GitHub token from environment variables
                var credential = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                
                if (string.IsNullOrEmpty(credential))
                {
                    _logger.LogError("GITHUB_TOKEN environment variable is not set.");
                    return new BadRequestObjectResult("GITHUB_TOKEN environment variable is required.");
                }

                // Configure the GitHub AI endpoint and client
                var endpoint = new Uri("https://models.github.ai/inference");
                var model = "openai/gpt-5";

                var openAIOptions = new OpenAIClientOptions()
                {
                    Endpoint = endpoint
                };

                var client = new ChatClient(model, new ApiKeyCredential(credential), openAIOptions);

                // Get user input from query parameter or request body
                string userInput = "What is the capital of France?"; // Default question
                
                if (req.Method == "GET")
                {
                    userInput = req.Query["question"].FirstOrDefault() ?? userInput;
                }
                else if (req.Method == "POST")
                {
                    using var reader = new StreamReader(req.Body);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        userInput = body;
                    }
                }

                // Prepare the chat messages
                List<ChatMessage> messages = new List<ChatMessage>()
                {
                    new SystemChatMessage("You are a helpful assistant."),
                    new UserChatMessage(userInput),
                };

                var requestOptions = new ChatCompletionOptions();

                _logger.LogInformation($"Sending request to GitHub AI with question: {userInput}");

                // Make the API call
                var response = await client.CompleteChatAsync(messages, requestOptions);
                var responseText = response.Value.Content[0].Text;

                _logger.LogInformation("Successfully received response from GitHub AI.");

                return new OkObjectResult(new
                {
                    question = userInput,
                    answer = responseText,
                    model = model
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request.");
                return new StatusCodeResult(500);
            }
        }
    }
}
