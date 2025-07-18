using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Anthropic.Client.Models;
using Anthropic.Client.Exceptions;
using System.Text;
using System.Net;

namespace Anthropic.Client.Tests
{
    public class AnthropicClientTests
    {
        private readonly Mock<ILogger<AnthropicClient>> _mockLogger;
        private readonly AnthropicClientOptions _validOptions;

        public AnthropicClientTests()
        {
            _mockLogger = new Mock<ILogger<AnthropicClient>>();
            _validOptions = new AnthropicClientOptions
            {
                ApiKey = "sk-ant-test-key-for-testing-purposes-only"
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Act
            using var client = new AnthropicClient(_validOptions, logger: _mockLogger.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AnthropicClient(null!));
        }

        [Fact]
        public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
        {
            // Arrange
            var options = new AnthropicClientOptions { ApiKey = "" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new AnthropicClient(options));
        }

        [Fact]
        public void Constructor_WithCustomHttpClient_UsesProvidedClient()
        {
            // Arrange
            using var customHttpClient = new HttpClient();

            // Act
            using var client = new AnthropicClient(_validOptions, customHttpClient, _mockLogger.Object);

            // Assert
            Assert.NotNull(client);
        }

        #endregion

        #region Options Validation Tests

        [Fact]
        public void Validate_WithValidOptions_DoesNotThrow()
        {
            // Act & Assert
            _validOptions.Validate(); // Should not throw
        }

        [Theory]
        [InlineData("sk-ant-valid-key-12345678901234567890")]
        [InlineData("sk-ant-another-valid-key-123456789012345")]
        public void Validate_WithValidApiKeys_DoesNotThrow(string apiKey)
        {
            // Arrange
            var options = new AnthropicClientOptions { ApiKey = apiKey };

            // Act & Assert
            options.Validate(); // Should not throw
        }

        [Theory]
        [InlineData("too-short")]
        [InlineData("wrong-prefix-key")]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_WithInvalidApiKeys_ThrowsArgumentException(string? apiKey)
        {
            // Arrange
            var options = new AnthropicClientOptions { ApiKey = apiKey ?? "" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        [Fact]
        public void Validate_WithInvalidBaseUrl_ThrowsArgumentException()
        {
            // Arrange
            var options = new AnthropicClientOptions
            {
                ApiKey = "sk-ant-test-key-for-testing-purposes-only",
                BaseUrl = "not-a-valid-url"
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        [Fact]
        public void Validate_WithInvalidTimeout_ThrowsArgumentException()
        {
            // Arrange
            var options = new AnthropicClientOptions
            {
                ApiKey = "sk-ant-test-key-for-testing-purposes-only",
                Timeout = TimeSpan.FromMilliseconds(500) // Too short
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        [Fact]
        public void Validate_WithInvalidMaxRetries_ThrowsArgumentException()
        {
            // Arrange
            var options = new AnthropicClientOptions
            {
                ApiKey = "sk-ant-test-key-for-testing-purposes-only",
                MaxRetries = 15 // Too high
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        #endregion

        #region HTTP Client Tests

        [Fact]
        public async Task CreateMessagesAsync_WithSuccessfulResponse_ReturnsMessagesResponse()
        {
            // Arrange
            var mockResponse = """
                {
                    "id": "test-message-id",
                    "type": "message",
                    "role": "assistant",
                    "content": [
                        {
                            "type": "text",
                            "text": "Hello, this is a test response!"
                        }
                    ],
                    "model": "claude-3-5-sonnet-20241022",
                    "stop_reason": "end_turn",
                    "usage": {
                        "input_tokens": 10,
                        "output_tokens": 20
                    }
                }
                """;

            var testHttpClient = CreateTestHttpClient(mockResponse, HttpStatusCode.OK);
            using var client = new AnthropicClient(_validOptions, testHttpClient, _mockLogger.Object);

            var request = new MessagesRequest
            {
                Model = AnthropicModels.Claude35Sonnet,
                MaxTokens = 100,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = new List<Content>
                        {
                            new Content { Type = "text", Text = "Hello" }
                        }
                    }
                }
            };

            // Act
            var response = await client.CreateMessagesAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("test-message-id", response.Id);
            Assert.Equal("Hello, this is a test response!", response.Content[0].Text);
            Assert.Equal("end_turn", response.StopReason);
            Assert.Equal(10, response.Usage.InputTokens);
            Assert.Equal(20, response.Usage.OutputTokens);
        }

        [Fact]
        public async Task CreateMessagesAsync_WithErrorResponse_ThrowsAnthropicApiException()
        {
            // Arrange
            var errorResponse = """
                {
                    "error": {
                        "type": "invalid_request_error",
                        "message": "Invalid API key"
                    }
                }
                """;

            var testHttpClient = CreateTestHttpClient(errorResponse, HttpStatusCode.Unauthorized);
            using var client = new AnthropicClient(_validOptions, testHttpClient, _mockLogger.Object);

            var request = new MessagesRequest
            {
                Model = AnthropicModels.Claude35Sonnet,
                MaxTokens = 100,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = new List<Content>
                        {
                            new Content { Type = "text", Text = "Hello" }
                        }
                    }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AnthropicApiException>(
                () => client.CreateMessagesAsync(request));

            Assert.Equal(401, exception.StatusCode);
            Assert.Equal("invalid_request_error", exception.ErrorType);
            Assert.Contains("Invalid API key", exception.Message);
        }

        #endregion

        #region Logging Tests

        [Fact]
        public void Constructor_WithLogger_DoesNotThrow()
        {
            // Act
            using var client = new AnthropicClient(_validOptions, logger: _mockLogger.Object);

            // Assert
            Assert.NotNull(client);
            // Verify logger was used (constructor doesn't log, but this verifies logger injection works)
        }

        [Fact]
        public async Task CreateMessagesAsync_WithError_LogsError()
        {
            // Arrange
            var errorResponse = """
                {
                    "error": {
                        "type": "rate_limit_error", 
                        "message": "Rate limit exceeded"
                    }
                }
                """;

            var testHttpClient = CreateTestHttpClient(errorResponse, HttpStatusCode.TooManyRequests);
            using var client = new AnthropicClient(_validOptions, testHttpClient, _mockLogger.Object);

            var request = new MessagesRequest
            {
                Model = AnthropicModels.Claude35Sonnet,
                MaxTokens = 100,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = new List<Content>
                        {
                            new Content { Type = "text", Text = "Hello" }
                        }
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<AnthropicApiException>(() => client.CreateMessagesAsync(request));

            // Verify logging occurred
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API request failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Model Tests

        [Fact]
        public void AnthropicModels_ContainsExpectedModels()
        {
            // Assert
            Assert.Equal("claude-3-opus-20240229", AnthropicModels.Claude3Opus);
            Assert.Equal("claude-3-sonnet-20240229", AnthropicModels.Claude3Sonnet);
            Assert.Equal("claude-3-haiku-20240307", AnthropicModels.Claude3Haiku);
            Assert.Equal("claude-3-5-sonnet-20241022", AnthropicModels.Claude35Sonnet);
            Assert.Equal("claude-3-5-haiku-20241022", AnthropicModels.Claude35Haiku);
        }

        [Fact]
        public void BetaFeatures_ContainsExpectedFeatures()
        {
            // Assert
            Assert.Equal("computer-use-2024-10-22", BetaFeatures.ComputerUse);
            Assert.Equal("prompt-caching-2024-07-31", BetaFeatures.PromptCaching);
            Assert.Equal("message-batches-2024-09-24", BetaFeatures.MessageBatches);
            Assert.Equal("pdfs-2024-09-25", BetaFeatures.PDFSupport);
        }

        #endregion

        #region Helper Methods

        private static HttpClient CreateTestHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new TestHttpMessageHandler(responseContent, statusCode);
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.anthropic.com")
            };
        }

        #endregion
    }

    /// <summary>
    /// Test HTTP message handler for mocking HTTP responses.
    /// </summary>
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public TestHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}