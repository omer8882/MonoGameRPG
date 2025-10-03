using Xunit;
using ANewWorld.Engine.Rendering;
using Microsoft.Xna.Framework;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class CameraServiceTests
    {
        [Fact]
        public void Camera_Initializes_With_Correct_Properties()
        {
            // Arrange / Act
            var camera = new CameraService(800, 600, 1600, 1200, 2.0f);

            // Assert
            camera.VirtualWidth.Should().Be(800);
            camera.VirtualHeight.Should().Be(600);
            camera.WorldWidth.Should().Be(1600);
            camera.WorldHeight.Should().Be(1200);
            camera.Zoom.Should().Be(2.0f);
        }

        [Fact]
        public void Camera_Initial_Position_Is_Center()
        {
            // Arrange / Act
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);

            // Assert
            camera.Position.Should().Be(new Vector2(400, 300));
        }

        [Fact]
        public void Update_Changes_Position()
        {
            // Arrange
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);
            var newPos = new Vector2(100, 200);

            // Act
            camera.Update(newPos);

            // Assert
            camera.Position.Should().Be(newPos);
        }

        [Fact]
        public void UpdateViewport_Changes_Virtual_Size()
        {
            // Arrange
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);

            // Act
            camera.UpdateViewport(1024, 768);

            // Assert
            camera.VirtualWidth.Should().Be(1024);
            camera.VirtualHeight.Should().Be(768);
        }

        [Fact]
        public void GetViewMatrix_Returns_Valid_Matrix()
        {
            // Arrange
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);
            camera.Update(new Vector2(100, 100));

            // Act
            var matrix = camera.GetViewMatrix();

            // Assert
            matrix.Should().NotBe(Matrix.Identity);
        }

        [Fact]
        public void Zoom_Can_Be_Modified()
        {
            // Arrange
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);

            // Act
            camera.Zoom = 3.0f;

            // Assert
            camera.Zoom.Should().Be(3.0f);
        }

        [Fact]
        public void GetViewMatrix_Reflects_Zoom_Changes()
        {
            // Arrange
            var camera = new CameraService(800, 600, 1600, 1200, 1.0f);
            var matrix1 = camera.GetViewMatrix();

            // Act
            camera.Zoom = 2.0f;
            var matrix2 = camera.GetViewMatrix();

            // Assert
            matrix1.Should().NotBe(matrix2);
        }
    }
}
