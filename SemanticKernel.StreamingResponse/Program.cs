// nuget install Microsoft.SemanticKernel

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: configuration["azureAiStudioDeploymentName"] ?? throw new InvalidOperationException("azureAiStudioDeploymentName is not set."),
        endpoint: configuration["azureAiStudioEndpoint"] ?? throw new InvalidOperationException("azureAiStudioEndpoint is not set."),
        apiKey: configuration["azureAiStudioApiKey"] ?? throw new InvalidOperationException("azureAiStudioApiKey is not set."))
    .Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var systemPrompt = "You are an unhelpful assistant. Give sarcastic answers to all the user's questions.";

var chatHistory = new ChatHistory(systemPrompt);

Console.Write("User/> ");
var userInput = Console.ReadLine();
chatHistory.AddUserMessage(userInput ?? string.Empty);

Console.Write($"Assistant/> ");
Console.Out.Flush();

// Get the AI's response as a stream like ChatGPT does.
await foreach (var responsePart in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
{
    Console.Write(responsePart.Content);

    Console.Out.Flush(); // For dramatic effect, we want to see the output as it comes in.
    Thread.Sleep(200); // Because life goes by too fast.
}