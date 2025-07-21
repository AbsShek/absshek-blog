// nuget install Microsoft.KernelMemory

using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var memory = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(
        new AzureOpenAIConfig
        {
            Deployment = configuration["textGenerationDeploymentName"] ?? throw new InvalidOperationException("textGenerationDeploymentName is not set."),
            Endpoint = configuration["textGenerationEndpoint"] ?? throw new InvalidOperationException("textGenerationEndpoint is not set."),
            APIKey = configuration["textGenerationApiKey"] ?? throw new InvalidOperationException("textGenerationApiKey is not set.")
        })
    .WithAzureOpenAITextEmbeddingGeneration(
        new AzureOpenAIConfig
        {
            Deployment = configuration["embeddingDeploymentName"] ?? throw new InvalidOperationException("embeddingDeploymentName is not set."),
            Endpoint = configuration["embeddingEndpoint"] ?? throw new InvalidOperationException("embeddingEndpoint is not set."),
            APIKey = configuration["embeddingApiKey"] ?? throw new InvalidOperationException("embeddingApiKey is not set.")
        })
    .Build<MemoryServerless>();

Console.Write("Enter the file path (PDF, TXT, DOCX)/> ");
string filePath = ReadInputPersistently()
                    .Trim(' ', '"'); // Remove spaces and quotes, if any
Console.WriteLine();

Console.WriteLine($"Importing {filePath}. This can take a while for big files.");
var documentId = await memory.ImportDocumentAsync(filePath);
Console.WriteLine($"File was imported with document id {documentId}.");
Console.WriteLine();

Console.Write("Ask a question/> ");
string question = ReadInputPersistently();
var answer = await memory.AskAsync(question);
Console.WriteLine();

Console.WriteLine($"Answer/> {answer}");

// Helper function to avoid bad input errors.
static string ReadInputPersistently()
{
    string? input;
    do
    {
        input = Console.ReadLine();
    } while (string.IsNullOrEmpty(input));

    return input!;
}