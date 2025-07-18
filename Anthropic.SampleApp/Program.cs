using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anthropic.Client;
using Anthropic.Client.Models;
using Anthropic.Client.Builders;
using Anthropic.Client.Extensions;
using Microsoft.Extensions.Logging;

namespace Anthropic.SampleApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<AnthropicClient>();

            var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("ANTHROPIC_API_KEY environment variable is not set.");
                Console.Write("Please enter your ANTHROPIC_API_KEY: ");
                apiKey = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("No API key provided. Exiting...");
                return;
            }
            
            // Create client with enhanced options
            var options = new AnthropicClientOptions
            {
                ApiKey = apiKey,
                EnableLogging = true,
                EnableTelemetry = true,
                MaxRetries = 3,
                UserAgent = "AnthropicSampleApp/1.0"
            };

            // Validate options
            try
            {
                options.Validate();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Configuration error: {ex.Message}");
                return;
            }

            using var client = new AnthropicClient(options, logger: logger);

            // Test all features
            await TestBuilderPattern(client);
            await TestExtensionMethods(client);
            await TestToolsWithHelpers(client);
            await TestStreamingWithTelemetry(client);
        }

        private static async Task TestBuilderPattern(AnthropicClient client)
        {
            Console.WriteLine("=== Testing Builder Pattern ===");

            try
            {
                // Use builder pattern for clean request creation
                var request = MessagesRequestBuilder.Create()
                    .WithModel(AnthropicModels.Claude35Sonnet)
                    .WithMaxTokens(100)
                    .WithTemperature(0.7)
                    .WithSystem("You are a helpful assistant that responds concisely.")
                    .AddUserMessage("What is the capital of France?")
                    .Build();

                var response = await client.CreateMessagesAsync(request);
                Console.WriteLine($"Response: {response.Content[0].Text}");
                Console.WriteLine($"Tokens: {response.Usage.InputTokens + response.Usage.OutputTokens}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }

        private static async Task TestExtensionMethods(AnthropicClient client)
        {
            Console.WriteLine("=== Testing Extension Methods ===");

            try
            {
                // Use extension methods for message creation
                var message = MessageExtensions.CreateUserMessage("Tell me a joke about programming.")
                    .AddText(" Make it clean and funny!");

                var request = new MessagesRequest
                {
                    Model = AnthropicModels.Claude35Sonnet,
                    MaxTokens = 150,
                    Messages = new List<Message> { message }
                };

                var response = await client.CreateMessagesAsync(request);
                Console.WriteLine($"Response: {response.GetText()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }

        private static async Task TestToolsWithHelpers(AnthropicClient client)
        {
            Console.WriteLine("=== Testing Tools with Helpers ===");

            try
            {
                var weatherTool = new Tool
                {
                    Name = "get_weather",
                    Description = "Get current weather information",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new { type = "string", description = "City name" }
                        },
                        required = new[] { "location" }
                    }
                };

                var request = MessagesRequestBuilder.Create()
                    .WithModel(AnthropicModels.Claude35Sonnet)
                    .WithMaxTokens(200)
                    .WithTools(weatherTool)
                    .WithToolChoice(ToolChoice.Auto)
                    .AddUserMessage("What's the weather like in London?")
                    .Build();

                var response = await client.CreateMessagesWithToolsAsync(request, request.Tools!);
                Console.WriteLine($"Response: {response.Content[0].Text}");
                
                // Handle tool use
                var toolUses = response.GetToolUses();
                if (toolUses.Count > 0)
                {
                    Console.WriteLine($"Tool requested: {toolUses[0].Name}");
                    
                    // Add tool result using extension method
                    var toolResultMessage = MessageExtensions.CreateUserMessage("")
                        .AddToolResult(toolUses[0].Id!, "London: 18°C, partly cloudy");
                    
                    Console.WriteLine("Tool result added successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }

        private static async Task TestStreamingWithTelemetry(AnthropicClient client)
        {
            Console.WriteLine("=== Testing Streaming with Telemetry ===");

            try
            {
                var request = MessagesRequestBuilder.Create()
                    .WithModel(AnthropicModels.Claude35Sonnet)
                    .WithMaxTokens(100)
                    .WithTemperature(0.8)
                    .AddUserMessage("Write a short poem about technology.")
                    .Build();

                Console.Write("Streaming poem: ");
                await foreach (var chunk in client.CreateStreamingMessagesAsync(request))
                {
                    if (chunk.Type == "content_block_delta" && chunk.Delta?.Text != null)
                    {
                        Console.Write(chunk.Delta.Text);
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
    }
}
