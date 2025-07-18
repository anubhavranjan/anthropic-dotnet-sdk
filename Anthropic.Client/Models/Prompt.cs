using System;

namespace Anthropic.Client.Models
{
    /// <summary>
    /// Defines the prompt parameters for a completion request.
    /// </summary>
    public class Prompt
    {
        /// <summary>
        /// Model name (e.g., "claude-v1").
        /// </summary>
        public string Model { get; set; } = "claude-v1";

        /// <summary>
        /// The prompt text to send.
        /// </summary>
        public string PromptText { get; set; } = string.Empty;

        /// <summary>
        /// Maximum tokens to sample in the completion.
        /// </summary>
        public int MaxTokensToSample { get; set; } = 300;

        /// <summary>
        /// Sampling temperature (0.0–1.0).
        /// </summary>
        public double Temperature { get; set; } = 1.0;

        /// <summary>
        /// Gets the properly formatted prompt text for the Anthropic API.
        /// Ensures the prompt starts with "Human:" if not already formatted.
        /// </summary>
        public string GetFormattedPrompt()
        {
            if (string.IsNullOrWhiteSpace(PromptText))
                return string.Empty;

            // Check if the prompt is already properly formatted
            var trimmedPrompt = PromptText.Trim();
            if (trimmedPrompt.StartsWith("Human:", StringComparison.OrdinalIgnoreCase))
                return $"{trimmedPrompt}\n\nAssistant:";

            // Format the prompt with required Human: prefix
            return $"Human: {trimmedPrompt}\n\nAssistant:";
        }
    }
}
