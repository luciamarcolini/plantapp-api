using OpenAI;
using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace PlantAPI.Models
{
    public class OpenAIResponse
    {
        public string id { set; get; }
        public float created { set;get;}
        public string model { set; get; }
        public List<Choice> choices { set; get; } /*message*/
        public Usage usage { set; get; }
        public string system_fingerprint { get; set; }
    }

    public class Choice
    {
        [JsonInclude]
        [JsonPropertyName("message")]
        public Message Message { get;  set; }

        [JsonInclude]
        [JsonPropertyName("finish_reason")]
        public string FinishReason { get;  set; }

        [JsonInclude]
        [JsonPropertyName("index")]
        public int? Index { get;  set; }

        [JsonInclude]
        [JsonPropertyName("logprobs")]
        public LogProbs LogProbs { get;  set; }

    }
    public class Usage
    {
        [JsonInclude]
        [JsonPropertyName("prompt_tokens")]
        public int? PromptTokens { get;  set; }

        [JsonInclude]
        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get;  set; }

        [JsonInclude]
        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get;  set; }
    }

    public class Message
    {

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get;  set; }

        [JsonInclude]
        [JsonPropertyName("role")]
        public string Role { get;  set; }

        [JsonInclude]
        [JsonPropertyName("content")]
        public dynamic Content { get;  set; }

    }
}
