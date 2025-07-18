using System;

namespace Anthropic.Client.Exceptions
{
    /// <summary>
    /// Represents errors that occur during Anthropic API operations.
    /// </summary>
    public class AnthropicException : Exception
    {
        /// <summary>
        /// Gets or sets the type of error returned by the Anthropic API.
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with the error.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AnthropicException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AnthropicException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents errors that occur specifically from Anthropic API responses.
    /// </summary>
    public class AnthropicApiException : AnthropicException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicApiException"/> class with a specified error message, status code, and optional error type.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The HTTP status code associated with the error.</param>
        /// <param name="errorType">The type of error returned by the Anthropic API.</param>
        public AnthropicApiException(string message, int statusCode, string? errorType = null) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorType = errorType;
        }
    }
}