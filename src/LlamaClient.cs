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

    // TODO: embedding

    // TODO: reranking

    // TODO: infill

    // TODO: props

    // TODO: embeddings

    // TODO: slots & friends

    // TODO: metrics

    // TODO: lora-adapters

    public async Task<OAIModelsResponse> OAIModelsAsync() {
        var response = await client.GetAsync(endpoint + "v1/models");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<OAIModelsResponse>(responseContent)!;
        return responseJson;
    }

    // TODO: v1/completions

    // TODO: v1/embeddings

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

    public void Dispose() {
        client.Dispose();
    }
}