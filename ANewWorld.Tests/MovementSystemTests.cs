using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class MovementSystemTests
    {
        [Fact]
        public void Movement_Updates_Position_Based_On_Velocity()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Transform { Position = new Vector2(10, 10), Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            var sys = new MovementSystem(world);

            // Act
            sys.Update(0.1f); // 0.1 sec * 100 speed * 1 velocity = 10 units

            // Assert
            var pos = e.Get<Transform>().Position;
            pos.X.Should().BeApproximately(20f, 0.01f);
            pos.Y.Should().BeApproximately(10f, 0.01f);
        }

        [Fact]
        public void Movement_Handles_Diagonal_Velocity()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Transform { Position = Vector2.Zero, Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = new Vector2(1, 1) });
            var sys = new MovementSystem(world);

            // Act
            sys.Update(0.1f);

            // Assert
            var pos = e.Get<Transform>().Position;
            pos.X.Should().BeGreaterThan(0);
            pos.Y.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Movement_With_Zero_Velocity_No_Change()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var initialPos = new Vector2(50, 50);
            e.Set(new Transform { Position = initialPos, Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = Vector2.Zero });
            var sys = new MovementSystem(world);

            // Act
            sys.Update(0.1f);

            // Assert
            e.Get<Transform>().Position.Should().Be(initialPos);
        }

        [Fact]
        public void Movement_Negative_Velocity_Moves_Backwards()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Transform { Position = new Vector2(100, 100), Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = new Vector2(-1, -1) });
            var sys = new MovementSystem(world);

            // Act
            sys.Update(0.1f);

            // Assert
            var pos = e.Get<Transform>().Position;
            pos.X.Should().BeLessThan(100);
            pos.Y.Should().BeLessThan(100);
        }

        [Fact]
        public void Movement_Multiple_Entities_Update_Independently()
        {
            // Arrange
            using var world = new World();
            var e1 = world.CreateEntity();
            e1.Set(new Transform { Position = Vector2.Zero, Rotation = 0f, Scale = Vector2.One });
            e1.Set(new Velocity { Value = new Vector2(1, 0) });

            var e2 = world.CreateEntity();
            e2.Set(new Transform { Position = new Vector2(100, 100), Rotation = 0f, Scale = Vector2.One });
            e2.Set(new Velocity { Value = new Vector2(0, 1) });

            var sys = new MovementSystem(world);

            // Act
            sys.Update(0.1f);

            // Assert
            var pos1 = e1.Get<Transform>().Position;
            var pos2 = e2.Get<Transform>().Position;
            pos1.X.Should().BeGreaterThan(0);
            pos1.Y.Should().Be(0);
            pos2.X.Should().Be(100);
            pos2.Y.Should().BeGreaterThan(100);
        }
    }
}
