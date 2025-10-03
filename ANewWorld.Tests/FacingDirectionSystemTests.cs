using Xunit;
using DefaultEcs;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using Microsoft.Xna.Framework;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class FacingDirectionSystemTests
    {
        [Fact]
        public void Facing_Updates_From_Velocity_Cardinals()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            e.Set(new FacingDirection { Value = Facing.Down });
            var sys = new FacingDirectionSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert
            e.Get<FacingDirection>().Value.Should().Be(Facing.Right);

            // Act
            e.Set(new Velocity { Value = new Vector2(-1, 0) });
            sys.Update(0.016f);

            // Assert
            e.Get<FacingDirection>().Value.Should().Be(Facing.Left);

            // Act
            e.Set(new Velocity { Value = new Vector2(0, 1) });
            sys.Update(0.016f);

            // Assert
            e.Get<FacingDirection>().Value.Should().Be(Facing.Down);

            // Act
            e.Set(new Velocity { Value = new Vector2(0, -1) });
            sys.Update(0.016f);

            // Assert
            e.Get<FacingDirection>().Value.Should().Be(Facing.Up);
        }

        [Fact]
        public void Facing_Idle_Retains_Last_Facing()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            e.Set(new FacingDirection { Value = Facing.Left });
            var sys = new FacingDirectionSystem(world);

            // Act
            sys.Update(0.016f); // now facing Right
            e.Set(new Velocity { Value = Vector2.Zero });
            sys.Update(0.016f);

            // Assert
            e.Get<FacingDirection>().Value.Should().Be(Facing.Right);
        }

        [Fact]
        public void Facing_Diagonal_Biases_By_Axis_Magnitude()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Velocity { Value = new Vector2(2, 1) });
            e.Set(new FacingDirection { Value = Facing.Up });
            var sys = new FacingDirectionSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert (X magnitude bigger -> Right)
            e.Get<FacingDirection>().Value.Should().Be(Facing.Right);

            // Act
            e.Set(new Velocity { Value = new Vector2(1, -3) });
            sys.Update(0.016f);

            // Assert (Y magnitude bigger and negative -> Up)
            e.Get<FacingDirection>().Value.Should().Be(Facing.Up);
        }
    }
}
