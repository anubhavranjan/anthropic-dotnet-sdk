using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.Client.Models;
using Anthropic.Client.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Text.Json.Serialization;

namespace Anthropic.Client
{
    /// <summary>
    /// Main client for interacting with the Anthropic API.
    /// </summary>
    public class AnthropicClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<AnthropicClient>? _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicClient"/> class.
        /// </summary>
        /// <param name="options">The configuration options for the client.</param>
        /// <param name="httpClient">Optional HTTP client to use for requests.</param>
        /// <param name="logger">Optional logger for client operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        /// <exception cref="ArgumentException">Thrown when API key is null or empty.</exception>
        public AnthropicClient(AnthropicClientOptions options, HttpClient? httpClient = null, ILogger<AnthropicClient>? logger = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new ArgumentException("API key must be provided.", nameof(options.ApiKey));

            _httpClient = httpClient ?? new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl),
                Timeout = options.Timeout
            };

            if (httpClient == null)
            {
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
            else
            {
                // Ensure the HttpClient is configured correctly
                if (string.IsNullOrWhiteSpace(_httpClient.BaseAddress?.ToString()))
                    _httpClient.BaseAddress = new Uri(options.BaseUrl);
                if (!_httpClient.DefaultRequestHeaders.Contains("anthropic-version"))
                    _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
                    _httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Add this line
            };

            _logger = logger;
            // Update the retry policy to use Polly v8 syntax
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>(ex => !CancellationToken.None.IsCancellationRequested)
                .OrResult<HttpResponseMessage>(response => ShouldRetry(response))
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => 
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
#if NET7_0_OR_GREATER
                        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
#else
                        var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
#endif
                        return baseDelay.Add(jitter);
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        if (outcome.Exception != null)
                        {
                            _logger?.LogWarning("Retry attempt {RetryCount} after {Delay}ms due to exception: {Exception}", 
                                retryCount, timespan.TotalMilliseconds, outcome.Exception.Message);
                        }
                        else if (outcome.Result != null)
                        {
                            _logger?.LogWarning("Retry attempt {RetryCount} after {Delay}ms due to HTTP {StatusCode}", 
                                retryCount, timespan.TotalMilliseconds, outcome.Result.StatusCode);
                        }
                    });
        }

        /// <summary>
        /// Creates a completion for the given prompt asynchronously.
        /// </summary>
        /// <param name="prompt">The prompt to send to the API.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the completion response.</returns>
        public async Task<Completion> CreateCompletionAsync(Prompt prompt, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = prompt.Model,
                prompt = prompt.GetFormattedPrompt(), // Use the formatted prompt method
                max_tokens_to_sample = prompt.MaxTokensToSample,
                temperature = prompt.Temperature,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.PostAsync("/v1/complete", content, cancellationToken))
                .ConfigureAwait(false);

            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }

            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Received empty response from API.");

            var dto = JsonSerializer.Deserialize<CompletionResponseDto>(raw, _jsonOptions)
                      ?? throw new InvalidOperationException("Failed to parse response.");

            return new Completion
            {
                CompletionText = dto.Completion,
                StopReason = dto.StopReason,
                Model = dto.Model,
                ModelVersion = dto.ModelVersion,
                CompletionTokens = dto.CompletionTokens,
                PromptTokens = dto.PromptTokens,
                TotalTokens = dto.TotalTokens
            };
        }

        /// <summary>
        /// Creates a message completion asynchronously.
        /// </summary>
        /// <param name="request">The message request to send to the API.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the message response.</returns>
        public async Task<MessagesResponse> CreateMessagesAsync(MessagesRequest request, CancellationToken cancellationToken = default)
        {
            request.Stream = false;
            
            // Serialize the full request object instead of creating anonymous object
            var payload = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.PostAsync("/v1/messages", content, cancellationToken))
                .ConfigureAwait(false);

            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }

            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Received empty response from API.");

            var messagesResponse = JsonSerializer.Deserialize<MessagesResponse>(raw, _jsonOptions)
                                 ?? throw new InvalidOperationException("Failed to parse response.");

            return messagesResponse;
        }

        /// <summary>
        /// Creates a message with tools support.
        /// </summary>
        /// <param name="request">The message request to send to the API.</param>
        /// <param name="tools">The list of tools available to the assistant.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the message response.</returns>
        public async Task<MessagesResponse> CreateMessagesWithToolsAsync(
            MessagesRequest request, 
            List<Tool> tools, 
            CancellationToken cancellationToken = default)
        {
            request.Tools = tools;
            request.Stream = false;
            
            // Add beta header for tools
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
            requestMessage.Headers.Add("anthropic-beta", "tools-2024-04-04");
            
            var payload = JsonSerializer.Serialize(request, _jsonOptions);
            requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.SendAsync(requestMessage, cancellationToken))
                .ConfigureAwait(false);
            
            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }
            
            var messagesResponse = JsonSerializer.Deserialize<MessagesResponse>(raw, _jsonOptions)
                                 ?? throw new InvalidOperationException("Failed to parse response.");

            return messagesResponse;
        }

        /// <summary>
        /// Creates a message with prompt caching enabled.
        /// </summary>
        /// <param name="request">The message request to send to the API.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the message response.</returns>
        public async Task<MessagesResponse> CreateMessagesWithCachingAsync(
            MessagesRequest request, 
            CancellationToken cancellationToken = default)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
            requestMessage.Headers.Add("anthropic-beta", BetaFeatures.PromptCaching);
            
            var payload = JsonSerializer.Serialize(request, _jsonOptions);
            requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.SendAsync(requestMessage, cancellationToken))
                .ConfigureAwait(false);
            
            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }
            
            var messagesResponse = JsonSerializer.Deserialize<MessagesResponse>(raw, _jsonOptions)
                                 ?? throw new InvalidOperationException("Failed to parse response.");

            return messagesResponse;
        }

        /// <summary>
        /// Creates a message batch for high-volume processing.
        /// </summary>
        /// <param name="requests">The list of message requests to process in the batch.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the batch response.</returns>
        public async Task<MessageBatch> CreateMessageBatchAsync(
            List<MessagesRequest> requests, 
            CancellationToken cancellationToken = default)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/messages/batches");
            requestMessage.Headers.Add("anthropic-beta", BetaFeatures.MessageBatches);
            
            var payload = new { requests = requests };
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
            requestMessage.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.SendAsync(requestMessage, cancellationToken))
                .ConfigureAwait(false);
            
            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }
            
            var batch = JsonSerializer.Deserialize<MessageBatch>(raw, _jsonOptions)
              ?? throw new InvalidOperationException("Failed to parse response.");

            return batch;
        }

        /// <summary>
        /// Gets the status of a message batch.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the batch status.</returns>
        public async Task<MessageBatch> GetMessageBatchAsync(string batchId, CancellationToken cancellationToken = default)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/v1/messages/batches/{batchId}");
            requestMessage.Headers.Add("anthropic-beta", BetaFeatures.MessageBatches);
            
            using var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.SendAsync(requestMessage, cancellationToken))
                .ConfigureAwait(false);
            
            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }
            
            var batch = JsonSerializer.Deserialize<MessageBatch>(raw, _jsonOptions)
              ?? throw new InvalidOperationException("Failed to parse response.");

            return batch;
        }

        /// <summary>
        /// Synchronous wrapper around CreateCompletionAsync.
        /// </summary>
        /// <param name="prompt">The prompt to send to the API.</param>
        /// <returns>The completion response.</returns>
        public Completion CreateCompletion(Prompt prompt)
            => CreateCompletionAsync(prompt).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper around CreateMessagesAsync.
        /// </summary>
        /// <param name="request">The message request to send to the API.</param>
        /// <returns>The message response.</returns>
        public MessagesResponse CreateMessages(MessagesRequest request)
            => CreateMessagesAsync(request).GetAwaiter().GetResult();

        /// <summary>
        /// Streams messages asynchronously for the given request.
        /// </summary>
        /// <param name="request">The message request to send to the API.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An async enumerable of streaming message chunks.</returns>
        public async IAsyncEnumerable<StreamingMessage> CreateStreamingMessagesAsync(
    MessagesRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;

            var payload = new
            {
                model = request.Model,
                max_tokens = request.MaxTokens,
                messages = request.Messages,
                system = request.System,
                temperature = request.Temperature,
                stream = request.Stream
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient
                .PostAsync("/v1/messages", content, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                await HandleErrorResponseAsync(response, raw).ConfigureAwait(false);
            }

            // Use ReadAsStreamAsync without CancellationToken for .NET Standard 2.1 compatibility
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            await foreach (var line in ReadLinesAsync(reader, cancellationToken))
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]") break;

                    StreamingMessage? streamingMessage = null;
                    bool parseFailed = false;
                    try
                    {
                        streamingMessage = JsonSerializer.Deserialize<StreamingMessage>(data, _jsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        parseFailed = true;
                        _logger?.LogWarning(ex, "Failed to parse streaming message: {Data}", data);
                    }
                    if (!parseFailed && streamingMessage != null)
                        yield return streamingMessage;
                }
            }
        }

        /// <summary>
        /// Reads lines from a StreamReader asynchronously.
        /// </summary>
        /// <param name="reader">The StreamReader to read from.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An async enumerable of lines.</returns>
        private static async IAsyncEnumerable<string> ReadLinesAsync(
            StreamReader reader, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return line;
            }
        }

        /// <summary>
        /// Handles error responses from the API.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="raw">The raw response content.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="AnthropicApiException">Thrown when the API returns an error response.</exception>
        private Task HandleErrorResponseAsync(HttpResponseMessage response, string raw)
        {
            var statusCode = (int)response.StatusCode;
            var errorMessage = $"API request failed with status {response.StatusCode}";
            string? errorType = null;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(raw, _jsonOptions);
                    if (errorResponse.TryGetProperty("error", out var errorElement))
                    {
                        if (errorElement.TryGetProperty("type", out var typeElement))
                        {
                            errorType = typeElement.GetString();
                        }
                        if (errorElement.TryGetProperty("message", out var messageElement))
                        {
                            errorMessage = messageElement.GetString() ?? errorMessage;
                        }
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, use the raw response
                    errorMessage = $"{errorMessage}: {raw}";
                }
            }

            _logger?.LogError("API request failed: {StatusCode} - {ErrorMessage}", statusCode, errorMessage);
            throw new AnthropicApiException(errorMessage, statusCode, errorType);
        }

        /// <summary>
        /// Determines if an HTTP response should be retried.
        /// </summary>
        /// <param name="response">The HTTP response message to evaluate.</param>
        /// <returns>True if the response should be retried, false otherwise.</returns>
        private static bool ShouldRetry(HttpResponseMessage response)
        {
            // Retry on server errors (5xx) and specific client errors
            return response.StatusCode >= System.Net.HttpStatusCode.InternalServerError ||
                   response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                   response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                   response.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                   response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                   response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AnthropicClient"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AnthropicClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Data transfer object for completion responses from the Anthropic API.
    /// </summary>
    internal class CompletionResponseDto
    {
        /// <summary>
        /// Gets or sets the completion text.
        /// </summary>
        public string Completion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason the completion stopped.
        /// </summary>
        public string StopReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model used for the completion.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model version used for the completion.
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of tokens in the completion.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens in the prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens used.
        /// </summary>
        public int TotalTokens { get; set; }
    }
}