using DefaultEcs;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ANewWorld.Engine.Systems
{
    public sealed class RenderSystem
    {
        private readonly World _world;
        private readonly SpriteBatch _spriteBatch;

        // Reused list to avoid allocations
        private readonly List<Item> _items = new(256);

        private struct Item
        {
            public float KeyY;
            public float KeyX;
            public Transform T;
            public SpriteComponent S;
        }

        public RenderSystem(World world, SpriteBatch spriteBatch)
        {
            _world = world;
            _spriteBatch = spriteBatch;
        }

        public void Update(float dt)
        {
            var set = _world.GetEntities().With<Transform>().With<SpriteComponent>().AsSet();

            _items.Clear();
            _items.Capacity = System.Math.Max(_items.Capacity, set.Count);

            foreach (var entity in set.GetEntities())
            {
                var t = entity.Get<Transform>();
                var s = entity.Get<SpriteComponent>();
                int srcH = s.SourceRect?.Height ?? s.Texture.Height;
                float bottomY = t.Position.Y + (srcH - s.Origin.Y) * t.Scale.Y - s.SortOffsetY;
                _items.Add(new Item { KeyY = bottomY, KeyX = t.Position.X, T = t, S = s });
            }

            _items.Sort(static (a, b) =>
            {
                int cmp = a.KeyY.CompareTo(b.KeyY);
                if (cmp != 0) return cmp;
                // tie-breaker by X to stabilize ordering
                cmp = a.KeyX.CompareTo(b.KeyX);
                if (cmp != 0) return cmp;
                // final fallback: keep order stable by hash
                return a.T.GetHashCode().CompareTo(b.T.GetHashCode());
            });

            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                _spriteBatch.Draw(it.S.Texture, it.T.Position, it.S.SourceRect, it.S.Color, it.T.Rotation, it.S.Origin, it.T.Scale, SpriteEffects.None, 0f);
            }
        }
    }
}