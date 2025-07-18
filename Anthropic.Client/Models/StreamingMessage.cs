using System.Text.Json.Serialization;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents a streaming message chunk from the Claude API.
    /// </summary>
    public class StreamingMessage
    {
        /// <summary>
        /// Gets or sets the type of streaming event (e.g., "message_start", "content_block_delta", "message_stop").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the index of the content block being streamed.
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }
        
        /// <summary>
        /// Gets or sets the delta containing the incremental content updates.
        /// </summary>
        [JsonPropertyName("delta")]
        public StreamingDelta? Delta { get; set; }
        
        /// <summary>
        /// Gets or sets the complete message when the stream starts or ends.
        /// </summary>
        [JsonPropertyName("message")]
        public MessagesResponse? Message { get; set; }
        
        /// <summary>
        /// Gets or sets the token usage information for the streaming response.
        /// </summary>
        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    /// <summary>
    /// Represents incremental content updates in a streaming response.
    /// </summary>
    public class StreamingDelta
    {
        /// <summary>
        /// Gets or sets the type of delta content (e.g., "text_delta").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the incremental text content being streamed.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        /// <summary>
        /// Gets or sets the reason why the streaming stopped.
        /// </summary>
        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }
        
        /// <summary>
        /// Gets or sets the stop sequence that triggered the end of streaming.
        /// </summary>
        [JsonPropertyName("stop_sequence")]
        public string? StopSequence { get; set; }
    }
}