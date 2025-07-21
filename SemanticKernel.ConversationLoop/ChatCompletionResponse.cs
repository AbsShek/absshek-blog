namespace SemanticKernel.ConversationLoop
{
    internal sealed class ChatCompletionResponse
    {
        public required string Reply { get; set; }
        public required bool EndConversation { get; set; }
    }
}
