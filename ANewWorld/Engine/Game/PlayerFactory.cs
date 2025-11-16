using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Components;
using System.Collections.Generic;
using ANewWorld.Engine.Items;

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

            var padding = new Point((64 - 15) / 2, 64 / 4 + 4);
            var sprite = new SpriteComponent
            {
                Texture = texture,
                SourceRect = sourceRect,
                Color = Color.White,
                Origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f),
                SortOffsetY = -padding.Y
            };
            entity.Set(sprite);

            var clips = new Dictionary<MovementAnimationKey, AnimationClip>();
            Rectangle Frame(int col, int row) => new Rectangle(col * sourceRect.Width, row * sourceRect.Height, sourceRect.Width, sourceRect.Height);

            clips[new(MovementAction.Idle, Facing.Down)] = new AnimationClip([Frame(0, 0)], 0.2f);
            clips[new(MovementAction.Walk, Facing.Down)] = new AnimationClip([Frame(0, 4), Frame(1, 4), Frame(2, 4), Frame(3, 4)], 0.12f );
            clips[new(MovementAction.Idle, Facing.Left)] = new AnimationClip ([Frame(0, 3)], 0.2f);
            clips[new(MovementAction.Walk, Facing.Left)] = new AnimationClip ([Frame(0, 7), Frame(1, 7), Frame(2, 7), Frame(3, 7)], 0.12f);
            clips[new(MovementAction.Idle, Facing.Right)] = new AnimationClip ([Frame(0, 2)], 0.2f);
            clips[new(MovementAction.Walk, Facing.Right)] = new AnimationClip ([Frame(0, 6), Frame(1, 6), Frame(2, 6), Frame(3, 6)], 0.12f);
            clips[new(MovementAction.Idle, Facing.Up)] = new AnimationClip ([Frame(0, 1)], 0.2f);
            clips[new(MovementAction.Walk, Facing.Up)] = new AnimationClip ([Frame(0, 5), Frame(1, 5), Frame(2, 5), Frame(3, 5)], 0.12f);

            entity.Set(new SpriteAnimatorComponent
            {
                Clips = clips,
                StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down),
                FrameIndex = 0,
                Timer = 0f
            });

            entity.Set(new Tag("Player"));
            entity.Set(new Name("Player"));
            entity.Set(new InventoryComponent());
            return entity;
        }
    }
}
