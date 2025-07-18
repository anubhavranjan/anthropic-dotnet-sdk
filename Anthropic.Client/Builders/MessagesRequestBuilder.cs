using System;
using System.Collections.Generic;
using Anthropic.Client.Models;
using Anthropic.Client.Extensions;

namespace Anthropic.Client.Builders
{
    /// <summary>
    /// Builder for creating MessagesRequest objects with a fluent API.
    /// </summary>
    public class MessagesRequestBuilder
    {
        private readonly MessagesRequest _request = new();

        /// <summary>
        /// Creates a new MessagesRequestBuilder instance.
        /// </summary>
        /// <returns>A new builder instance.</returns>
        public static MessagesRequestBuilder Create() => new();

        /// <summary>
        /// Sets the model to use for the request.
        /// </summary>
        /// <param name="model">The model name.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithModel(string model)
        {
            _request.Model = model;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of tokens to generate.
        /// </summary>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithMaxTokens(int maxTokens)
        {
            _request.MaxTokens = maxTokens;
            return this;
        }

        /// <summary>
        /// Sets the temperature for response generation.
        /// </summary>
        /// <param name="temperature">The temperature value (0.0 to 1.0).</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithTemperature(double temperature)
        {
            _request.Temperature = temperature;
            return this;
        }

        /// <summary>
        /// Sets the system message for the conversation.
        /// </summary>
        /// <param name="systemMessage">The system message.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithSystem(string systemMessage)
        {
            _request.System = systemMessage;
            return this;
        }

        /// <summary>
        /// Adds a user message to the conversation.
        /// </summary>
        /// <param name="text">The user message text.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder AddUserMessage(string text)
        {
            _request.Messages.Add(MessageExtensions.CreateUserMessage(text));
            return this;
        }

        /// <summary>
        /// Adds an assistant message to the conversation.
        /// </summary>
        /// <param name="text">The assistant message text.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder AddAssistantMessage(string text)
        {
            _request.Messages.Add(MessageExtensions.CreateAssistantMessage(text));
            return this;
        }

        /// <summary>
        /// Adds a custom message to the conversation.
        /// </summary>
        /// <param name="message">The message to add.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder AddMessage(Message message)
        {
            _request.Messages.Add(message);
            return this;
        }

        /// <summary>
        /// Adds tools to the request.
        /// </summary>
        /// <param name="tools">The tools to add.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithTools(params Tool[] tools)
        {
            _request.Tools = new List<Tool>(tools);
            return this;
        }

        /// <summary>
        /// Sets the tool choice strategy.
        /// </summary>
        /// <param name="toolChoice">The tool choice strategy.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithToolChoice(object toolChoice)
        {
            _request.ToolChoice = toolChoice;
            return this;
        }

        /// <summary>
        /// Sets stop sequences for the request.
        /// </summary>
        /// <param name="stopSequences">The stop sequences.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithStopSequences(params string[] stopSequences)
        {
            _request.StopSequences = new List<string>(stopSequences);
            return this;
        }

        /// <summary>
        /// Sets the top-p value for nucleus sampling.
        /// </summary>
        /// <param name="topP">The top-p value.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithTopP(double topP)
        {
            _request.TopP = topP;
            return this;
        }

        /// <summary>
        /// Sets the top-k value for sampling.
        /// </summary>
        /// <param name="topK">The top-k value.</param>
        /// <returns>The builder for method chaining.</returns>
        public MessagesRequestBuilder WithTopK(int topK)
        {
            _request.TopK = topK;
            return this;
        }

        /// <summary>
        /// Builds the MessagesRequest object.
        /// </summary>
        /// <returns>The configured MessagesRequest.</returns>
        public MessagesRequest Build()
        {
            // Basic validation
            if (_request.Messages.Count == 0)
                throw new InvalidOperationException("At least one message is required.");

            if (_request.MaxTokens <= 0)
                throw new InvalidOperationException("MaxTokens must be greater than 0.");

            return _request;
        }
    }
}