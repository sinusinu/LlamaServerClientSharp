using System.Text.Json.Nodes;
using static LlamaServerClientSharp.LlamaClient;

namespace LlamaServerClientSharp;

class Program {
    static async Task Main(string[] args) {
        Uri uri = new Uri("http://localhost:8080");
        using var llamaClient = new LlamaClient(uri);

        await GetHealthAsync(llamaClient);
        await CompletionAsync(llamaClient);
        var tokens = await Tokenize(llamaClient);
        await Detokenize(llamaClient, tokens);
        await ApplyTemplateAsync(llamaClient);
        await GenerateEmbeddingAsync(llamaClient);
        await RerankingAsync(llamaClient);
        await GetSetServerPropertiesAsync(llamaClient);
        await GetMetricsAsync(llamaClient);
        await GetSetLoRAAdaptersAsync(llamaClient);
        await GetModelInfoAsync(llamaClient);
        await OAICompletionAsync(llamaClient);
        await OAIChatCompletionAsync(llamaClient);
        await OAIChatCompletionWithStructuredOutputAsync(llamaClient);
        await OAIChatCompletionWithToolCallingAsync(llamaClient);
        await OAIChatCompletionWithMultimodalityAsync(llamaClient);
        await OAICreateEmbeddingsAsync(llamaClient);
    }
    static async Task GetHealthAsync(LlamaClient llamaClient) {
        var health = await llamaClient.GetHealthAsync();
        Console.WriteLine(health);
    }

    static async Task CompletionAsync(LlamaClient llamaClient)
    {
        // Gemma 3 specific format
        var prompt = "<start_of_turn>user\nYou are a helpful assistant\n\nHello<end_of_turn>\n<start_of_turn>model\n";
        var completionRequest = new CompletionRequest.Builder()
            .SetPrompt(prompt)
            .SetNPredict(128)
            .Build();

        // immediate
        var completionResponse = await llamaClient.CompletionAsync(completionRequest);
        Console.WriteLine(completionResponse.Content);

        // streaming
        await foreach (var partialResponse in llamaClient.CompletionStreamAsync(completionRequest))
        {
            Console.Write(partialResponse.Content);
        }
        Console.WriteLine();
    }

    static async Task<int[]> Tokenize(LlamaClient llamaClient) {
        var tokenizeString = "Hello world!";
        var tokenizeRequest = new TokenizeRequest.Builder()
            .SetContent(tokenizeString)
            .Build();

        var tokens = await llamaClient.TokenizeAsync(tokenizeRequest);
        foreach (var token in tokens) Console.Write($"{token} ");
        return tokens;
    }

    static async Task Detokenize(LlamaClient llamaClient, int[] tokens) {
        var detokenizeTokens = tokens;
        var detokenizeRequest = new DetokenizeRequest.Builder()
            .SetTokens(detokenizeTokens)
            .Build();

        var detokenizedString = await llamaClient.DetokenizeAsync(detokenizeRequest);
        Console.WriteLine(detokenizedString);
    }

    static async Task ApplyTemplateAsync(LlamaClient llamaClient) {
        var applyTemplateRequest = new ApplyTemplateRequest.Builder()
            .SetMessages([
                SimpleMessage.User("Hello!")
            ])
            .Build();

        var applyTemplateResponse = await llamaClient.ApplyTemplateAsync(applyTemplateRequest);
        Console.WriteLine(applyTemplateResponse.Prompt);
    }

    static async Task GenerateEmbeddingAsync(LlamaClient llamaClient) {
        var embeddingRequest = new EmbeddingRequest.Builder()
            .SetContent("Hello world!")
            .Build();

        var embeddingResponse = await llamaClient.GetEmbeddingAsync(embeddingRequest);
        Console.WriteLine(embeddingResponse[0].Embedding[0][0]);
    }

    static async Task RerankingAsync(LlamaClient llamaClient) {
        var rerankRequest = new RerankRequest.Builder()
            .SetQuery("What is panda?")
            .SetDocuments([
                "hi",
                "it is a bear",
                "The giant panda (Ailuropoda melanoleuca), sometimes called a panda bear or simply panda, is a bear species endemic to China.",
            ])
            .Build();

        try {
            var rerankResponse = await llamaClient.RerankAsync(rerankRequest);
            foreach (var results in rerankResponse.Results) {
                Console.WriteLine(results.RelevanceScore);
            }
        } catch (LlamaServerException) {
            Console.WriteLine("This server does not support reranking (need a reranking model, set --reranking)");
        }
    }

    static async Task GetSetServerPropertiesAsync(LlamaClient llamaClient) {
        var propsGetResponse = await llamaClient.GetPropsAsync();
        Console.WriteLine(propsGetResponse.BuildInfo);

        try {
            var propsSetRequest = new PropsSetRequest() { ChatTemplate = propsGetResponse.ChatTemplate };
            var propsSetSuccess = await llamaClient.SetPropsAsync(propsSetRequest);
            Console.WriteLine($"SetPropsAsync: {propsSetSuccess}");
        } catch (LlamaServerException) {
            Console.WriteLine($"This server does not support setting props (forgot to set --props?)");
        }
    }

    static async Task GetMetricsAsync(LlamaClient llamaClient) {
        try {
            var metrics = await llamaClient.GetMetricsAsync();
            foreach (var metricsLine in metrics.Split('\n')) {
                if (metricsLine.StartsWith('#')) continue;
                Console.WriteLine(metricsLine);
            }
        } catch (LlamaServerException) {
            Console.WriteLine($"This server does not support exporting metrics (forgot to set --metrics?)");
        }
    }

    static async Task GetSetLoRAAdaptersAsync(LlamaClient llamaClient) {
        var loraAdapters = await llamaClient.GetLoRAAdaptersAsync();
        Console.WriteLine(loraAdapters.Length);

        await llamaClient.SetLoRAAdaptersAsync([]);
    }

    static async Task GetModelInfoAsync(LlamaClient llamaClient) {
        var models = await llamaClient.OAIGetModelsAsync();
        Console.WriteLine(models.Data[0].Id);
    }

    static async Task OAICompletionAsync(LlamaClient llamaClient) {
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
    }

    static async Task OAIChatCompletionAsync(LlamaClient llamaClient) {
        var oaiChatCompletionMessages = new Message.ListBuilder()
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
    }

    static async Task OAIChatCompletionWithStructuredOutputAsync(LlamaClient llamaClient) {
        var oaiChatCompletionSOMessages = new Message.ListBuilder()
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

        // For JSON mode:
        /*
        var oaiChatCompletionSORequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(oaiChatCompletionSOMessages)
            .SetResponseFormat(
                OAIResponseFormat.ResponseType.JsonObject,
                null
            )
            .Build();
        */

        // For Structured Output:
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
    }

    static async Task OAIChatCompletionWithToolCallingAsync(LlamaClient llamaClient) {
        var oaiChatCompletionToolCallMessages = new Message.ListBuilder()
            .System("Write an answer to the user's message.")
            .User("What time is it in London right now?")
            .Build();

        var oaiChatCompletionToolCallRequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(oaiChatCompletionToolCallMessages)
            .SetResponseFormat(OAIResponseFormat.ResponseType.Text, null)
            .SetMaxCompletionTokens(128)
            .SetTools([
                new OAIChatCompletionTool() {
                    Type = "function",
                    Function = new() {
                        Name = "get_time",
                        Description = "Get the current time",
                        Parameters = new() {
                            Type = "object",
                            Properties = new() {
                                { "city", new()
                                    {
                                        Type = "string",
                                        Description = "The city name to get the timezone"
                                    }
                                }
                            }
                        },
                        Required = [ "city" ]
                    }
                }
            ])
            .Build();

        // with tool calling, streaming response is not supported
        try {
            var oaiChatCompletionToolCallImmediateResponse = await llamaClient.OAIChatCompletionAsync(oaiChatCompletionToolCallRequest);
            if (oaiChatCompletionToolCallImmediateResponse.FirstChoice.FinishReason == "tool_calls") {
                var toolCalls = oaiChatCompletionToolCallImmediateResponse.FirstChoice.Message.ToolCalls!;
                foreach (var toolCall in toolCalls) {
                    if (toolCall.Function.Name == "get_time") {
                        var timeArgs = JsonNode.Parse(toolCall.Function.Arguments!)!;
                        var cityNode = timeArgs["city"];
                        if (cityNode is not null) {
                            var city = cityNode.ToString();
                            Console.WriteLine($"Model called get_time tool, asking for time in {city}");
                        } else {
                            Console.WriteLine("Model called get_time tool, but no city was given");
                        }
                    } else {
                        Console.WriteLine($"Model called {toolCall.Function.Name} tool with arguments {toolCall.Function.Arguments ?? "none"}");
                    }
                }
            } else {
                Console.WriteLine(oaiChatCompletionToolCallImmediateResponse.FirstChoice.Message.Content);
            }
        } catch (LlamaServerException e) when (e.LlamaErrorMessage.Contains("tools param requires --jinja")) {
            Console.WriteLine("This server does not support tool calling (forgot to set --jinja?)");
        }
    }

    static async Task OAIChatCompletionWithMultimodalityAsync(LlamaClient llamaClient) {
        var oaiChatCompletionMultimodalMessages = new Message.ListBuilder()
            .System("Write an answer to the user's message.")
            .Add(new CompositeMessage.Builder()
                .AddText("Describe this image.")
                .AddImageUrl("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAACv0lEQVQ4jW1SW0hTcRz+zjn/M7e5eWmTeWnreFveQsK0NyUtkageBAnCIIogsggJn9Ko9KHoQXywIiqxLAIfAqWQIWURZJGLMNG8zOsKnbc5t7PbOf8edDpd39Of7/d9v+ufwS5kp2oya6qEy+WHE8pTU9RpADBp99gsXx2W1s6pB8OT62Pheib0YFmGuXPJ3HD9THq9Y9nH84SBQa8Ew2yLJYkG7j4fb7r5aLRRlikFAC4UfFKf/7CmSqhr7bBxy84AJmbcGB53Ic8cg7AiXPFB3RGjQWXo+jT/dquD86eM1U8b8l+EhMtOP6JVBFEKFgCwuOLHyIQLMqXIMGmQbFDiQuPPs8+6ZjsYnrBkqrvUlqxXGgHA55dRd28QB8yxuHhaAABYh1bR/mYGmYIGWjXBidJE+ILyjHDyfTopK9SVhMwAwDAAx7EgZHt4s6CB6JXg80m4Up0Wok1lhboSUpQbVxS+VQXPoqk2B8rN9gFAE01QWZ4MIUW942JFuXFFJD5GodvBUopoFYfdqCg2RHDxMQodcbmDa+GkOPEHADCvjMeU3YO4GB45GVooeDYigcsTdLLW305rOMnyBIGFVYheCWoVh/lFL5rbxuERpYgE1hHnD0at5JRz747Oxmt5PfUH4R60AZRCc2g/ZNEHac2D4JobsscHda4AhmyMt+IKLO493mvkAkEapBSBY4W6CvegDbLoBzgO/jkH/PZFSOtesFE8iD4WRKve+ru3Ho/e6BtY+swBQP+v1W8FyaTAxHnNAABJBmS6cZXEPVClp4DTqLbMPV8Wuq/eH6qlFJQFAEmicmvndGvEkJs7CcerHntbZd1AlSRRGQC2oh/HxL6/Tmk6KZbbF25glAqIPmn9w/el3pbXky2Wfkff/woBALKT+CzLtaS+lWbBP3zbONl+LuFlnkmVRTgm8oab+Aet4AewMpDhZwAAAABJRU5ErkJggg==")
                .BuildUser())
            .Build();

        var oaiChatCompletionMultimodalRequest = new OAIChatCompletionRequest.Builder()
            .SetMessages(oaiChatCompletionMultimodalMessages)
            .SetResponseFormat(OAIResponseFormat.ResponseType.Text, null)
            .SetMaxCompletionTokens(128)
            .Build();

        try {
            var oaiChatCompletionMultimodalImmediateResponse = await llamaClient.OAIChatCompletionAsync(oaiChatCompletionMultimodalRequest);
            Console.WriteLine(oaiChatCompletionMultimodalImmediateResponse.FirstChoice.Message.Content);
        } catch (LlamaServerException e) when (e.LlamaErrorMessage.Contains("image input is not supported")) {
            Console.WriteLine("This server does not support multimodal input (forgot to set --mmproj?)");
        }
    }

    static async Task OAICreateEmbeddingsAsync(LlamaClient llamaClient) {
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
    }

    record AnswerSchema(string answer, bool positive);
}