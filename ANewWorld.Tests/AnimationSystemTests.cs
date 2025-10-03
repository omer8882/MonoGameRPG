using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using System.Collections.Generic;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class AnimationSystemTests
    {
        [Fact]
        public void Animator_Advances_Frame()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var r1 = new Rectangle(10, 0, 10, 10);
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([r0, r1], 0.01f)
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.02f);

            // Assert
            e.Get<SpriteComponent>().SourceRect.Should().Be(r1);
        }

        [Fact]
        public void Animator_Loops_When_Clip_Loop_True()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var r1 = new Rectangle(10, 0, 10, 10);
            var clip = new AnimationClip([r0, r1], 0.01f) { Loop = true };
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = clip
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.03f); // should advance 3 ticks -> end wraps to start

            // Assert
            e.Get<SpriteComponent>().SourceRect.Should().Be(r1); // last applied frame may be r1 depending on remainder
            e.Get<SpriteAnimatorComponent>().FrameIndex.Should().BeInRange(0, 1);
        }

        [Fact]
        public void Animator_Clamps_When_Clip_Loop_False()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var r1 = new Rectangle(10, 0, 10, 10);
            var clip = new AnimationClip([r0, r1], 0.01f) { Loop = false };
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = clip
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.2f); // long enough to exceed clip length

            // Assert
            e.Get<SpriteAnimatorComponent>().FrameIndex.Should().Be(1);
            e.Get<SpriteComponent>().SourceRect.Should().Be(r1);
        }

        [Fact]
        public void Animator_Handles_Empty_Clip()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([], 0.01f)
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.02f);

            // Assert - should not crash, sprite rect unchanged
            e.Get<SpriteComponent>().SourceRect.Should().Be(r0);
        }

        [Fact]
        public void Animator_Handles_Missing_StateKey()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([r0], 0.01f)
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Walk, Facing.Up) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.02f);

            // Assert - should not crash
            e.Get<SpriteComponent>().SourceRect.Should().Be(r0);
        }

        [Fact]
        public void Animator_IsEnabled_False_Skips_Update()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var r1 = new Rectangle(10, 0, 10, 10);
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([r0, r1], 0.01f)
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world) { IsEnabled = false };

            // Act
            sys.Update(0.02f);

            // Assert
            e.Get<SpriteComponent>().SourceRect.Should().Be(r0);
        }

        [Fact]
        public void Animator_Single_Frame_Doesnt_Advance()
        {
            // Arrange
            using var world = new World();
            var e = world.CreateEntity();
            var r0 = new Rectangle(0, 0, 10, 10);
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>
            {
                [new MovementAnimationKey(MovementAction.Idle, Facing.Down)] = new AnimationClip([r0], 0.01f)
            };
            e.Set(new SpriteComponent { SourceRect = r0, Color = Color.White, Origin = Vector2.Zero });
            e.Set(new SpriteAnimatorComponent { Clips = clips, StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down) });
            var sys = new AnimationSystem(world);

            // Act
            sys.Update(0.5f);

            // Assert
            e.Get<SpriteAnimatorComponent>().FrameIndex.Should().Be(0);
            e.Get<SpriteComponent>().SourceRect.Should().Be(r0);
        }
    }
}
