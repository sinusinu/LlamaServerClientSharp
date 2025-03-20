using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LlamaServerClientSharp;

public partial class LlamaClient : IDisposable {
    private HttpClient client;
    private string endpoint;

    public LlamaClient(Uri endpoint) {
        client = new HttpClient();
        this.endpoint = endpoint.ToString();
    }

    public async Task<Health> HealthAsync() {
        var response = await client.GetAsync(endpoint + "health");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var statusString = responseJson.RootElement.GetProperty("status").GetString();
        switch (statusString) {
            case "loading model":
                return Health.LoadingModel;
            case "error":
                return Health.Error;
            case "ok":
                return Health.OK;
            default:
                return Health.Unknown;
        }
    }
    
    public async Task<CompletionResponse> CompletionAsync(CompletionContent content) {
        content.Stream = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "completions", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<CompletionResponse>(responseContent)!;
        return responseJson;
    }
    
    public async IAsyncEnumerable<CompletionResponse> CompletionStreamAsync(CompletionContent content) {
        content.Stream = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint + "completions") {
            Content = postContent
        };
        var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync();
        using (var reader = new StreamReader(responseStream)) {
            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();
                if (line is null) continue;
                line = line.Trim();
                if (line.Length == 0) continue;
                if (!line.StartsWith("data: ")) continue;
                line = line.Substring(6);
                if (line == "[DONE]") break;
                var partialResult = JsonSerializer.Deserialize<CompletionResponse>(line)!;
                yield return partialResult;
            }
        }
    }

    public async Task<int[]> TokenizeAsync(TokenizeContent content) {
        content.WithPieces = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "tokenize", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var tokensEnumerator = responseJson.RootElement.GetProperty("tokens").EnumerateArray();
        List<int> tokensList = new();
        foreach (var t in tokensEnumerator) {
            tokensList.Add(t.GetInt32());
        }
        return tokensList.ToArray();
    }

    public async Task<TokenizeTokensWithPieces[]> TokenizeWithPiecesAsync(TokenizeContent content) {
        content.WithPieces = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "tokenize", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var tokensEnumerator = responseJson.RootElement.GetProperty("tokens").EnumerateArray();
        List<TokenizeTokensWithPieces> tokensList = new();
        foreach (var t in tokensEnumerator) {
            tokensList.Add(t.Deserialize<TokenizeTokensWithPieces>()!);
        }
        return tokensList.ToArray();
    }
    
    public async Task<string> DetokenizeAsync(DetokenizeContent content) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "detokenize", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var detokenized = responseJson.RootElement.GetProperty("content").GetString()!;
        return detokenized;
    }

    public async Task<ApplyTemplateResponse> ApplyTemplateAsync(ApplyTemplateContent content) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "apply-template", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<ApplyTemplateResponse>(responseContent)!;
        return responseJson;
    }

    public async Task<EmbeddingResponse[]> EmbeddingAsync(EmbeddingContent content) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "embedding", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<EmbeddingResponse[]>(responseContent)!;
        return responseJson;
    }

    // TODO: reranking

    // TODO: infill

    // TODO: props

    // TODO: embeddings

    // TODO: slots & friends

    // TODO: metrics

    public async Task<LoRAAdapterResponse[]> GetLoRAAdaptersAsync() {
        var response = await client.GetAsync(endpoint + "lora-adapters");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<LoRAAdapterResponse[]>(responseContent)!;
        return responseJson;
    }

    public async Task SetLoRAAdaptersAsync(LoRAAdapterContent[] content) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "lora-adapters", postContent);
        response.EnsureSuccessStatusCode(); // should crash out here if failed
    }

    public async Task<OAIModelsResponse> OAIModelsAsync() {
        var response = await client.GetAsync(endpoint + "v1/models");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<OAIModelsResponse>(responseContent)!;
        return responseJson;
    }

    // TODO: v1/completions

    public async Task<OAIChatCompletionResponse> OAIChatCompletionAsync(OAIChatCompletionContent content) {
        content.Stream = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "v1/chat/completions", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<OAIChatCompletionResponse>(responseContent)!;
        return responseJson;
    }

    public async IAsyncEnumerable<OAIChatCompletionStreamResponse> OAIChatCompletionStreamAsync(OAIChatCompletionContent content) {
        content.Stream = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint + "v1/chat/completions") {
            Content = postContent
        };
        var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync();
        using (var reader = new StreamReader(responseStream)) {
            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();
                if (line is null) continue;
                line = line.Trim();
                if (line.Length == 0) continue;
                if (!line.StartsWith("data: ")) continue;
                line = line.Substring(6);
                if (line == "[DONE]") break;
                var partialResult = JsonSerializer.Deserialize<OAIChatCompletionStreamResponse>(line)!;
                yield return partialResult;
            }
        }
    }

    public async Task<OAIEmbeddingsFloatResponse> OAIEmbeddingsAsFloatAsync(OAIEmbeddingsContent content) {
        content.EncodingFormat = OAIEmbeddingsContent.OAIEmbeddingsContentEncodingFormat.Float;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "v1/embeddings", postContent);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent.Contains("Pooling type 'none'")) throw new InvalidOperationException("Pooling type 'none' is not OAI compatible. Please use a different pooling type.");
            else response.EnsureSuccessStatusCode();
            return null!;   // shouldn't be reached
        } else {
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIEmbeddingsFloatResponse>(responseContent)!;
            return responseJson;
        }
    }

    public async Task<OAIEmbeddingsBase64Response> OAIEmbeddingsAsBase64Async(OAIEmbeddingsContent content) {
        content.EncodingFormat = OAIEmbeddingsContent.OAIEmbeddingsContentEncodingFormat.Base64;
        using StringContent postContent = new(
            JsonSerializer.Serialize(content, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "v1/embeddings", postContent);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent.Contains("Pooling type 'none'")) throw new InvalidOperationException("Pooling type 'none' is not OAI compatible. Please use a different pooling type.");
            else response.EnsureSuccessStatusCode();
            return null!;   // shouldn't be reached
        } else {
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIEmbeddingsBase64Response>(responseContent)!;
            return responseJson;
        }
    }

    public void Dispose() {
        client.Dispose();
    }
}