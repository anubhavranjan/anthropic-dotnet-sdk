using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents a message in a conversation with Claude.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the role of the message sender (e.g., "user", "assistant", "system").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the content of the message as a list of content blocks.
        /// </summary>
        [JsonPropertyName("content")]
        public List<Content> Content { get; set; } = new();
    }

    /// <summary>
    /// Represents the source of an image, typically containing base64-encoded data.
    /// </summary>
    public class ImageSource
    {
        /// <summary>
        /// Gets or sets the type of image source (typically "base64").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "base64";
        
        /// <summary>
        /// Gets or sets the MIME type of the image (e.g., "image/jpeg", "image/png").
        /// </summary>
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the base64-encoded image data.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a request to the Claude Messages API.
    /// </summary>
    public class MessagesRequest
    {
        /// <summary>
        /// Gets or sets the model to use for the request.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "claude-3-sonnet-20240229";
        
        /// <summary>
        /// Gets or sets the maximum number of tokens to generate in the response.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets the list of messages in the conversation.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the system message to provide context for the assistant.
        /// </summary>
        [JsonPropertyName("system")]
        public string? System { get; set; }
        
        /// <summary>
        /// Gets or sets the temperature for response generation (0.0 to 1.0).
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets a value indicating whether to stream the response.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of tools available to the assistant.
        /// </summary>
        [JsonPropertyName("tools")]
        public List<Tool>? Tools { get; set; }
        
        /// <summary>
        /// Gets or sets the tool choice configuration for the request.
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public object? ToolChoice { get; set; }
        
        /// <summary>
        /// Gets or sets the list of stop sequences that will halt generation.
        /// </summary>
        [JsonPropertyName("stop_sequences")]
        public List<string>? StopSequences { get; set; }
        
        /// <summary>
        /// Gets or sets the nucleus sampling parameter (0.0 to 1.0).
        /// </summary>
        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }
        
        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }
        
        /// <summary>
        /// Gets or sets additional metadata for the request.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Represents a response from the Claude Messages API.
    /// </summary>
    public class MessagesResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of response (typically "message").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the role of the response sender (typically "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the content of the response as a list of content blocks.
        /// </summary>
        [JsonPropertyName("content")]
        public List<Content> Content { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the model that generated the response.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the reason why the response generation stopped.
        /// </summary>
        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }
        
        /// <summary>
        /// Gets or sets the stop sequence that triggered the end of generation.
        /// </summary>
        [JsonPropertyName("stop_sequence")]
        public int? StopSequence { get; set; }
        
        /// <summary>
        /// Gets or sets the token usage information for the response.
        /// </summary>
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = new();
    }

    /// <summary>
    /// Represents token usage information for a request/response pair.
    /// </summary>
    public class Usage
    {
        /// <summary>
        /// Gets or sets the number of tokens in the input (prompt and messages).
        /// </summary>
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }
        
        /// <summary>
        /// Gets or sets the number of tokens in the output (response).
        /// </summary>
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}