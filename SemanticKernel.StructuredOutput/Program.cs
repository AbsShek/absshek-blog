// nuget install Microsoft.SemanticKernel

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Newtonsoft.Json;
using SemanticKernel.StructuredOutput;

// Protect your secrets foo!
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: configuration["azureAiStudioDeploymentName"],
        endpoint: configuration["azureAiStudioEndpoint"],
        apiKey: configuration["azureAiStudioApiKey"])
    .Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Note ths extra bit at the end of the system prompt. We're not directly mentioning the structured output format,
// but we are hinting that the assistant should end the conversation if it wants to.
var systemPrompt =
@"You are an unhelpful assistant. Give sarcastic answers to all the user's questions.
If the user has indicated that they want to leave, or you're bored, end the conversation.";
var chatHistory = new ChatHistory(systemPrompt);

var executionSettings = new AzureOpenAIPromptExecutionSettings
{
    // Yes, this is literally all you need to do.
    // Note how I don't need to mention this in the system prompt.
    ResponseFormat = typeof(ChatCompletionResponse)
};

Console.Write("User/> ");
var userInput = Console.ReadLine();
chatHistory.AddUserMessage(userInput ?? string.Empty);

var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);

// We can be sure the LLM has replied in the requested format.
var structuredResponse = JsonConvert.DeserializeObject<ChatCompletionResponse>(response.Content!); // Let's pretend it's never null for now.

// Let's just output the raw response to see what it's like.
Console.WriteLine($"Assistant/> {structuredResponse!.Reply ?? "NO RESPONSE"}");

// We can check if the assistant wants to end the conversation.
Console.WriteLine();
if (structuredResponse.EndConversation)
{
    Console.WriteLine("Assistant wants to end the conversation.");
}
else
{
    Console.WriteLine("Assistant wants to keep talking.");
}