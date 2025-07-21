// nuget install Microsoft.SemanticKernel

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

// Protect your secrets foo!
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// The Kernel is basically a container for all the services you need to use Semantic Kernel.
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: configuration["azureAiStudioDeploymentName"] ?? throw new InvalidOperationException("azureAiStudioDeploymentName is not set."),
        endpoint: configuration["azureAiStudioEndpoint"] ?? throw new InvalidOperationException("azureAiStudioEndpoint is not set."),
        apiKey: configuration["azureAiStudioApiKey"] ?? throw new InvalidOperationException("azureAiStudioApiKey is not set."))
    .Build();

// The chat completion service is the main entry point for chat-based interactions.
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Optional execution settings. There's a lot to explore in here.
var executionSettings = new AzureOpenAIPromptExecutionSettings
{
    Temperature = 1.0f,
    TopP = 1.0f
};

// A system prompt is useful to give the AI a task, or a persona.
var systemPrompt = "You are an unhelpful assistant. Give sarcastic answers to all the user's questions.";

// The chat history object represents the conversation state.
var chatHistory = new ChatHistory(systemPrompt);

// Get the user message to send to the AI
Console.Write("User/> ");
var userInput = Console.ReadLine();
chatHistory.AddUserMessage(userInput ?? string.Empty);

// Get the AI's response
var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);
Console.WriteLine($"Assistant/> {response?.Content ?? "NO RESPONSE"}");