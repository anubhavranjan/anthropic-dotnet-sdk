using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Anthropic.Client.Telemetry
{
    /// <summary>
    /// Telemetry and metrics collection for the Anthropic client.
    /// </summary>
    public class AnthropicTelemetry
    {
        private static readonly ActivitySource ActivitySource = new("Anthropic.Client");
        private static readonly Meter Meter = new("Anthropic.Client");
        
        private readonly Counter<long> _requestCounter;
        private readonly Counter<long> _tokenCounter;
        private readonly Histogram<double> _requestDuration;
        private readonly Counter<long> _errorCounter;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the AnthropicTelemetry class.
        /// </summary>
        /// <param name="logger">Optional logger for telemetry events.</param>
        public AnthropicTelemetry(ILogger? logger = null)
        {
            _logger = logger;
            _requestCounter = Meter.CreateCounter<long>("anthropic_requests_total", "requests", "Total number of API requests");
            _tokenCounter = Meter.CreateCounter<long>("anthropic_tokens_total", "tokens", "Total number of tokens processed");
            _requestDuration = Meter.CreateHistogram<double>("anthropic_request_duration", "seconds", "Duration of API requests");
            _errorCounter = Meter.CreateCounter<long>("anthropic_errors_total", "errors", "Total number of API errors");
        }

        /// <summary>
        /// Starts tracking an API request.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="model">The model being used.</param>
        /// <returns>A disposable activity for tracking the request.</returns>
        public Activity? StartActivity(string operationName, string? model = null)
        {
            var activity = ActivitySource.StartActivity($"anthropic.{operationName}");
            activity?.SetTag("anthropic.model", model);
            activity?.SetTag("anthropic.operation", operationName);
            return activity;
        }

        /// <summary>
        /// Records a successful API request.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="model">The model used.</param>
        /// <param name="duration">The request duration.</param>
        /// <param name="inputTokens">Number of input tokens.</param>
        /// <param name="outputTokens">Number of output tokens.</param>
        public void RecordRequest(string operationName, string model, TimeSpan duration, int inputTokens = 0, int outputTokens = 0)
        {
            var baseTags = new KeyValuePair<string, object?>[]
            {
                new("operation", operationName),
                new("model", model),
                new("status", "success")
            };

            _requestCounter.Add(1, baseTags);
            _requestDuration.Record(duration.TotalSeconds, baseTags);
            
            if (inputTokens > 0)
            {
                var inputTokenTags = new KeyValuePair<string, object?>[]
                {
                    new("operation", operationName),
                    new("model", model),
                    new("status", "success"),
                    new("token_type", "input")
                };
                _tokenCounter.Add(inputTokens, inputTokenTags);
            }
            
            if (outputTokens > 0)
            {
                var outputTokenTags = new KeyValuePair<string, object?>[]
                {
                    new("operation", operationName),
                    new("model", model),
                    new("status", "success"),
                    new("token_type", "output")
                };
                _tokenCounter.Add(outputTokens, outputTokenTags);
            }

            _logger?.LogDebug("API request completed: {Operation} with {Model} in {Duration}ms", 
                operationName, model, duration.TotalMilliseconds);
        }

        /// <summary>
        /// Records a failed API request.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="model">The model used.</param>
        /// <param name="duration">The request duration.</param>
        /// <param name="errorType">The type of error.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public void RecordError(string operationName, string model, TimeSpan duration, string errorType, int? statusCode = null)
        {
            var baseTags = new List<KeyValuePair<string, object?>>
            {
                new("operation", operationName),
                new("model", model),
                new("status", "error"),
                new("error_type", errorType)
            };

            if (statusCode.HasValue)
            {
                baseTags.Add(new("status_code", statusCode.Value.ToString()));
            }

            var tags = baseTags.ToArray();
            _requestCounter.Add(1, tags);
            _errorCounter.Add(1, tags);
            _requestDuration.Record(duration.TotalSeconds, tags);

            _logger?.LogWarning("API request failed: {Operation} with {Model} - {ErrorType} ({StatusCode})", 
                operationName, model, errorType, statusCode);
        }

        /// <summary>
        /// Disposes the telemetry resources.
        /// </summary>
        public void Dispose()
        {
            ActivitySource.Dispose();
            Meter.Dispose();
        }
    }
}