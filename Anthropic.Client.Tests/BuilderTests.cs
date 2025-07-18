using System;
using System.Linq;
using Xunit;
using Anthropic.Client.Builders;
using Anthropic.Client.Models;

namespace Anthropic.Client.Tests
{
    public class BuilderTests
    {
        [Fact]
        public void MessagesRequestBuilder_BuildsValidRequest()
        {
            // Act
            var request = MessagesRequestBuilder.Create()
                .WithModel(AnthropicModels.Claude35Sonnet)
                .WithMaxTokens(150)
                .WithTemperature(0.7)
                .WithSystem("You are a helpful assistant")
                .AddUserMessage("Hello")
                .Build();

            // Assert
            Assert.Equal(AnthropicModels.Claude35Sonnet, request.Model);
            Assert.Equal(150, request.MaxTokens);
            Assert.Equal(0.7, request.Temperature);
            Assert.Equal("You are a helpful assistant", request.System);
            Assert.Single(request.Messages);
            Assert.Equal("user", request.Messages.First().Role);
            Assert.Equal("Hello", request.Messages.First().Content.First().Text);
        }

        [Fact]
        public void MessagesRequestBuilder_WithEmptyMessages_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = MessagesRequestBuilder.Create()
                .WithModel(AnthropicModels.Claude35Sonnet);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void MessagesRequestBuilder_WithZeroMaxTokens_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = MessagesRequestBuilder.Create()
                .WithMaxTokens(0)
                .AddUserMessage("Hello");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }
    }
}