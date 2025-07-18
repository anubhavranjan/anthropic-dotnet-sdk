using System.Text.Json.Serialization;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents cache control settings for prompt caching functionality.
    /// </summary>
    public class CacheControl
    {
        /// <summary>
        /// Gets or sets the type of cache control (typically "ephemeral").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "ephemeral";
    }
}