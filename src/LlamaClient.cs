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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<CompletionResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
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
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);
            var tokensEnumerator = responseJson.RootElement.GetProperty("tokens").EnumerateArray();
            List<int> tokensList = new();
            foreach (var t in tokensEnumerator) {
                tokensList.Add(t.GetInt32());
            }
            return tokensList.ToArray();
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);
            var detokenized = responseJson.RootElement.GetProperty("content").GetString()!;
            return detokenized;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<ApplyTemplateResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<EmbeddingResponse[]>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<LoRAAdapterResponse[]>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (!response.IsSuccessStatusCode) {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
    }

    /// <summary>GET /v1/models</summary>
    public async Task<OAIModelsResponse> OAIGetModelsAsync() {
        var response = await client.GetAsync(endpoint + "v1/models");
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIModelsResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
    }

    /// <summary>POST /v1/completions, "stream": false</summary>
    /// <param name="request">Use <c>OAICompletionRequest.Builder</c>.</param>
    public async Task<OAICompletionResponse> OAICompletionAsync(OAICompletionRequest request) {
        request.Stream = false;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        var response = await client.PostAsync(endpoint + "v1/completions", postContent);
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAICompletionResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
    }

    /// <summary>POST /v1/completions, "stream": true</summary>
    /// <param name="request">Use <c>OAICompletionRequest.Builder</c>.</param>
    public async IAsyncEnumerable<OAICompletionStreamResponse> OAICompletionStreamAsync(OAICompletionRequest request) {
        request.Stream = true;
        using StringContent postContent = new(
            JsonSerializer.Serialize(request, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), Encoding.UTF8, "application/json"
        );
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint + "v1/completions") {
            Content = postContent
        };
        var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
        if (response.IsSuccessStatusCode) {
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
                    var partialResult = JsonSerializer.Deserialize<OAICompletionStreamResponse>(line)!;
                    yield return partialResult;
                }
            }
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
    }

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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIChatCompletionResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
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
        if (response.IsSuccessStatusCode) {
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
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIEmbeddingsFloatResponse>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
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
        if (response.IsSuccessStatusCode) {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<OAIEmbeddingsBase64Response>(responseContent)!;
            return responseJson;
        } else {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<Error>(errorContent)!;
            throw new LlamaServerException(errorJson.InnerError.Code, errorJson.InnerError.Message, errorJson.InnerError.Type);
        }
    }

    public class LlamaServerException : Exception {
        public int LlamaErrorCode { get; private set; }
        public string LlamaErrorMessage { get; private set; }
        public string LlamaErrorType { get; private set; }
        public LlamaServerException(int code, string message, string type) : base($"{type}: {message}") {
            LlamaErrorCode = code;
            LlamaErrorMessage = message;
            LlamaErrorType = type;
        }
    }

    public void Dispose() {
        client.Dispose();
    }
}