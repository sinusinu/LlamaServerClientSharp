using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LlamaServerClientSharp;

public partial class LlamaClient {
#region Health
    public enum Health {
        Unknown,
        LoadingModel,
        Error,
        OK
    }
#endregion Health

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

    public class Message {
        public string role { get; set; }
        public string content { get; set; }

        public Message(string role, string content) {
            this.role = role;
            this.content = content;
        }
    }
#endregion Commonly used classes

#region Completion
    public class CompletionContent {
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
        [JsonPropertyName("image_data")] public CompletionContentImageData[]? ImageData { get; set; }
        [JsonPropertyName("id_slot")] public int? IdSlot { get; set; }
        [JsonPropertyName("cache_prompt")] public bool? CachePrompt { get; set; }
        [JsonPropertyName("return_tokens")] public bool? ReturnTokens { get; set; }
        [JsonPropertyName("samplers")] public string[]? Samplers { get; set; }
        [JsonPropertyName("timings_per_token")] public bool? TimingsPerToken { get; set; }
        [JsonPropertyName("post_sampling_probs")] public bool? PostSamplingProbs { get; set; }
        [JsonPropertyName("response_fields")] public string[]? ResponseFields { get; set; }
        [JsonPropertyName("lora")] public CompletionContentLoRA[]? LoRA { get; set; }

        public class Builder {
            private CompletionContent content;

            public Builder() {
                content = new() {
                    Prompt = null!
                };
            }

            public CompletionContent Build() {
                if (content.Prompt is null) throw new InvalidOperationException("Prompt is not set!");
                return content;
            }

            public Builder SetPrompt(string value) { content.Prompt = value; return this; }
            public Builder SetTemperature(float? value) { content.Temperature = value; return this; }
            public Builder SetDynatempRange(float? value) { content.DynatempRange = value; return this; }
            public Builder SetDynatempExponent(float? value) { content.DynatempExponent = value; return this; }
            public Builder SetTopK(int? value) { content.TopK = value; return this; }
            public Builder SetTopP(float? value) { content.TopP = value; return this; }
            public Builder SetMinP(float? value) { content.MinP = value; return this; }
            public Builder SetNPredict(int? value) { content.NPredict = value; return this; }
            public Builder SetNIndent(int? value) { content.NIndent = value; return this; }
            public Builder SetNKeep(int? value) { content.NKeep = value; return this; }
            public Builder SetStop(string[]? value) { content.Stop = value; return this; }
            public Builder SetTypicalP(float? value) { content.TypicalP = value; return this; }
            public Builder SetRepeatPenalty(float? value) { content.RepeatPenalty = value; return this; }
            public Builder SetRepeatLastN(int? value) { content.RepeatLastN = value; return this; }
            public Builder SetPresencePenalty(float? value) { content.PresencePenalty = value; return this; }
            public Builder SetFrequencyPenalty(float? value) { content.FrequencyPenalty = value; return this; }
            public Builder SetDryMultiplier(float? value) { content.DryMultiplier = value; return this; }
            public Builder SetDryBase(float? value) { content.DryBase = value; return this; }
            public Builder SetDryAllowedLength(int? value) { content.DryAllowedLength = value; return this; }
            public Builder SetDryPenaltyLastN(int? value) { content.DryPenaltyLastN = value; return this; }
            public Builder SetDrySequenceBreakers(string[]? value) { content.DrySequenceBreakers = value; return this; }
            public Builder SetXtcProbability(float? value) { content.XtcProbability = value; return this; }
            public Builder SetXtcThreshold(float? value) { content.XtcThreshold = value; return this; }
            public Builder SetMirostat(int? value) { content.Mirostat = value; return this; }
            public Builder SetMirostatTau(float? value) { content.MirostatTau = value; return this; }
            public Builder SetMirostatEta(float? value) { content.MirostatEta = value; return this; }
            public Builder SetGrammar(string? value) { content.Grammar = value; return this; }
            public Builder SetJsonSchema(JsonNode? value) { content.JsonSchema = value; return this; }
            public Builder SetSeed(int? value) { content.Seed = value; return this; }
            public Builder SetIgnoreEos(bool? value) { content.IgnoreEos = value; return this; }
            public Builder SetNProbs(int? value) { content.NProbs = value; return this; }
            public Builder SetMinKeep(int? value) { content.MinKeep = value; return this; }
            public Builder SetTMaxPredictMs(int? value) { content.TMaxPredictMs = value; return this; }
            public Builder SetImageData(CompletionContentImageData[]? value) { content.ImageData = value; return this; }
            public Builder SetIdSlot(int? value) { content.IdSlot = value; return this; }
            public Builder SetCachePrompt(bool? value) { content.CachePrompt = value; return this; }
            public Builder SetReturnTokens(bool? value) { content.ReturnTokens = value; return this; }
            public Builder SetSamplers(string[]? value) { content.Samplers = value; return this; }
            public Builder SetTimingsPerToken(bool? value) { content.TimingsPerToken = value; return this; }
            public Builder SetPostSamplingProbs(bool? value) { content.PostSamplingProbs = value; return this; }
            public Builder SetResponseFields(string[]? value) { content.ResponseFields = value; return this; }
            public Builder SetLoRA(CompletionContentLoRA[]? value) { content.LoRA = value; return this; }
        }
    }

    public class CompletionContentImageData {
        [JsonPropertyName("data")] public required string Data { get; set; }
        [JsonPropertyName("id")] public required int Id { get; set; }
    }
    
    public class CompletionContentLoRA {
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
    public class TokenizeContent {
        [JsonPropertyName("content")] public required string Content { get; set; }
        [JsonPropertyName("add_special")] public bool AddSpecial { get; set; }
        [JsonPropertyName("with_pieces")] public bool WithPieces { get; set; }

        public class Builder {
            TokenizeContent content;

            public Builder() {
                content = new() {
                    Content = null!,
                };
            }

            public TokenizeContent Build() {
                if (content.Content is null) throw new InvalidOperationException("Content is not set!");
                return content;
            }

            public Builder SetContent(string value) { content.Content = value; return this; }
            public Builder SetAddSpecial(bool value) { content.AddSpecial = value; return this; }
        }
    }

    public class TokenizeTokensWithPieces {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("piece")] public required JsonNode Piece { get; set; }    // TODO: this can either be a string or an int array...
    }
#endregion Tokenize

#region Detokenize
    public class DetokenizeContent {
        [JsonPropertyName("tokens")] public required int[] Tokens { get; set; }

        public class Builder {
            DetokenizeContent content;

            public Builder() {
                content = new() {
                    Tokens = null!
                };
            }

            public DetokenizeContent Build() {
                if (content.Tokens is null) throw new InvalidOperationException("Tokens are not set!");
                return content;
            }

            public Builder SetTokens(int[] value) { content.Tokens = value; return this; }
        }
    }
#endregion Tokenize

#region Apply chat template
    public class ApplyTemplateContent {
        [JsonPropertyName("messages")] public required Message[] Messages { get; set; }

        public class Builder {
            private ApplyTemplateContent content;

            public Builder() {
                content = new() {
                    Messages = null!
                };
            }

            public ApplyTemplateContent Build() {
                if (content.Messages is null) throw new InvalidOperationException("Messages are not set!");
                return content;
            }

            public Builder SetMessages(Message[] value) { content.Messages = value; return this; }
        }
    }

    public class ApplyTemplateResponse {
        [JsonPropertyName("prompt")] public required string Prompt { get; set; }
    }
#endregion Apply chat template

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
    }

    public class OAIChatCompletionContent {
        [JsonPropertyName("model")] public required string Model { get; set; }
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
            private OAIChatCompletionContent content;

            public Builder() {
                content = new OAIChatCompletionContent() {
                    Model = "",
                    Messages = null!,
                };
            }

            public OAIChatCompletionContent Build() {
                if (content.Messages is null) throw new InvalidOperationException("Messages are not set!");
                return content;
            }

            public Builder SetMessages(Message[] value) { content.Messages = value; return this; }
            public Builder SetTemperature(float? value) { content.Temperature = value; return this; }
            public Builder SetSeed(int value) { content.Seed = value; return this; }
            public Builder SetResponseFormat(OAIResponseFormat.ResponseType type, JsonNode? schema) { content.ResponseFormat = new OAIResponseFormat() { Type = type, Schema = schema }; return this; }
            public Builder SetFrequencyPenalty(float value) { content.FrequencyPenalty = value; return this; }
            public Builder SetMaxCompletionTokens(int value) { content.MaxCompletionTokens = value; return this; }
            public Builder SetN(int value) { content.N = value; return this; }
            public Builder SetLogprobs(bool value) { content.Logprobs = value; return this; }
            public Builder SetTopLogprobs(int value) { content.TopLogprobs = value; return this; }
            public Builder SetPresencePenalty(float value) { content.PresencePenalty = value; return this; }
            public Builder SetStop(string[] value) { content.Stop = value; return this; }
            public Builder SetTopP(float value) { content.TopP = value; return this; }
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
}