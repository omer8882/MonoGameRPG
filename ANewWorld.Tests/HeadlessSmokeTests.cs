using Xunit;
using DefaultEcs;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using Microsoft.Xna.Framework;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class HeadlessSmokeTests
    {
        [Fact]
        public void MovementUpdatesPositionOverTenTicks()
        {
            // Arrange (Setup)
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Transform { Position = new Vector2(0, 0), Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            var movement = new MovementSystem(world);

            // Act
            float initialX = e.Get<Transform>().Position.X;
            for (int i = 0; i < 10; i++)
            {
                movement.Update(1f / 60f);
            }
            float finalX = e.Get<Transform>().Position.X;

            // Assert
            finalX.Should().BeGreaterThan(initialX);
        }
    }
}