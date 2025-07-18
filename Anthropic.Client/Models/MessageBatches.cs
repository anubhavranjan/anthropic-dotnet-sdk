using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents a batch of messages processed by the Anthropic API.
    /// </summary>
    public class MessageBatch
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message batch.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of batch (typically "message_batch").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "message_batch";
        
        /// <summary>
        /// Gets or sets the current processing status of the batch.
        /// </summary>
        [JsonPropertyName("processing_status")]
        public string ProcessingStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the counts of requests in different states within the batch.
        /// </summary>
        [JsonPropertyName("request_counts")]
        public BatchRequestCounts RequestCounts { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the timestamp when the batch processing ended.
        /// </summary>
        [JsonPropertyName("ended_at")]
        public DateTime? EndedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the batch was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the batch will expire.
        /// </summary>
        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets the URL where batch results can be retrieved.
        /// </summary>
        [JsonPropertyName("results_url")]
        public string? ResultsUrl { get; set; }
    }

    /// <summary>
    /// Represents the counts of requests in different states within a message batch.
    /// </summary>
    public class BatchRequestCounts
    {
        /// <summary>
        /// Gets or sets the number of requests currently being processed.
        /// </summary>
        [JsonPropertyName("processing")]
        public int Processing { get; set; }
        
        /// <summary>
        /// Gets or sets the number of requests that completed successfully.
        /// </summary>
        [JsonPropertyName("succeeded")]
        public int Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the number of requests that resulted in errors.
        /// </summary>
        [JsonPropertyName("errored")]
        public int Errored { get; set; }
        
        /// <summary>
        /// Gets or sets the number of requests that were canceled.
        /// </summary>
        [JsonPropertyName("canceled")]
        public int Canceled { get; set; }
        
        /// <summary>
        /// Gets or sets the number of requests that expired before processing.
        /// </summary>
        [JsonPropertyName("expired")]
        public int Expired { get; set; }
    }
}