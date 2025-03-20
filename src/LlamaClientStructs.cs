using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace LlamaServerClientSharp;

public partial class LlamaClient {
#region Error
    public class Error {
        [JsonPropertyName("error")] public required ErrorInnerError InnerError { get; set; }
    }

    public class ErrorInnerError {
        [JsonPropertyName("code")] public required int Code { get; set; }
        [JsonPropertyName("message")] public required string Message { get; set; }
        [JsonPropertyName("type")] public required string Type { get; set; }
    }
#endregion Error

#region Commonly used classes
    public class Timings {
        [JsonPropertyName("prompt_n")] public double PromptN { get; set; }
        [JsonPropertyName("prompt_ms")] public double PromptMs { get; set; }
        [JsonPropertyName("prompt_per_token_ms")] public double PromptPerTokenMs { get; set; }
        [JsonPropertyName("prompt_per_second")] public double PromptPerSecond { get; set; }
        [JsonPropertyName("predicted_n")] public double PredictedN { get; set; }
        [JsonPropertyName("predicted_ms")] public double PredictedMs { get; set; }
        [JsonPropertyName("predicted_per_token_ms")] public double PredictedPerTokenMs { get; set; }
        [JsonPropertyName("predicted_per_second")] public double PredictedPerSecond { get; set; }
    }

    public class ImageData {
        [JsonPropertyName("data")] public required string Data { get; set; }
        [JsonPropertyName("id")] public required int Id { get; set; }
    }

    public class Message {
        public string role { get; set; }
        public string content { get; set; }

        public Message(string role, string content) {
            this.role = role;
            this.content = content;
        }

        public static Message System(string content) => new Message("system", content);
        public static Message User(string content) => new Message("user", content);
        public static Message Assistant(string content) => new Message("assistant", content);
    }
#endregion Commonly used classes

#region Health
    public enum Health {
        Unknown,
        LoadingModel,
        Error,
        OK
    }
#endregion Health

#region Completion
    public class CompletionRequest {
        [JsonPropertyName("prompt")] public required string Prompt { get; set; }
        [JsonPropertyName("temperature")] public float? Temperature { get; set; }
        [JsonPropertyName("dynatemp_range")] public float? DynatempRange { get; set; }
        [JsonPropertyName("dynatemp_exponent")] public float? DynatempExponent { get; set; }
        [JsonPropertyName("top_k")] public int? TopK { get; set; }
        [JsonPropertyName("top_p")] public float? TopP { get; set; }
        [JsonPropertyName("min_p")] public float? MinP { get; set; }
        [JsonPropertyName("n_predict")] public int? NPredict { get; set; }
        [JsonPropertyName("n_indent")] public int? NIndent { get; set; }
        [JsonPropertyName("n_keep")] public int? NKeep { get; set; }
        [JsonPropertyName("stream")] public bool? Stream { get; set; }
        [JsonPropertyName("stop")] public string[]? Stop { get; set; }
        [JsonPropertyName("typical_p")] public float? TypicalP { get; set; }
        [JsonPropertyName("repeat_penalty")] public float? RepeatPenalty { get; set; }
        [JsonPropertyName("repeat_last_n")] public int? RepeatLastN { get; set; }
        [JsonPropertyName("presence_penalty")] public float? PresencePenalty { get; set; }
        [JsonPropertyName("frequency_penalty")] public float? FrequencyPenalty { get; set; }
        [JsonPropertyName("dry_multiplier")] public float? DryMultiplier { get; set; }
        [JsonPropertyName("dry_base")] public float? DryBase { get; set; }
        [JsonPropertyName("dry_allowed_length")] public int? DryAllowedLength { get; set; }
        [JsonPropertyName("dry_penalty_last_n")] public int? DryPenaltyLastN { get; set; }
        [JsonPropertyName("dry_sequence_breakers")] public string[]? DrySequenceBreakers { get; set; }
        [JsonPropertyName("xtc_probability")] public float? XtcProbability { get; set; }
        [JsonPropertyName("xtc_threshold")] public float? XtcThreshold { get; set; }
        [JsonPropertyName("mirostat")] public int? Mirostat { get; set; }
        [JsonPropertyName("mirostat_tau")] public float? MirostatTau { get; set; }
        [JsonPropertyName("mirostat_eta")] public float? MirostatEta { get; set; }
        [JsonPropertyName("grammar")] public string? Grammar { get; set; }
        [JsonPropertyName("json_schema")] public JsonNode? JsonSchema { get; set; }
        [JsonPropertyName("seed")] public int? Seed { get; set; }
        [JsonPropertyName("ignore_eos")] public bool? IgnoreEos { get; set; }
        //[JsonPropertyName("logit_bias")] public List<object>? LogitBias { get; set; }   // TODO: how'd i do this...
        [JsonPropertyName("n_probs")] public int? NProbs { get; set; }
        [JsonPropertyName("min_keep")] public int? MinKeep { get; set; }
        [JsonPropertyName("t_max_predict_ms")] public int? TMaxPredictMs { get; set; }
        [JsonPropertyName("image_data")] public ImageData[]? ImageData { get; set; }
        [JsonPropertyName("id_slot")] public int? IdSlot { get; set; }
        [JsonPropertyName("cache_prompt")] public bool? CachePrompt { get; set; }
        [JsonPropertyName("return_tokens")] public bool? ReturnTokens { get; set; }
        [JsonPropertyName("samplers")] public string[]? Samplers { get; set; }
        [JsonPropertyName("timings_per_token")] public bool? TimingsPerToken { get; set; }
        [JsonPropertyName("post_sampling_probs")] public bool? PostSamplingProbs { get; set; }
        [JsonPropertyName("response_fields")] public string[]? ResponseFields { get; set; }
        [JsonPropertyName("lora")] public CompletionRequestLoRA[]? LoRA { get; set; }

        public class Builder {
            private CompletionRequest request;

            public Builder() {
                request = new() {
                    Prompt = null!
                };
            }

            public CompletionRequest Build() {
                if (request.Prompt is null) throw new InvalidOperationException("Prompt is not set!");
                return request;
            }

            public Builder SetPrompt(string value) { request.Prompt = value; return this; }
            public Builder SetTemperature(float? value) { request.Temperature = value; return this; }
            public Builder SetDynatempRange(float? value) { request.DynatempRange = value; return this; }
            public Builder SetDynatempExponent(float? value) { request.DynatempExponent = value; return this; }
            public Builder SetTopK(int? value) { request.TopK = value; return this; }
            public Builder SetTopP(float? value) { request.TopP = value; return this; }
            public Builder SetMinP(float? value) { request.MinP = value; return this; }
            public Builder SetNPredict(int? value) { request.NPredict = value; return this; }
            public Builder SetNIndent(int? value) { request.NIndent = value; return this; }
            public Builder SetNKeep(int? value) { request.NKeep = value; return this; }
            public Builder SetStop(string[]? value) { request.Stop = value; return this; }
            public Builder SetTypicalP(float? value) { request.TypicalP = value; return this; }
            public Builder SetRepeatPenalty(float? value) { request.RepeatPenalty = value; return this; }
            public Builder SetRepeatLastN(int? value) { request.RepeatLastN = value; return this; }
            public Builder SetPresencePenalty(float? value) { request.PresencePenalty = value; return this; }
            public Builder SetFrequencyPenalty(float? value) { request.FrequencyPenalty = value; return this; }
            public Builder SetDryMultiplier(float? value) { request.DryMultiplier = value; return this; }
            public Builder SetDryBase(float? value) { request.DryBase = value; return this; }
            public Builder SetDryAllowedLength(int? value) { request.DryAllowedLength = value; return this; }
            public Builder SetDryPenaltyLastN(int? value) { request.DryPenaltyLastN = value; return this; }
            public Builder SetDrySequenceBreakers(string[]? value) { request.DrySequenceBreakers = value; return this; }
            public Builder SetXtcProbability(float? value) { request.XtcProbability = value; return this; }
            public Builder SetXtcThreshold(float? value) { request.XtcThreshold = value; return this; }
            public Builder SetMirostat(int? value) { request.Mirostat = value; return this; }
            public Builder SetMirostatTau(float? value) { request.MirostatTau = value; return this; }
            public Builder SetMirostatEta(float? value) { request.MirostatEta = value; return this; }
            public Builder SetGrammar(string? value) { request.Grammar = value; return this; }
            public Builder SetJsonSchema(JsonNode? value) { request.JsonSchema = value; return this; }
            public Builder SetSeed(int? value) { request.Seed = value; return this; }
            public Builder SetIgnoreEos(bool? value) { request.IgnoreEos = value; return this; }
            public Builder SetNProbs(int? value) { request.NProbs = value; return this; }
            public Builder SetMinKeep(int? value) { request.MinKeep = value; return this; }
            public Builder SetTMaxPredictMs(int? value) { request.TMaxPredictMs = value; return this; }
            public Builder SetImageData(ImageData[]? value) { request.ImageData = value; return this; }
            public Builder SetIdSlot(int? value) { request.IdSlot = value; return this; }
            public Builder SetCachePrompt(bool? value) { request.CachePrompt = value; return this; }
            public Builder SetReturnTokens(bool? value) { request.ReturnTokens = value; return this; }
            public Builder SetSamplers(string[]? value) { request.Samplers = value; return this; }
            public Builder SetTimingsPerToken(bool? value) { request.TimingsPerToken = value; return this; }
            public Builder SetPostSamplingProbs(bool? value) { request.PostSamplingProbs = value; return this; }
            public Builder SetResponseFields(string[]? value) { request.ResponseFields = value; return this; }
            public Builder SetLoRA(CompletionRequestLoRA[]? value) { request.LoRA = value; return this; }
        }
    }
    
    public class CompletionRequestLoRA {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("scale")] public required float Scale { get; set; }
    }

    public class CompletionResponse {
        [JsonPropertyName("content")] public required string Content { get; set; }
        [JsonPropertyName("tokens")] public int[]? Tokens { get; set; }
        [JsonPropertyName("stop")] public bool Stop { get; set; }
        [JsonPropertyName("generation_settings")] public JsonNode? GenerationSettings { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("stop_type")] public string? StopType { get; set; }
        [JsonPropertyName("stopping_word")] public string? StoppingWord { get; set; }
        [JsonPropertyName("timings")] public Timings? Timings { get; set; }
        [JsonPropertyName("tokens_cached")] public int? TokensCached { get; set; }
        [JsonPropertyName("tokens_evaluated")] public int? TokensEvaluated { get; set; }
        [JsonPropertyName("truncated")] public bool Truncated { get; set; }
        [JsonPropertyName("probs")] public JsonNode[]? Probs { get; set; }
    }
#endregion Completion

#region Tokenize
    public class TokenizeRequest {
        [JsonPropertyName("content")] public required string Content { get; set; }
        [JsonPropertyName("add_special")] public bool AddSpecial { get; set; }
        [JsonPropertyName("with_pieces")] public bool WithPieces { get; set; }

        public class Builder {
            TokenizeRequest request;

            public Builder() {
                request = new() {
                    Content = null!,
                };
            }

            public TokenizeRequest Build() {
                if (request.Content is null) throw new InvalidOperationException("Content is not set!");
                return request;
            }

            public Builder SetContent(string value) { request.Content = value; return this; }
            public Builder SetAddSpecial(bool value) { request.AddSpecial = value; return this; }
        }
    }

    public class TokenizeTokensWithPieces {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("piece")] public required JsonNode Piece { get; set; }    // TODO: this can either be a string or an int array...
    }
#endregion Tokenize

#region Detokenize
    public class DetokenizeRequest {
        [JsonPropertyName("tokens")] public required int[] Tokens { get; set; }

        public class Builder {
            DetokenizeRequest request;

            public Builder() {
                request = new() {
                    Tokens = null!
                };
            }

            public DetokenizeRequest Build() {
                if (request.Tokens is null) throw new InvalidOperationException("Tokens are not set!");
                return request;
            }

            public Builder SetTokens(int[] value) { request.Tokens = value; return this; }
        }
    }
#endregion Detokenize

#region Apply Chat Template
    public class ApplyTemplateRequest {
        [JsonPropertyName("messages")] public required Message[] Messages { get; set; }

        public class Builder {
            private ApplyTemplateRequest request;

            public Builder() {
                request = new() {
                    Messages = null!
                };
            }

            public ApplyTemplateRequest Build() {
                if (request.Messages is null) throw new InvalidOperationException("Messages are not set!");
                return request;
            }

            public Builder SetMessages(Message[] value) { request.Messages = value; return this; }
        }
    }

    public class ApplyTemplateResponse {
        [JsonPropertyName("prompt")] public required string Prompt { get; set; }
    }
#endregion Apply Chat Template

#region Generate Embedding
    public class EmbeddingRequest {
        [JsonPropertyName("content")] public required string Content { get; set; }
        [JsonPropertyName("image_data")] public ImageData[]? ImageData { get; set; }

        public class Builder {
            private EmbeddingRequest request;

            public Builder() {
                request = new() {
                    Content = null!
                };
            }

            public EmbeddingRequest Build() {
                if (request.Content is null) throw new InvalidOperationException("Content is not set!");
                return request;
            }

            public Builder SetContent(string value) { request.Content = value; return this; }
            public Builder SetImageData(ImageData[]? value) { request.ImageData = value; return this; }
        }
    }

    public class EmbeddingResponse {
        [JsonPropertyName("index")] public required int Index { get; set; }
        [JsonPropertyName("embedding")] public required double[][] Embedding { get; set; }
    }
#endregion Generate Embedding

#region LoRA Adapters
    public class LoRAAdapterResponse {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("path")] public required string Path { get; set; }
        [JsonPropertyName("scale")] public required double Scale { get; set; }
    }

    public class LoRAAdapterRequest {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("scale")] public required double Scale { get; set; }
    }
#endregion LoRA Adapters

#region OpenAI-compatible Model Info
    public class OAIModelsResponse {
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("data")] public required OAIModelsResponseData[] Data { get; set; }
    }

    public class OAIModelsResponseData {
        [JsonPropertyName("id")] public required string Id { get; set; }
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("created")] public required long Created { get; set; }
        [JsonPropertyName("owned_by")] public required string OwnedBy { get; set; }
        [JsonPropertyName("meta")] public required OAIModelsResponseDataMeta Meta { get; set; }
    }

    public class OAIModelsResponseDataMeta {
        [JsonPropertyName("vocab_type")] public required int VocabType { get; set; }
        [JsonPropertyName("n_vocab")] public required long NVocab { get; set; }
        [JsonPropertyName("n_ctx_train")] public required long NCtxTrain { get; set; }
        [JsonPropertyName("n_embd")] public required long NEmbd { get; set; }
        [JsonPropertyName("n_params")] public required long NParams { get; set; }
        [JsonPropertyName("size")] public required long Size { get; set; }
    }
#endregion OpenAI-compatible Model Info

#region OpenAI-compatible Chat Completion
    public class OAIResponseFormat {
        [JsonPropertyName("type")] public required ResponseType Type { get; set; }
        [JsonPropertyName("schema")] public required JsonNode? Schema { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ResponseType {
            [JsonStringEnumMemberName("text")] Text,
            [JsonStringEnumMemberName("json_object")] JsonObject,
            [JsonStringEnumMemberName("json_schema")] JsonSchema,
        }

        public static JsonNode SchemaOf<T>() => JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(T));
    }

    public class OAIChatCompletionRequest {
        [JsonPropertyName("model")] public string? Model { get; set; }  // seems like unused and can be omitted on llama.cpp server
        [JsonPropertyName("messages")] public required Message[] Messages { get; set; }
        [JsonPropertyName("temperature")] public float? Temperature { get; set; }
        [JsonPropertyName("seed")] public int? Seed { get; set; }
        [JsonPropertyName("response_format")] public OAIResponseFormat? ResponseFormat { get; set; }
        [JsonPropertyName("frequency_penalty")] public float? FrequencyPenalty { get; set; }
        [JsonPropertyName("max_completion_tokens")] public int? MaxCompletionTokens { get; set; }
        [JsonPropertyName("n")] public int? N { get; set; }
        [JsonPropertyName("logprobs")] public bool? Logprobs { get; set; }
        [JsonPropertyName("top_logprobs")] public int? TopLogprobs { get; set; }
        [JsonPropertyName("presence_penalty")] public float? PresencePenalty { get; set; }
        [JsonPropertyName("stop")] public string[]? Stop { get; set; }
        [JsonPropertyName("top_p")] public float? TopP { get; set; }
        [JsonPropertyName("stream")] public bool? Stream { get; set; }

        public class Builder {
            private OAIChatCompletionRequest request;

            public Builder() {
                request = new OAIChatCompletionRequest() {
                    Model = null,
                    Messages = null!,
                };
            }

            public OAIChatCompletionRequest Build() {
                if (request.Messages is null) throw new InvalidOperationException("Messages are not set!");
                return request;
            }

            public Builder SetMessages(Message[] value) { request.Messages = value; return this; }
            public Builder SetTemperature(float? value) { request.Temperature = value; return this; }
            public Builder SetSeed(int value) { request.Seed = value; return this; }
            public Builder SetResponseFormat(OAIResponseFormat.ResponseType type, JsonNode? schema) { request.ResponseFormat = new OAIResponseFormat() { Type = type, Schema = schema }; return this; }
            public Builder SetFrequencyPenalty(float value) { request.FrequencyPenalty = value; return this; }
            public Builder SetMaxCompletionTokens(int value) { request.MaxCompletionTokens = value; return this; }
            public Builder SetN(int value) { request.N = value; return this; }
            public Builder SetLogprobs(bool value) { request.Logprobs = value; return this; }
            public Builder SetTopLogprobs(int value) { request.TopLogprobs = value; return this; }
            public Builder SetPresencePenalty(float value) { request.PresencePenalty = value; return this; }
            public Builder SetStop(string[] value) { request.Stop = value; return this; }
            public Builder SetTopP(float value) { request.TopP = value; return this; }
        }
    }

    public class OAIChatCompletionResponse {
        [JsonPropertyName("choices")] public required List<OAIChatCompletionResponseChoice> Choices { get; set; }
        [JsonPropertyName("created")] public long Created { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("system_fingerprint")] public string? SystemFingerprint { get; set; }
        [JsonPropertyName("object")] public string? Object { get; set; }
        [JsonPropertyName("usage")] public OAIChatCompletionResponseUsage? Usage { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("timings")] public Timings? Timings { get; set; }

        [JsonIgnore]
        public OAIChatCompletionResponseChoice FirstChoice { get { return Choices[0]; } }
    }

    public class OAIChatCompletionResponseChoice {
        [JsonPropertyName("finish_reason")] public required string FinishReason { get; set; }
        [JsonPropertyName("index")] public required int Index { get; set; }
        [JsonPropertyName("message")] public required OAIChatCompletionResponseChoiceMessage Message { get; set; }
    }

    public class OAIChatCompletionResponseChoiceMessage {
        [JsonPropertyName("role")] public required string Role { get; set; }
        [JsonPropertyName("content")] public required string Content { get; set; }
    }

    public class OAIChatCompletionResponseUsage {
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
    }

    public class OAIChatCompletionStreamResponse {
        [JsonPropertyName("choices")] public required List<OAIChatCompletionStreamResponseChoice> Choices { get; set; }
        [JsonPropertyName("created")] public long Created { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("system_fingerprint")] public string? SystemFingerprint { get; set; }
        [JsonPropertyName("object")] public string? Object { get; set; }
        [JsonPropertyName("usage")] public OAIChatCompletionResponseUsage? Usage { get; set; }
        [JsonPropertyName("timings")] public Timings? Timings { get; set; }

        [JsonIgnore]
        public OAIChatCompletionStreamResponseChoice FirstChoice { get { return Choices[0]; } }
    }

    public class OAIChatCompletionStreamResponseChoice {
        [JsonPropertyName("finish_reason")] public required string FinishReason { get; set; }
        [JsonPropertyName("index")] public required int Index { get; set; }
        [JsonPropertyName("delta")] public OAIChatCompletionStreamResponseChoiceDelta? Delta { get; set; }
    }

    // look at this stupid class name lmfao
    public class OAIChatCompletionStreamResponseChoiceDelta {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }
#endregion OpenAI-compatible Chat Completion

#region OpenAI-compatible Create Embeddings
    public class OAIEmbeddingsRequest {
        [JsonPropertyName("input")] public required string Input { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }  // seems like unused and can be omitted on llama.cpp server
        [JsonPropertyName("encoding_format")] public OAIEmbeddingsRequestEncodingFormat? EncodingFormat { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum OAIEmbeddingsRequestEncodingFormat {
            [JsonStringEnumMemberName("float")] Float,
            [JsonStringEnumMemberName("base64")] Base64,
        }

        public class Builder {
            private OAIEmbeddingsRequest request;

            public Builder() {
                request = new() {
                    Input = null!,
                };
            }

            public OAIEmbeddingsRequest Build() {
                if (request.Input is null) throw new InvalidOperationException("Input is not set!");
                return request;
            }

            public Builder SetInput(string value) { request.Input = value; return this; }
        }
    }

    public class OAIEmbeddingsFloatResponse {
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("data")] public required OAIEmbeddingsFloatResponseData[] Data { get; set; }
        [JsonPropertyName("model")] public required string Model { get; set; }
        [JsonPropertyName("usage")] public required OAIEmbeddingsResponseUsage Usage { get; set; }
    }

    public class OAIEmbeddingsFloatResponseData {
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("embedding")] public required double[] Embedding { get; set; }
        [JsonPropertyName("index")] public required int Index { get; set; }
    }

    public class OAIEmbeddingsBase64Response {
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("data")] public required OAIEmbeddingsBase64ResponseData[] Data { get; set; }
        [JsonPropertyName("model")] public required string Model { get; set; }
        [JsonPropertyName("usage")] public required OAIEmbeddingsResponseUsage Usage { get; set; }
    }

    public class OAIEmbeddingsBase64ResponseData {
        [JsonPropertyName("object")] public required string Object { get; set; }
        [JsonPropertyName("embedding")] public required string Embedding { get; set; }
        [JsonPropertyName("index")] public required int Index { get; set; }
    }

    public class OAIEmbeddingsResponseUsage {
        [JsonPropertyName("prompt_tokens")] public required int PromptTokens { get; set; }
        [JsonPropertyName("total_tokens")] public required int TotalTokens { get; set; }
    }
#endregion OpenAI-compatible Create Embeddings
}