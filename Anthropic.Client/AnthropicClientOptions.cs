using System;
using System.ComponentModel.DataAnnotations;

namespace Anthropic.Client
{
    /// <summary>
    /// Configuration options for the Anthropic client.
    /// </summary>
    public class AnthropicClientOptions
    {
        /// <summary>
        /// Your Anthropic API key. This is required and must not be empty.
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for the API (default: https://api.anthropic.com).
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.anthropic.com";

        /// <summary>
        /// HTTP request timeout (default: 100s).
        /// Must be between 1 second and 10 minutes.
        /// </summary>
        [Range(1, 600)]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// Maximum number of retry attempts for failed requests (default: 3).
        /// Must be between 0 and 10.
        /// </summary>
        [Range(0, 10)]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Enable request/response logging for debugging (default: false).
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// Enable telemetry collection (default: true).
        /// </summary>
        public bool EnableTelemetry { get; set; } = true;

        /// <summary>
        /// Custom user agent string to append to requests.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Validates the configuration options.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new ArgumentException("API key is required and cannot be empty.", nameof(ApiKey));

            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new ArgumentException("Base URL is required and cannot be empty.", nameof(BaseUrl));

            if (!Uri.IsWellFormedUriString(BaseUrl, UriKind.Absolute))
                throw new ArgumentException("Base URL must be a valid absolute URI.", nameof(BaseUrl));

            if (Timeout < TimeSpan.FromSeconds(1) || Timeout > TimeSpan.FromMinutes(10))
                throw new ArgumentException("Timeout must be between 1 second and 10 minutes.", nameof(Timeout));

            if (MaxRetries < 0 || MaxRetries > 10)
                throw new ArgumentException("MaxRetries must be between 0 and 10.", nameof(MaxRetries));

            if (ApiKey.Length < 20)
                throw new ArgumentException("API key appears to be invalid (too short).", nameof(ApiKey));

            if (!ApiKey.StartsWith("sk-ant-", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("API key must start with 'sk-ant-'.", nameof(ApiKey));
        }
    }
}
