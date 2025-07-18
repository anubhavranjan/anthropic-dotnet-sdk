using System.Text.Json.Serialization;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents content in a message, which can be text, media, or tool-related data.
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Gets or sets the type of content (e.g., "text", "image", "tool_use", "tool_result").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the text content when the type is "text".
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        /// <summary>
        /// Gets or sets the media source when the content contains media (e.g., images).
        /// </summary>
        [JsonPropertyName("source")]
        public MediaSource? Source { get; set; }
        
        /// <summary>
        /// Gets or sets the cache control settings for this content.
        /// </summary>
        [JsonPropertyName("cache_control")]
        public CacheControl? CacheControl { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier for tool use content.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the tool being used.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        /// <summary>
        /// Gets or sets the input parameters for tool use.
        /// </summary>
        [JsonPropertyName("input")]
        public object? Input { get; set; }
        
        /// <summary>
        /// Gets or sets the tool use ID for tool result content.
        /// </summary>
        [JsonPropertyName("tool_use_id")]
        public string? ToolUseId { get; set; }
        
        /// <summary>
        /// Gets or sets the content of a tool result.
        /// </summary>
        [JsonPropertyName("content")]
        public string? ToolContent { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the tool result represents an error.
        /// </summary>
        [JsonPropertyName("is_error")]
        public bool? IsError { get; set; }
    }

    /// <summary>
    /// Represents the source of media content, typically containing base64-encoded data.
    /// </summary>
    public class MediaSource
    {
        /// <summary>
        /// Gets or sets the type of media source (typically "base64").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "base64";
        
        /// <summary>
        /// Gets or sets the MIME type of the media (e.g., "image/jpeg", "image/png").
        /// </summary>
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the base64-encoded media data.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }
}