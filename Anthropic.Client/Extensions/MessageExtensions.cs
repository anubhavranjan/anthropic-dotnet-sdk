using System.Collections.Generic;
using System.Linq;
using Anthropic.Client.Models;

namespace Anthropic.Client.Extensions
{
    /// <summary>
    /// Extension methods for working with messages and content.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Adds a text content block to the message.
        /// </summary>
        /// <param name="message">The message to add content to.</param>
        /// <param name="text">The text content to add.</param>
        /// <returns>The message for method chaining.</returns>
        public static Message AddText(this Message message, string text)
        {
            message.Content.Add(new Content { Type = "text", Text = text });
            return message;
        }

        /// <summary>
        /// Adds an image content block to the message.
        /// </summary>
        /// <param name="message">The message to add content to.</param>
        /// <param name="base64Data">The base64-encoded image data.</param>
        /// <param name="mediaType">The media type (e.g., "image/jpeg", "image/png").</param>
        /// <returns>The message for method chaining.</returns>
        public static Message AddImage(this Message message, string base64Data, string mediaType)
        {
            message.Content.Add(new Content
            {
                Type = "image",
                Source = new MediaSource
                {
                    Type = "base64",
                    MediaType = mediaType,
                    Data = base64Data
                }
            });
            return message;
        }

        /// <summary>
        /// Adds a tool result content block to the message.
        /// </summary>
        /// <param name="message">The message to add content to.</param>
        /// <param name="toolUseId">The ID of the tool use request.</param>
        /// <param name="result">The tool execution result.</param>
        /// <param name="isError">Whether the tool execution resulted in an error.</param>
        /// <returns>The message for method chaining.</returns>
        public static Message AddToolResult(this Message message, string toolUseId, string result, bool isError = false)
        {
            message.Content.Add(new Content
            {
                Type = "tool_result",
                ToolUseId = toolUseId,
                ToolContent = result,
                IsError = isError
            });
            return message;
        }

        /// <summary>
        /// Gets the first text content from the message.
        /// </summary>
        /// <param name="message">The message to get text from.</param>
        /// <returns>The first text content, or null if none exists.</returns>
        public static string? GetText(this Message message)
        {
            return message.Content.FirstOrDefault(c => c.Type == "text")?.Text;
        }

        /// <summary>
        /// Gets all tool use requests from the message.
        /// </summary>
        /// <param name="message">The message to get tool uses from.</param>
        /// <returns>A list of tool use content blocks.</returns>
        public static List<Content> GetToolUses(this Message message)
        {
            return message.Content.Where(c => c.Type == "tool_use").ToList();
        }

        /// <summary>
        /// Gets the first text content from the messages response.
        /// </summary>
        /// <param name="response">The messages response to get text from.</param>
        /// <returns>The first text content, or null if none exists.</returns>
        public static string? GetText(this MessagesResponse response)
        {
            return response.Content.FirstOrDefault(c => c.Type == "text")?.Text;
        }

        /// <summary>
        /// Gets all tool use requests from the messages response.
        /// </summary>
        /// <param name="response">The messages response to get tool uses from.</param>
        /// <returns>A list of tool use content blocks.</returns>
        public static List<Content> GetToolUses(this MessagesResponse response)
        {
            return response.Content.Where(c => c.Type == "tool_use").ToList();
        }

        /// <summary>
        /// Creates a user message with text content.
        /// </summary>
        /// <param name="text">The text content for the message.</param>
        /// <returns>A new user message with the specified text.</returns>
        public static Message CreateUserMessage(string text)
        {
            return new Message
            {
                Role = "user",
                Content = new List<Content> { new Content { Type = "text", Text = text } }
            };
        }

        /// <summary>
        /// Creates an assistant message with text content.
        /// </summary>
        /// <param name="text">The text content for the message.</param>
        /// <returns>A new assistant message with the specified text.</returns>
        public static Message CreateAssistantMessage(string text)
        {
            return new Message
            {
                Role = "assistant",
                Content = new List<Content> { new Content { Type = "text", Text = text } }
            };
        }
    }
}