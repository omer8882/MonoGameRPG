using Xunit;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Npc;
using FluentAssertions;
using System.Collections.Generic;

namespace ANewWorld.Tests
{
    public class NpcAnimationBuilderTests
    {
        [Fact]
        public void BuildAnimationClips_Creates_Correct_Rectangle_For_Each_Frame()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 64,
                SpriteHeight = 64,
                AnimationClips = new Dictionary<string, AnimationClipData>
                {
                    ["idleDown"] = new AnimationClipData 
                    { 
                        Row = 0, 
                        Frames = [0, 1], 
                        FrameDuration = 0.2f 
                    }
                }
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            var key = new MovementAnimationKey(MovementAction.Idle, Facing.Down);
            clips.Should().ContainKey(key);
            
            var clip = clips[key];
            clip.Frames.Should().HaveCount(2);
            clip.Frames[0].Should().Be(new Rectangle(0, 0, 64, 64));   // frame 0, row 0
            clip.Frames[1].Should().Be(new Rectangle(64, 0, 64, 64));  // frame 1, row 0
            clip.FrameDuration.Should().Be(0.2f);
            clip.Loop.Should().BeTrue();
        }

        [Fact]
        public void BuildAnimationClips_Handles_Multiple_Rows()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 32,
                SpriteHeight = 32,
                AnimationClips = new Dictionary<string, AnimationClipData>
                {
                    ["idleDown"] = new AnimationClipData { Row = 0, Frames = [0], FrameDuration = 0.1f },
                    ["idleUp"] = new AnimationClipData { Row = 1, Frames = [0], FrameDuration = 0.1f },
                    ["idleLeft"] = new AnimationClipData { Row = 2, Frames = [0], FrameDuration = 0.1f },
                    ["idleRight"] = new AnimationClipData { Row = 3, Frames = [0], FrameDuration = 0.1f }
                }
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            clips.Should().HaveCount(4);
            
            clips[new MovementAnimationKey(MovementAction.Idle, Facing.Down)].Frames[0]
                .Should().Be(new Rectangle(0, 0, 32, 32));
            
            clips[new MovementAnimationKey(MovementAction.Idle, Facing.Up)].Frames[0]
                .Should().Be(new Rectangle(0, 32, 32, 32));
            
            clips[new MovementAnimationKey(MovementAction.Idle, Facing.Left)].Frames[0]
                .Should().Be(new Rectangle(0, 64, 32, 32));
            
            clips[new MovementAnimationKey(MovementAction.Idle, Facing.Right)].Frames[0]
                .Should().Be(new Rectangle(0, 96, 32, 32));
        }

        [Fact]
        public void BuildAnimationClips_Creates_All_Movement_States()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 64,
                SpriteHeight = 64,
                AnimationClips = new Dictionary<string, AnimationClipData>
                {
                    ["idleDown"] = new AnimationClipData { Row = 0, Frames = [0], FrameDuration = 0.2f },
                    ["idleUp"] = new AnimationClipData { Row = 1, Frames = [0], FrameDuration = 0.2f },
                    ["idleLeft"] = new AnimationClipData { Row = 2, Frames = [0], FrameDuration = 0.2f },
                    ["idleRight"] = new AnimationClipData { Row = 3, Frames = [0], FrameDuration = 0.2f },
                    ["walkDown"] = new AnimationClipData { Row = 0, Frames = [0, 1, 2, 3], FrameDuration = 0.15f },
                    ["walkUp"] = new AnimationClipData { Row = 1, Frames = [0, 1, 2, 3], FrameDuration = 0.15f },
                    ["walkLeft"] = new AnimationClipData { Row = 2, Frames = [0, 1, 2, 3], FrameDuration = 0.15f },
                    ["walkRight"] = new AnimationClipData { Row = 3, Frames = [0, 1, 2, 3], FrameDuration = 0.15f }
                }
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            clips.Should().HaveCount(8); // 4 idle + 4 walk
            
            // Verify all keys exist
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Idle, Facing.Down));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Idle, Facing.Up));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Idle, Facing.Left));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Idle, Facing.Right));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Walk, Facing.Down));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Walk, Facing.Up));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Walk, Facing.Left));
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Walk, Facing.Right));
            
            // Verify walk animations have 4 frames
            clips[new MovementAnimationKey(MovementAction.Walk, Facing.Down)].Frames.Should().HaveCount(4);
        }

        [Fact]
        public void BuildAnimationClips_Returns_Empty_Dictionary_When_No_Clips_Defined()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 64,
                SpriteHeight = 64,
                AnimationClips = null
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            clips.Should().BeEmpty();
        }

        [Fact]
        public void BuildAnimationClips_Ignores_Unknown_Clip_Names()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 64,
                SpriteHeight = 64,
                AnimationClips = new Dictionary<string, AnimationClipData>
                {
                    ["idleDown"] = new AnimationClipData { Row = 0, Frames = [0], FrameDuration = 0.2f },
                    ["unknownClip"] = new AnimationClipData { Row = 1, Frames = [0], FrameDuration = 0.2f }
                }
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            clips.Should().HaveCount(1); // only idleDown
            clips.Should().ContainKey(new MovementAnimationKey(MovementAction.Idle, Facing.Down));
        }

        [Fact]
        public void BuildAnimationClips_Uses_Custom_Sprite_Dimensions()
        {
            // Arrange
            var definition = new NpcDefinition
            {
                SpriteWidth = 48,
                SpriteHeight = 72,
                AnimationClips = new Dictionary<string, AnimationClipData>
                {
                    ["walkDown"] = new AnimationClipData { Row = 0, Frames = [0, 1], FrameDuration = 0.1f }
                }
            };

            // Act
            var clips = NpcAnimationBuilder.BuildAnimationClips(definition);

            // Assert
            var clip = clips[new MovementAnimationKey(MovementAction.Walk, Facing.Down)];
            clip.Frames[0].Should().Be(new Rectangle(0, 0, 48, 72));
            clip.Frames[1].Should().Be(new Rectangle(48, 0, 48, 72));
        }
    }
}
