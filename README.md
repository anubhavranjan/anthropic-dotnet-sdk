# Anthropic.Client

[![NuGet](https://img.shields.io/nuget/v/Anthropic.Client.svg)](https://www.nuget.org/packages/Anthropic.Client)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.1%20%7C%20.NET%207%20%7C%20.NET%208%20%7C%20.NET%209-512BD4)](https://dotnet.microsoft.com)

A strongly-typed, feature-rich .NET client library for Anthropic's Claude API, providing easy integration with Claude's powerful language models.

## ✨ Features

- 🎯 **Strongly-typed models** with comprehensive IntelliSense support
- 🔄 **Async/await pattern** with full cancellation token support
- 🛠️ **Fluent builder pattern** for easy request construction
- 🔁 **Automatic retry logic** with exponential backoff and jitter
- 📊 **Streaming responses** for real-time chat experiences
- 🧰 **Tool support** for function calling capabilities
- 💾 **Prompt caching** for improved performance
- 📦 **Message batching** for high-volume processing
- 🔍 **Comprehensive logging** with Microsoft.Extensions.Logging
- 🏗️ **Extension methods** for simplified usage
- ⚡ **High performance** with System.Text.Json serialization
- 🔧 **Configurable options** with validation
- 🌐 **Multi-framework support** (.NET Standard 2.1, .NET 7, .NET 8, .NET 9)

## 📦 Installation

Install the package via NuGet Package Manager:
`dotnet add package Anthropic.Client`

Or via Package Manager Console:
`Install-Package Anthropic.Client`


## 🚀 Quick Start

### Basic Setup

```csharp
using Anthropic.Client; 
using Anthropic.Client.Models;
// Configure the client var options = new AnthropicClientOptions { ApiKey = "your-api-key-here", 
// or set ANTHROPIC_API_KEY environment variable EnableLogging = true, MaxRetries = 3, Timeout = TimeSpan.FromSeconds(100) };

// Create the client using var client = new AnthropicClient(options);
```

### Simple Message Request
```csharp
var request = new MessagesRequest { Model = AnthropicModels.Claude35Sonnet, MaxTokens = 1000, Messages = new List<Message> { new Message { Role = "user", Content = "Hello, Claude!" } } };
var response = await client.CreateMessagesAsync(request); Console.WriteLine(response.Content[0].Text);
```

## 📚 Available Models

The library includes constants for all available Claude models:
```csharp
// Claude 3.5 models (recommended) AnthropicModels.Claude35Sonnet  
// "claude-3-5-sonnet-20241022" AnthropicModels.Claude35Haiku   
// "claude-3-5-haiku-20241022"
// Claude 3 models AnthropicModels.Claude3Opus     
// "claude-3-opus-20240229" AnthropicModels.Claude3Sonnet   
// "claude-3-sonnet-20240229" AnthropicModels.Claude3Haiku    
// "claude-3-haiku-20240307"
```


## 🏗️ Builder Pattern

Use the fluent builder pattern for clean, readable request construction:
```csharp
using Anthropic.Client.Builders;
var request = MessagesRequestBuilder.Create()
	.WithModel(AnthropicModels.Claude35Sonnet)
	.WithMaxTokens(1000)
	.WithTemperature(0.7)
	.WithSystem("You are a helpful assistant.") 
	.AddUserMessage("Explain quantum computing") 
	.AddAssistantMessage("Quantum computing is...") 
	.AddUserMessage("Can you elaborate on quantum entanglement?") 
	.Build();
var response = await client.CreateMessagesAsync(request);
```


## 🔧 Extension Methods

Simplified usage with extension methods:

```csharp
using Anthropic.Client.Extensions;

// Quick single message 
var response = await client.SendMessageAsync( "What is the capital of France?", AnthropicModels.Claude35Sonnet);

// With system prompt 
var response = await client.SendMessageAsync( "Write a haiku about programming", AnthropicModels.Claude35Sonnet, systemPrompt: "You are a creative poet");

```

## 🔄 Streaming Responses

For real-time chat experiences:

```csharp
var request = new MessagesRequest 
{ 
	Model = AnthropicModels.Claude35Sonnet, 
	MaxTokens = 1000, 
	Messages = new List<Message> 
	{ 
		new Message 
		{ 
			Role = "user", 
			Content = "Tell me a story" 
		}
	}
};

await foreach (var chunk in client.CreateStreamingMessagesAsync(request)) 
{ 
	if (chunk.Type == "content_block_delta") 
	{ 
		Console.Write(chunk.Delta?.Text); 
	} 
}

```


## 🧰 Tool Support

Enable function calling capabilities:

```csharp
var tools = new List<Tool> 
{ 
	new Tool 
	{ 
		Name = "get_weather", 
		Description = "Get current weather for a location", 
		InputSchema = new ToolInputSchema 
		{ 
			Type = "object", 
			Properties = new Dictionary<string, object> 
			{ 
				["location"] = new { type = "string", description = "City name" } 
			}, 
			Required = new[] {"location"} 
		} 
	} 
};
var request = new MessagesRequest 
{ 
	Model = AnthropicModels.Claude35Sonnet, 
	MaxTokens = 1000, 
	Messages = new List<Message> 
	{ 
		new Message 
		{ 
			Role = "user", 
			Content = "What's the weather in New York?" 
		} 
	} 
};

var response = await client.CreateMessagesWithToolsAsync(request, tools);
```


## 💾 Prompt Caching

Improve performance with prompt caching:

```csharp
var request = new MessagesRequest 
{ 
	Model = AnthropicModels.Claude35Sonnet, 
	MaxTokens = 1000, 
	Messages = new List<Message> 
	{ 
		new Message 
		{ 
			Role = "user", 
			Content = "Large context that should be cached...", 
			CacheControl = new CacheControl 
			{ 
				Type = "ephemeral" 
			} 
		} 
	} 
};

var response = await client.CreateMessagesWithCachingAsync(request);

```


## 📦 Message Batching

Process multiple requests efficiently:

```csharp
var requests = new List<MessagesRequest> 
{ 
	new MessagesRequest 
	{ 
		Model = AnthropicModels.Claude35Sonnet, 
		MaxTokens = 100, 
		Messages = new List<Message> 
		{ 
			new Message 
			{ 
				Role = "user", 
				Content = "Translate 'Hello' to French" 
			} 
		} 
	}, 
	new MessagesRequest 
	{ 
		Model = AnthropicModels.Claude35Sonnet, 
		MaxTokens = 100, 
		Messages = new List<Message> 
		{ 
			new Message 
			{ 
				Role = "user", 
				Content = "Translate 'Hello' to Spanish" 
			} 
		} 
	} 
};

var batch = await client.CreateMessageBatchAsync(requests); 
Console.WriteLine($"Batch ID: {batch.Id}");

// Check batch status 
var status = await client.GetMessageBatchAsync(batch.Id); 
Console.WriteLine($"Status: {status.ProcessingStatus}");

```


## ⚙️ Configuration Options

```csharp

var options = new AnthropicClientOptions 
{ 
	ApiKey = "your-api-key",                    // Required 
	BaseUrl = "https://api.anthropic.com",      // Default API endpoint 
	Timeout = TimeSpan.FromSeconds(100),        // Request timeout 
	MaxRetries = 3,                             // Retry attempts 
	EnableLogging = true,                       // Enable request/response logging 
	EnableTelemetry = true,                     // Enable telemetry collection 
	UserAgent = "MyApp/1.0"                     // Custom user agent 
};

// Validate configuration 
options.Validate();

```


## 📊 Logging Integration

The client integrates with Microsoft.Extensions.Logging:

```csharp

using Microsoft.Extensions.Logging;

// Setup logging 
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<AnthropicClient>();

// Pass logger to client 
using var client = new AnthropicClient(options, logger: logger);

```


## 🔐 Authentication

Set your API key using one of these methods:

1. **Environment Variable (Recommended)**:
`export ANTHROPIC_API_KEY="your-api-key-here"`

2. **Configuration Options**:
`var options = new AnthropicClientOptions { ApiKey = "your-api-key-here" };`

3. **appsettings.json** (for ASP.NET Core):
```json
{ 
	"Anthropic": 
	{ 
		"ApiKey": "your-api-key-here" 
	} 
}

```


## 🔄 Dependency Injection

Register the client in your DI container:

```csharp

// Program.cs (ASP.NET Core) 
builder.Services.Configure<AnthropicClientOptions>(builder.Configuration.GetSection("Anthropic"));

builder.Services.AddHttpClient<AnthropicClient>(); 
builder.Services.AddScoped<AnthropicClient>();

```


## 🛡️ Error Handling

The client provides comprehensive error handling:

```csharp

try 
{ 
	var response = await client.CreateMessagesAsync(request); 
} 
catch (AnthropicApiException ex) 
{ 
	Console.WriteLine($"API Error: {ex.Message}"); 
	Console.WriteLine($"Status Code: {ex.StatusCode}"); 
	Console.WriteLine($"Error Type: {ex.ErrorType}"); 
} 
catch (HttpRequestException ex) 
{ 
	Console.WriteLine($"Network Error: {ex.Message}"); 
}

```


## 📈 Performance Tips

1. **Reuse the client instance** - it's thread-safe and designed for reuse
2. **Use streaming** for long responses to improve perceived performance
3. **Enable prompt caching** for repeated large contexts
4. **Use message batching** for high-volume scenarios
5. **Configure appropriate timeouts** based on your use case

## 🧪 Testing

The library includes comprehensive test coverage and supports mocking:

```csharp

// Example test setup 
var mockHttpClient = new Mock<HttpClient>(); 
var client = new AnthropicClient(options, mockHttpClient.Object);

```


## 🔧 Advanced Usage

### Custom HTTP Client

```csharp

var httpClient = new HttpClient(); 
httpClient.DefaultRequestHeaders.Add("Custom-Header", "Value");
using var client = new AnthropicClient(options, httpClient);

```


### Retry Configuration

```csharp

var options = new AnthropicClientOptions 
{ 
	ApiKey = "your-api-key", 
	MaxRetries = 5,  // Custom retry count 
	Timeout = TimeSpan.FromMinutes(2)  // Extended timeout 
};

```


## 📋 Requirements

- .NET Standard 2.1 or later
- .NET 7 or later
- .NET 8 or later  
- .NET 9 or later

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/Anthropic.Client)
- [GitHub Repository](https://github.com/anubhavranjan/anthropic-dotnet-sdk)
- [Anthropic API Documentation](https://docs.anthropic.com)

## 📞 Support

For issues and questions:
- Create an issue on [GitHub](https://github.com/anubhavranjan/anthropic-dotnet-sdk/issues)
- Check the [Anthropic API documentation](https://docs.anthropic.com)

---

Made with ❤️ by [Anubhav Ranjan](https://github.com/anubhavranjan)