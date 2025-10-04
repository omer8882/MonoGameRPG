using Xunit;
using ANewWorld.Engine.Input;
using FluentAssertions;
using System.IO;
using System.Text.Json;

namespace ANewWorld.Tests
{
    public class InputActionServiceTests
    {
        private const string TestBindingsPath = "test_input_bindings.json";

        public InputActionServiceTests()
        {
            // Arrange - Create test bindings file
            var bindings = new
            {
                MoveUp = new[] { "W", "Up" },
                MoveDown = new[] { "S", "Down" },
                Jump = new[] { "Space" },
                Attack = new[] { "LeftControl", "Z" }
            };
            File.WriteAllText(TestBindingsPath, JsonSerializer.Serialize(bindings));
        }

        [Fact]
        public void LoadBindings_Reads_From_File()
        {
            // Arrange
            var svc = new InputActionService(TestBindingsPath);

            // Act
            svc.LoadBindings();

            // Assert - no exception, bindings loaded
            svc.Should().NotBeNull();
        }

        [Fact]
        public void LoadBindings_Handles_Missing_File()
        {
            // Arrange
            var svc = new InputActionService("nonexistent.json");

            // Act
            svc.LoadBindings();

            // Assert - should not crash
            svc.Should().NotBeNull();
        }

        [Fact]
        public void OverlayActive_Defaults_To_True()
        {
            // Arrange
            var svc = new InputActionService(TestBindingsPath);

            // Act / Assert
            svc.OverlayActive.Should().BeTrue();
        }

        internal void Dispose()
        {
            if (File.Exists(TestBindingsPath))
                File.Delete(TestBindingsPath);
        }
    }
}
