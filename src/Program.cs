using System.Text.Json;
using System.Text.Json.Schema;

using static LlamaServerClientSharp.LlamaClient;

namespace LlamaServerClientSharp;

class Program {
    static async Task Main(string[] args) {
        Uri uri = new Uri("http://localhost:8080");
        using var llamaClient = new LlamaClient(uri);
        
        var health = await llamaClient.GetHealthAsync();
        Console.WriteLine(health);

#region Completion
        // Gemma 3 specific format
        var prompt = "<start_of_turn>user\nYou are a helpful assistant\n\nHello<end_of_turn>\n<start_of_turn>model\n";
        var completionRequest = new CompletionRequest.Builder()
            .SetPrompt(prompt)
            .Build();

        // immediate
        var completionResponse = await llamaClient.CompletionAsync(completionRequest);
        Console.WriteLine(completionResponse.Content);

        // streaming
        await foreach (var partialResponse in llamaClient.CompletionStreamAsync(completionRequest)) {
            Console.Write(partialResponse.Content);
        }
        Console.WriteLine();
#endregion Completion

#region Tokenize
        var tokenizeString = "Hello world!";
        var tokenizeRequest = new TokenizeRequest.Builder()
            .SetContent(tokenizeString)
            .Build();

        var tokens = await llamaClient.TokenizeAsync(tokenizeRequest);
        Console.Write('[');
        foreach (var token in tokens) Console.Write($"{token},");
        Console.WriteLine(']');
#endregion Tokenize

#region Detokenize
        var detokenizeTokens = tokens;
        var detokenizeRequest = new DetokenizeRequest.Builder()
            .SetTokens(detokenizeTokens)
            .Build();

        var detokenizedString = await llamaClient.DetokenizeAsync(detokenizeRequest);
        Console.WriteLine(detokenizedString);
#endregion Detokenize

#region Apply Chat Template
        var applyTemplateRequest = new ApplyTemplateRequest.Builder()
            .SetMessages([
                Message.User("Hello!")
            ])
            .Build();

        var applyTemplateResponse = await llamaClient.ApplyTemplateAsync(applyTemplateRequest);
        Console.WriteLine(applyTemplateResponse.Prompt);
#endregion Apply Chat Template

#region Generate Embedding
        var embeddingRequest = new EmbeddingRequest.Builder()
            .SetContent("Hello world!")
            .Build();

        var embeddingResponse = await llamaClient.GetEmbeddingAsync(embeddingRequest);
        Console.WriteLine(embeddingResponse[0].Embedding[0][0]);
#endregion Generate Embedding

#region Get/Set Server Global Properties
        var propsGetResponse = await llamaClient.GetPropsAsync();
        Console.WriteLine(propsGetResponse.BuildInfo);

        try {
            var propsSetRequest = new PropsSetRequest() { ChatTemplate = propsGetResponse.ChatTemplate };
            var propsSetSuccess = await llamaClient.SetPropsAsync(propsSetRequest);
            Console.WriteLine($"SetPropsAsync: {propsSetSuccess}");
        } catch (LlamaServerException) {
            Console.WriteLine($"This server does not support setting props (forgot to set --props?)");
        }
#endregion Get/Set Server Global Properties

#region LoRA Adapters
        var loraAdapters = await llamaClient.GetLoRAAdaptersAsync();
        Console.WriteLine(loraAdapters.Length);

        await llamaClient.SetLoRAAdaptersAsync([]);
#endregion LoRA Adapters

#region OpenAI-compatible Model Info
        var models = await llamaClient.OAIGetModelsAsync();
        Console.WriteLine(models.Data[0].Id);
#endregion OpenAI-compatible Model Info

#region OpenAI-compatible Completion
        var oaiCompletionMessage = "Hello, world! ";

        var oaiCompletionRequest = new OAICompletionRequest.Builder()
            .SetPrompt(oaiCompletionMessage)
            .SetMaxTokens(128)
            .Build();

        // immediate
        var oaiCompletionImmediateResponse = await llamaClient.OAICompletionAsync(oaiCompletionRequest);
        Console.WriteLine(oaiCompletionImmediateResponse.FirstChoice.Text);

        // streaming
        await foreach (var oaiCompletionPartialResponse in llamaClient.OAICompletionStreamAsync(oaiCompletionRequest)) {
            Console.Write(oaiCompletionPartialResponse.FirstChoice.Text);
        }
        Console.WriteLine();
#endregion OpenAI-compatible Completion

#region OpenAI-compatible Chat Completion
        var oaiChatCompletionMessages = new Message.Builder()
            .System("Write an answer to the user's message.")
            .User("Nice to meet you!")
            .Build();

        var oaiChatCompletionRequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(oaiChatCompletionMessages)
            .SetResponseFormat(OAIResponseFormat.ResponseType.Text, null)
            .SetMaxCompletionTokens(128)
            .Build();

        // immediate
        var oaiChatCompletionImmediateResponse = await llamaClient.OAIChatCompletionAsync(oaiChatCompletionRequest);
        Console.WriteLine(oaiChatCompletionImmediateResponse.FirstChoice.Message.Content);

        // streaming
        await foreach (var oaiChatCompletionPartialResponse in llamaClient.OAIChatCompletionStreamAsync(oaiChatCompletionRequest)) {
            if (oaiChatCompletionPartialResponse.FirstChoice.Delta is not null) Console.Write(oaiChatCompletionPartialResponse.FirstChoice.Delta.Content);
        }
        Console.WriteLine();
#endregion OpenAI-compatible Chat Completion

#region OpenAI-compatible Chat Completion (Structured Output)
        var oaiChatCompletionSOMessages = new Message.Builder()
            .System(
@"Write an answer to the user's message, and evaluate if user's message was positive. Output must follow the JSON schema given below.

# JSON Schema
```json
{ ""answer"": string, ""positive"": boolean }
```
- answer: Answer to the user's message
- positive: true if user's message was positive, false if not"
            )
            .User("Nice to meet you!")
            .Build();

        var oaiChatCompletionSORequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(oaiChatCompletionSOMessages)
            .SetResponseFormat(
                OAIResponseFormat.ResponseType.JsonSchema,
                OAIResponseFormat.SchemaOf<AnswerSchema>()
            )
            .Build();

        // immediate
        var oaiChatCompletionSOImmediateResponse = await llamaClient.OAIChatCompletionAsync(oaiChatCompletionSORequest);
        Console.WriteLine(oaiChatCompletionSOImmediateResponse.FirstChoice.Message.Content);

        // streaming
        await foreach (var oaiChatCompletionSOPartialResponse in llamaClient.OAIChatCompletionStreamAsync(oaiChatCompletionSORequest)) {
            if (oaiChatCompletionSOPartialResponse.FirstChoice.Delta is not null) Console.Write(oaiChatCompletionSOPartialResponse.FirstChoice.Delta.Content);
        }
        Console.WriteLine();
#endregion OpenAI-compatible Chat Completion (Structured Output)

#region OpenAI-compatible Create Embeddings
        var oaiEmbeddingsRequest = new OAIEmbeddingsRequest.Builder()
            .SetInput("Hello world!")
            .Build();

        try {
            var oaiEmbeddingsResponse = await llamaClient.OAIEmbeddingsAsFloatAsync(oaiEmbeddingsRequest);
            Console.WriteLine(oaiEmbeddingsResponse.Data[0].Embedding[0]);

            var oaiEmbeddingsBResponse = await llamaClient.OAIEmbeddingsAsBase64Async(oaiEmbeddingsRequest);
            Console.WriteLine(oaiEmbeddingsBResponse.Data[0].Embedding.Substring(0, 5));
        } catch (LlamaServerException) {
            Console.WriteLine("This server does not support OAI-compatible embeddings (forgot to set --polling?)");
        }
#endregion OpenAI-compatible Create Embeddings
    }

    record AnswerSchema(string answer, bool positive);
}