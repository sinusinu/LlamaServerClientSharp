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

    /// <summary>GET /health</summary>
    public async Task<Health> GetHealthAsync() {
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
    
    /// <summary>POST /completion, "stream": false</summary>
    /// <param name="request">Use <c>CompletionRequest.Builder</c>.</param>
    public async Task<CompletionResponse> CompletionAsync(CompletionRequest request) {
        request.Stream = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "completions", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<CompletionResponse>(responseContent)!;
        return responseJson;
    }
    
    /// <summary>POST /completion, "stream": true</summary>
    /// <param name="request">Use <c>CompletionRequest.Builder</c>.</param>
    public async IAsyncEnumerable<CompletionResponse> CompletionStreamAsync(CompletionRequest request) {
        request.Stream = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>POST /tokenize, "with_pieces": false</summary>
    /// <param name="request">Use <c>TokenizeRequest.Builder</c>.</param>
    public async Task<int[]> TokenizeAsync(TokenizeRequest request) {
        request.WithPieces = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>POST /tokenize, "with_pieces": true</summary>
    /// <param name="request">Use <c>TokenizeRequest.Builder</c>.</param>
    public async Task<TokenizeTokensWithPieces[]> TokenizeWithPiecesAsync(TokenizeRequest request) {
        request.WithPieces = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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
    
    /// <summary>POST /detokenize</summary>
    /// <param name="request">Use <c>DetokenizeRequest.Builder</c>.</param>
    public async Task<string> DetokenizeAsync(DetokenizeRequest request) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>POST /apply-template</summary>
    /// <param name="request">Use <c>ApplyTemplateRequest.Builder</c>.</param>
    public async Task<ApplyTemplateResponse> ApplyTemplateAsync(ApplyTemplateRequest request) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "apply-template", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<ApplyTemplateResponse>(responseContent)!;
        return responseJson;
    }

    /// <summary>POST /embedding</summary>
    /// <param name="request">Use <c>EmbeddingRequest.Builder</c>.</param>
    public async Task<EmbeddingResponse[]> GetEmbeddingAsync(EmbeddingRequest request) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>GET /lora-adapters</summary>
    public async Task<LoRAAdapterResponse[]> GetLoRAAdaptersAsync() {
        var response = await client.GetAsync(endpoint + "lora-adapters");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<LoRAAdapterResponse[]>(responseContent)!;
        return responseJson;
    }

    /// <summary>POST /lora-adapters</summary>
    /// <param name="request">An array of <c>LoRAAdapterRequest</c>.</param>
    public async Task SetLoRAAdaptersAsync(LoRAAdapterRequest[] request) {
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "lora-adapters", postContent);
        response.EnsureSuccessStatusCode(); // should crash out here if failed
    }

    /// <summary>GET /v1/models</summary>
    public async Task<OAIModelsResponse> OAIGetModelsAsync() {
        var response = await client.GetAsync(endpoint + "v1/models");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<OAIModelsResponse>(responseContent)!;
        return responseJson;
    }

    // TODO: v1/completions

    /// <summary>POST /v1/chat/completions, "stream": false</summary>
    /// <param name="request">Use <c>OAIChatCompletionRequest.Builder</c>.</param>
    public async Task<OAIChatCompletionResponse> OAIChatCompletionAsync(OAIChatCompletionRequest request) {
        request.Stream = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "v1/chat/completions", postContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<OAIChatCompletionResponse>(responseContent)!;
        return responseJson;
    }

    /// <summary>POST /v1/chat/completions, "stream": true</summary>
    /// <param name="request">Use <c>OAIChatCompletionRequest.Builder</c>.</param>
    public async IAsyncEnumerable<OAIChatCompletionStreamResponse> OAIChatCompletionStreamAsync(OAIChatCompletionRequest request) {
        request.Stream = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>POST /v1/embeddings, "encoding_format": "float"</summary>
    /// <param name="request">Use <c>OAIEmbeddingsRequest.Builder</c>.</param>
    public async Task<OAIEmbeddingsFloatResponse> OAIEmbeddingsAsFloatAsync(OAIEmbeddingsRequest request) {
        request.EncodingFormat = OAIEmbeddingsRequest.OAIEmbeddingsRequestEncodingFormat.Float;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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

    /// <summary>POST /v1/embeddings, "encoding_format": "base64"</summary>
    /// <param name="request">Use <c>OAIEmbeddingsRequest.Builder</c>.</param>
    public async Task<OAIEmbeddingsBase64Response> OAIEmbeddingsAsBase64Async(OAIEmbeddingsRequest request) {
        request.EncodingFormat = OAIEmbeddingsRequest.OAIEmbeddingsRequestEncodingFormat.Base64;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
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