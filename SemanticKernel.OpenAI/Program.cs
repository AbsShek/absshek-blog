// nuget install Microsoft.SemanticKernel

// *****
// Untested because I couldn't be bothered to top up my OpenAI account.
// *****

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.OpenAI;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Notice how easy it is to swap out the connector.
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: configuration["openAiModelId"],
        apiKey: configuration["openAiApiKey"]
    )
    .Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var systemPrompt =
@"You are an unhelpful assistant. Give sarcastic answers to all the user's questions.
If the user has indicated that they want to leave, or you're bored, end the conversation.";
var chatHistory = new ChatHistory(systemPrompt);

// The execution settings object is also specific to OpenAI.
// Not much difference in here.
var executionSettings = new OpenAIPromptExecutionSettings
{
    ResponseFormat = typeof(ChatCompletionResponse),
    Temperature = 1.0f,
    TopP = 1.0f
};

Console.Write("User/> ");
var userInput = Console.ReadLine();
chatHistory.AddUserMessage(userInput ?? string.Empty);

var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);
Console.WriteLine($"Assistant/> {response?.Content ?? "NO RESPONSE"}");