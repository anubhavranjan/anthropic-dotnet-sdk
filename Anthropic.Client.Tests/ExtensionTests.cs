using System.Collections.Generic;
using Xunit;
using Anthropic.Client.Models;
using Anthropic.Client.Extensions;

namespace Anthropic.Client.Tests
{
    public class ExtensionTests
    {
        [Fact]
        public void AddText_AddsTextContentToMessage()
        {
            // Arrange
            var message = new Message { Role = "user", Content = new List<Content>() };

            // Act
            message.AddText("Hello world");

            // Assert
            Assert.Single(message.Content);
            Assert.Equal("text", message.Content[0].Type);
            Assert.Equal("Hello world", message.Content[0].Text);
        }

        [Fact]
        public void AddImage_AddsImageContentToMessage()
        {
            // Arrange
            var message = new Message { Role = "user", Content = new List<Content>() };

            // Act
            message.AddImage("base64data", "image/jpeg");

            // Assert
            Assert.Single(message.Content);
            Assert.Equal("image", message.Content[0].Type);
            Assert.Equal("base64", message.Content[0].Source!.Type);
            Assert.Equal("image/jpeg", message.Content[0].Source.MediaType);
            Assert.Equal("base64data", message.Content[0].Source.Data);
        }

        [Fact]
        public void CreateUserMessage_CreatesUserMessageWithText()
        {
            // Act
            var message = MessageExtensions.CreateUserMessage("Hello");

            // Assert
            Assert.Equal("user", message.Role);
            Assert.Single(message.Content);
            Assert.Equal("text", message.Content[0].Type);
            Assert.Equal("Hello", message.Content[0].Text);
        }

        [Fact]
        public void CreateAssistantMessage_CreatesAssistantMessageWithText()
        {
            // Act
            var message = MessageExtensions.CreateAssistantMessage("Hi there");

            // Assert
            Assert.Equal("assistant", message.Role);
            Assert.Single(message.Content);
            Assert.Equal("text", message.Content[0].Type);
            Assert.Equal("Hi there", message.Content[0].Text);
        }
    }
}