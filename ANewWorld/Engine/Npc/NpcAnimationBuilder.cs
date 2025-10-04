using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;

namespace ANewWorld.Engine.Npc
{
    /// <summary>
    /// Helper to build animation clips from NPC definitions
    /// </summary>
    public static class NpcAnimationBuilder
    {
        public static Dictionary<MovementAnimationKey, AnimationClip> BuildAnimationClips(
            NpcDefinition definition)
        {
            var clips = new Dictionary<MovementAnimationKey, AnimationClip>();
            
            if (definition.AnimationClips == null)
                return clips;
            
            int spriteWidth = definition.SpriteWidth;
            int spriteHeight = definition.SpriteHeight;
            
            // Map animation clip names to keys
            var clipMappings = new Dictionary<string, MovementAnimationKey>
            {
                ["idleDown"] = new MovementAnimationKey(MovementAction.Idle, Facing.Down),
                ["idleUp"] = new MovementAnimationKey(MovementAction.Idle, Facing.Up),
                ["idleLeft"] = new MovementAnimationKey(MovementAction.Idle, Facing.Left),
                ["idleRight"] = new MovementAnimationKey(MovementAction.Idle, Facing.Right),
                ["walkDown"] = new MovementAnimationKey(MovementAction.Walk, Facing.Down),
                ["walkUp"] = new MovementAnimationKey(MovementAction.Walk, Facing.Up),
                ["walkLeft"] = new MovementAnimationKey(MovementAction.Walk, Facing.Left),
                ["walkRight"] = new MovementAnimationKey(MovementAction.Walk, Facing.Right)
            };
            
            foreach (var (clipName, clipData) in definition.AnimationClips)
            {
                if (!clipMappings.TryGetValue(clipName, out var key))
                    continue;
                
                var frames = new List<Rectangle>();
                foreach (var frameIndex in clipData.Frames)
                {
                    var rect = new Rectangle(
                        frameIndex * spriteWidth,
                        clipData.Row * spriteHeight,
                        spriteWidth,
                        spriteHeight
                    );
                    frames.Add(rect);
                }
                
                if (frames.Count > 0)
                {
                    clips[key] = new AnimationClip(frames, clipData.FrameDuration)
                    {
                        Loop = true
                    };
                }
            }
            
            return clips;
        }
    }
}
