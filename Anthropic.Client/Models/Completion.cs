namespace Anthropic.Client.Models
{
    /// <summary>
    /// Represents a completion response.
    /// </summary>
    public class Completion
    {
        /// <summary>
        /// Gets or sets the generated completion text.
        /// </summary>
        public string CompletionText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason why the completion stopped generating.
        /// </summary>
        public string StopReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model that generated the completion.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the model that generated the completion.
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of tokens used in the completion.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens used in the prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens used (prompt + completion).
        /// </summary>
        public int TotalTokens { get; set; }
    }
}