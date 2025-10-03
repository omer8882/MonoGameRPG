using Xunit;
using ANewWorld.Engine.Tilemap;
using Microsoft.Xna.Framework;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class CollisionGridServiceTests
    {
        [Fact]
        public void PointInPolygon_BasicTriangle()
        {
            // Arrange
            var poly = new[]
            {
                new Vector2(0,0),
                new Vector2(10,0),
                new Vector2(0,10)
            };
            var method = typeof(CollisionGridService).GetMethod("PointInPolygon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var inside = (bool)method!.Invoke(null, [poly, new Vector2(1, 1)])!;
            var outside = (bool)method!.Invoke(null, [poly, new Vector2(20, 20)])!;
            var onEdge = (bool)method!.Invoke(null, [poly, new Vector2(5, 0)])!;

            // Assert
            inside.Should().BeTrue();
            outside.Should().BeFalse();
            onEdge.Should().BeFalse(); // current implementation treats edge as outside
        }
    }
}
