namespace Anthropic.Client.Models
{
    /// <summary>
    /// Contains constant values for available Anthropic Claude models.
    /// </summary>
    public static class AnthropicModels
    {
        /// <summary>
        /// The Claude 3 Opus model identifier, the most capable model in the Claude 3 family.
        /// </summary>
        public const string Claude3Opus = "claude-3-opus-20240229";

        /// <summary>
        /// The Claude 3 Sonnet model identifier, offering a balance of performance and speed.
        /// </summary>
        public const string Claude3Sonnet = "claude-3-sonnet-20240229";

        /// <summary>
        /// The Claude 3 Haiku model identifier, the fastest model in the Claude 3 family.
        /// </summary>
        public const string Claude3Haiku = "claude-3-haiku-20240307";

        /// <summary>
        /// The Claude 3.5 Sonnet model identifier, an improved version of Claude 3 Sonnet.
        /// </summary>
        public const string Claude35Sonnet = "claude-3-5-sonnet-20241022";

        /// <summary>
        /// The Claude 3.5 Haiku model identifier, an improved version of Claude 3 Haiku.
        /// </summary>
        public const string Claude35Haiku = "claude-3-5-haiku-20241022";
    }
}