﻿using System.Text.Json;
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
        // Gemma 3 specific tokens
        var detokenizeTokens = new int[] { 9259, 1902, 236888 };
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

#region LoRA Adapters
        var loraAdapters = await llamaClient.GetLoRAAdaptersAsync();
        Console.WriteLine(loraAdapters.Length);

        await llamaClient.SetLoRAAdaptersAsync([]);
#endregion LoRA Adapters

#region OpenAI-compatible Model Info
        var models = await llamaClient.OAIGetModelsAsync();
        Console.WriteLine(models.Data[0].Id);
#endregion OpenAI-compatible Model Info

#region OpenAI-compatible Chat Completion
        var chatCompletionMessages = new Message[] {
            Message.System("Write an answer to the user's message."),
            Message.User("Nice to meet you!"),
        };

        var chatCompletionRequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(chatCompletionMessages)
            .SetResponseFormat(OAIResponseFormat.ResponseType.Text, null)
            .Build();

        // immediate
        var chatCompletionImmediateResponse = await llamaClient.OAIChatCompletionAsync(chatCompletionRequest);
        Console.WriteLine(chatCompletionImmediateResponse.FirstChoice.Message.Content);

        // streaming
        await foreach (var chatCompletionPartialResponse in llamaClient.OAIChatCompletionStreamAsync(chatCompletionRequest)) {
            if (chatCompletionPartialResponse.FirstChoice.Delta is not null) Console.Write(chatCompletionPartialResponse.FirstChoice.Delta.Content);
        }
        Console.WriteLine();
#endregion OpenAI-compatible Chat Completion

#region OpenAI-compatible Chat Completion (Structured Output)
        var chatCompletionSOMessages = new Message[] {
            Message.System("Write an answer to the user's message, and evaluate if user's message was positive. Output must follow the JSON schema given below.\n\n# JSON Schema\n```json\n{ \"answer\": string, \"positive\": boolean }\n```\n- answer: Answer to the user's message\n- positive: true if user's message was positive, false if not"),
            Message.User("Nice to meet you!"),
        };

        var chatCompletionSORequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(chatCompletionSOMessages)
            .SetResponseFormat(
                OAIResponseFormat.ResponseType.JsonSchema,
                OAIResponseFormat.SchemaOf<AnswerSchema>()
            )
            .Build();

        // immediate
        var chatCompletionSOImmediateResponse = await llamaClient.OAIChatCompletionAsync(chatCompletionSORequest);
        Console.WriteLine(chatCompletionSOImmediateResponse.FirstChoice.Message.Content);

        // streaming
        await foreach (var chatCompletionSOPartialResponse in llamaClient.OAIChatCompletionStreamAsync(chatCompletionSORequest)) {
            if (chatCompletionSOPartialResponse.FirstChoice.Delta is not null) Console.Write(chatCompletionSOPartialResponse.FirstChoice.Delta.Content);
        }
        Console.WriteLine();
#endregion OpenAI-compatible Chat Completion (Structured Output)

#region OpenAI-compatible Create Embeddings
        var oaiEmbeddingsRequest = new OAIEmbeddingsRequest.Builder()
            .SetInput("Hello world!")
            .Build();

        var oaiEmbeddingsResponse = await llamaClient.OAIEmbeddingsAsFloatAsync(oaiEmbeddingsRequest);
        Console.WriteLine(oaiEmbeddingsResponse.Data[0].Embedding[0]);

        var oaiEmbeddingsBResponse = await llamaClient.OAIEmbeddingsAsBase64Async(oaiEmbeddingsRequest);
        Console.WriteLine(oaiEmbeddingsBResponse.Data[0].Embedding.Substring(0, 5));
#endregion OpenAI-compatible Create Embeddings
    }

    record AnswerSchema(string answer, bool positive);
}