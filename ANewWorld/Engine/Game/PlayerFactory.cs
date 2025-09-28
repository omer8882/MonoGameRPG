using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Components;
using System.Collections.Generic;

namespace ANewWorld.Engine.Game
{
    public static class PlayerFactory
    {
        public static Entity CreatePlayer(World world, Texture2D texture, Rectangle sourceRect, Vector2 position, float rotation = 0f, Vector2? scale = null)
        {
            var entity = world.CreateEntity();
            entity.Set(new Transform
            {
                Position = position,
                Rotation = rotation,
                Scale = scale ?? Vector2.One
            });
            entity.Set(new Velocity { Value = Vector2.Zero });
            entity.Set(new FacingDirection { Value = Facing.Down });

            // Base sprite
            var padding = new Point((64 - 15) / 2, 64 / 4 + 4);
            var sprite = new SpriteComponent
            {
                Texture = texture,
                SourceRect = sourceRect,
                Color = Color.White,
                Origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f),
                SortOffsetY = padding.Y
            };
            entity.Set(sprite);

            // Animator: 4-dir, assume rows: 0=IdleDown,1=WalkDown,2=IdleLeft,3=WalkLeft,4=IdleRight,5=WalkRight,6=IdleUp,7=WalkUp (example)
            // If your sheet differs, adjust mapping accordingly.
            var clips = new Dictionary<string, AnimationClip>();
            Rectangle Frame(int col, int row) => new Rectangle(col * sourceRect.Width, row * sourceRect.Height, sourceRect.Width, sourceRect.Height);
            // Each clip 4 frames; tweak FrameDuration as needed
            clips["IdleDown"] = new AnimationClip { Frames = [Frame(0, 0)], FrameDuration = 0.2f, Loop = true };
            clips["WalkDown"] = new AnimationClip { Frames = [Frame(0, 4), Frame(1, 4), Frame(2, 4), Frame(3, 4)], FrameDuration = 0.12f, Loop = true };
            clips["IdleLeft"] = new AnimationClip { Frames = [Frame(0, 3)], FrameDuration = 0.2f, Loop = true };
            clips["WalkLeft"] = new AnimationClip { Frames = [Frame(0, 7), Frame(1, 7), Frame(2, 7), Frame(3, 7)], FrameDuration = 0.12f, Loop = true };
            clips["IdleRight"] = new AnimationClip { Frames = [Frame(0, 2)], FrameDuration = 0.2f, Loop = true };
            clips["WalkRight"] = new AnimationClip { Frames = [Frame(0, 6), Frame(1, 6), Frame(2, 6), Frame(3, 6)], FrameDuration = 0.12f, Loop = true };
            clips["IdleUp"] = new AnimationClip { Frames = [Frame(0, 1)], FrameDuration = 0.2f, Loop = true };
            clips["WalkUp"] = new AnimationClip { Frames = [Frame(0, 5), Frame(1, 5), Frame(2, 5), Frame(3, 5)], FrameDuration = 0.12f, Loop = true };

            entity.Set(new SpriteAnimatorComponent
            {
                Clips = clips,
                State = "IdleDown",
                FrameIndex = 0,
                Timer = 0f
            });

            entity.Set(new Tag("Player"));
            entity.Set(new Name("Player"));
            return entity;
        }
    }
}
