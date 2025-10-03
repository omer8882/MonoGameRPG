using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using System.Collections.Generic;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class AnimationStateSystemTests
    {
        [Fact]
        public void StateKey_Transitions_To_Walk_When_Moving()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f),
                [new MovementAnimationKey(MovementAction.Walk, Facing.Down)] = new AnimationClip([new Rectangle(10, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            e.Set(new FacingDirection { Value = Facing.Down });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationStateSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert
            e.Get<SpriteAnimatorComponent>().StateKey.Should().Be(new MovementAnimationKey(MovementAction.Walk, Facing.Down));
        }

        [Fact]
        public void StateKey_Transitions_To_Idle_When_Stopped()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Right)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f),
                [new MovementAnimationKey(MovementAction.Walk, Facing.Right)] = new AnimationClip([new Rectangle(10, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = Vector2.Zero });
            e.Set(new FacingDirection { Value = Facing.Right });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Walk, Facing.Right) });
            var sys = new AnimationStateSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert
            e.Get<SpriteAnimatorComponent>().StateKey.Should().Be(new MovementAnimationKey(MovementAction.Idle, Facing.Right));
        }

        [Fact]
        public void StateKey_Resets_FrameIndex_And_Timer_On_Change()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Up)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f),
                [new MovementAnimationKey(MovementAction.Walk, Facing.Up)] = new AnimationClip([new Rectangle(10, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = new Vector2(0, -1) });
            e.Set(new FacingDirection { Value = Facing.Up });
            var animator = new SpriteAnimatorComponent 
            { 
                Clips = clips, 
                StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Up),
                FrameIndex = 5,
                Timer = 0.8f
            };
            e.Set(animator);
            var sys = new AnimationStateSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert
            var result = e.Get<SpriteAnimatorComponent>();
            result.StateKey.Should().Be(new MovementAnimationKey(MovementAction.Walk, Facing.Up));
            result.FrameIndex.Should().Be(0);
            result.Timer.Should().Be(0f);
        }

        [Fact]
        public void StateKey_Updates_With_Facing_Direction_Change()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Walk, Facing.Left)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f),
                [new MovementAnimationKey(MovementAction.Walk, Facing.Right)] = new AnimationClip([new Rectangle(10, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            e.Set(new FacingDirection { Value = Facing.Left });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Walk, Facing.Left) });
            var sys = new AnimationStateSystem(world);

            // Act - change facing
            e.Set(new FacingDirection { Value = Facing.Right });
            sys.Update(0.016f);

            // Assert
            e.Get<SpriteAnimatorComponent>().StateKey.Should().Be(new MovementAnimationKey(MovementAction.Walk, Facing.Right));
        }

        [Fact]
        public void StateKey_Unchanged_When_Already_Correct()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Walk, Facing.Down)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = new Vector2(0, 1) });
            e.Set(new FacingDirection { Value = Facing.Down });
            var animator = new SpriteAnimatorComponent 
            { 
                Clips = clips, 
                StateKey = new MovementAnimationKey(MovementAction.Walk, Facing.Down),
                FrameIndex = 3,
                Timer = 0.5f
            };
            e.Set(animator);
            var sys = new AnimationStateSystem(world);

            // Act
            sys.Update(0.016f);

            // Assert - frame and timer should be preserved
            var result = e.Get<SpriteAnimatorComponent>();
            result.StateKey.Should().Be(new MovementAnimationKey(MovementAction.Walk, Facing.Down));
            result.FrameIndex.Should().Be(3);
            result.Timer.Should().Be(0.5f);
        }

        [Fact]
        public void IsEnabled_False_Skips_State_Updates()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([new Rectangle(0, 0, 10, 10)], 0.1f)
            };
            e.Set(new Velocity { Value = new Vector2(1, 0) });
            e.Set(new FacingDirection { Value = Facing.Down });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationStateSystem(world) { IsEnabled = false };

            // Act
            sys.Update(0.016f);

            // Assert
            e.Get<SpriteAnimatorComponent>().StateKey.Should().Be(new MovementAnimationKey(MovementAction.Idle, Facing.Down));
        }
    }
}
