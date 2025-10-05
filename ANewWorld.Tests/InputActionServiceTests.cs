using Xunit;
using ANewWorld.Engine.Input;
using FluentAssertions;
using System.IO;
using System.Text.Json;
using NSubstitute;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Tests
{
    public class InputActionServiceTests : IDisposable
    {
        private const string TestRootPath = "TestContent";
        private const string TestBindingsPath = "test_input_bindings.json";
        private readonly ContentManager _contentManager;

        public InputActionServiceTests()
        {
            // Create test content directory and file
            Directory.CreateDirectory(TestRootPath);
            
            var bindings = new Dictionary<string, string[]>
            {
                { "MoveUp", new[] { "W", "Up" } },
                { "MoveDown", new[] { "S", "Down" } },
                { "Jump", new[] { "Space" } },
                { "Attack", new[] { "LeftControl", "Z" } }
            };
            
            var fullPath = Path.Combine(TestRootPath, TestBindingsPath);
            File.WriteAllText(fullPath, JsonSerializer.Serialize(bindings));

            // Create ContentManager with test root
            var serviceProvider = new GameServiceContainer();
            _contentManager = new ContentManager(serviceProvider, TestRootPath);
        }

        [Fact]
        public void LoadBindings_Reads_From_File()
        {
            // Arrange
            var svc = new InputActionService(_contentManager, TestBindingsPath);

            // Act
            svc.LoadBindings();

            // Assert - no exception, bindings loaded
            svc.Should().NotBeNull();
        }

        [Fact]
        public void LoadBindings_Handles_Missing_File()
        {
            // Arrange & Act
            var act = () => new InputActionService(_contentManager, "nonexistent.json");

            // Assert - should throw FileNotFoundException
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void OverlayActive_Defaults_To_True()
        {
            // Arrange
            var svc = new InputActionService(_contentManager, TestBindingsPath);

            // Act / Assert
            svc.OverlayActive.Should().BeTrue();
        }

        public void Dispose()
        {
            _contentManager?.Dispose();
            if (Directory.Exists(TestRootPath))
                Directory.Delete(TestRootPath, recursive: true);
        }
    }
}
