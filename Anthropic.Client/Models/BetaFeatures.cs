namespace Anthropic.Client.Models
{
    /// <summary>
    /// Contains constant values for Anthropic API beta features.
    /// </summary>
    public static class BetaFeatures
    {
        /// <summary>
        /// The beta feature identifier for computer use capabilities.
        /// </summary>
        public const string ComputerUse = "computer-use-2024-10-22";

        /// <summary>
        /// The beta feature identifier for prompt caching functionality.
        /// </summary>
        public const string PromptCaching = "prompt-caching-2024-07-31";

        /// <summary>
        /// The beta feature identifier for message batches support.
        /// </summary>
        public const string MessageBatches = "message-batches-2024-09-24";

        /// <summary>
        /// The beta feature identifier for PDF support functionality.
        /// </summary>
        public const string PDFSupport = "pdfs-2024-09-25";
    }
}