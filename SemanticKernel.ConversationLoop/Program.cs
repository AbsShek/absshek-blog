// nuget install Microsoft.SemanticKernel

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Newtonsoft.Json;
using SemanticKernel.ConversationLoop;

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

var chat = kernel.GetRequiredService<IChatCompletionService>();

var systemPrompt =
@"You are an unhelpful assistant. Give sarcastic answers to all the user's questions.
If the user has indicated that they want to leave, or you're bored, end the conversation.";
var chatHistory = new ChatHistory(systemPrompt);

var executionSettings = new AzureOpenAIPromptExecutionSettings
{
    ResponseFormat = typeof(ChatCompletionResponse),
};

while (true) // You can set conversation length limits here for safety. We'll live on the edge for this one.
{
    Console.Write("User/> ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    chatHistory.AddUserMessage(userInput);

    var response = await chat.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings);

    if (!string.IsNullOrEmpty(response?.Content))
    {
        var structuredResponse = JsonConvert.DeserializeObject<ChatCompletionResponse>(response.Content);

        if (structuredResponse != null)
        {
            var reply = structuredResponse.Reply;

            Console.WriteLine($"Assistant/> {reply}");

            // We add the assistant's reply to the chat history.
            // This helps us simulate a natural on-going conversation.
            chatHistory.AddAssistantMessage(reply);

            // The agent will tell us when it's time to end.
            if (structuredResponse.EndConversation)
            {
                // Exit
                break;
            }
            else
            {
                // Keep going
                continue;
            }
        }
    }// END: if (structuredResponse != null)

    // If we got here, it means the response was not in the expected format or was empty.
    Console.WriteLine($"Assistant/> NO RESPONSE");
} // END: while (true)