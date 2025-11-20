using ANewWorld.Engine.Components;
using ANewWorld.Engine.Extensions;
using ANewWorld.Engine.Items;
using ANewWorld.Engine.Rendering;
using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ANewWorld.Engine.Systems
{
    public sealed class RenderSystem
    {
        private readonly World _world;
        private readonly SpriteBatch _spriteBatch;
        public CameraService? Camera { get; set; }

        // Reused list to avoid allocations
        private readonly List<Item> _items = new(256);

        private struct Item
        {
            public float KeyY;
            public float KeyX;
            public Transform T;
            public SpriteComponent S;
        }

        public int LastVisibleCount { get; private set; }
        public int LastCulledCount { get; private set; }

        public RenderSystem(World world, SpriteBatch spriteBatch)
        {
            _world = world;
            _spriteBatch = spriteBatch;
        }

        public void Draw(float dt)
        {
            var set = _world.GetEntities().With<Transform>().With<SpriteComponent>().AsSet();

            _items.Clear();
            _items.Capacity = System.Math.Max(_items.Capacity, set.Count);
            LastVisibleCount = 0;
            LastCulledCount = 0;

            // Compute view rect in world pixels if camera present
            RectangleF? view = null;
            if (Camera != null)
            {
                float viewW = Camera.VirtualWidth / Camera.Zoom;
                float viewH = Camera.VirtualHeight / Camera.Zoom;
                float left = Camera.Position.X - viewW / 2f;
                float top = Camera.Position.Y - viewH / 2f;
                view = new RectangleF(left, top, viewW, viewH);
            }

            foreach (var entity in set.GetEntities())
            {
                var t = entity.Get<Transform>();
                var s = entity.Get<SpriteComponent>();

                if(entity.Has<WorldItemComponent>())
                {

                }

                // Frustum culling (AABB) if view exists
                bool inView = true;
                if (view.HasValue)
                {
                    int srcW = s.SourceRect?.Width ?? s.Texture.Width;
                    int srcH = s.SourceRect?.Height ?? s.Texture.Height;
                    var scaledW = srcW * t.Scale.X;
                    var scaledH = srcH * t.Scale.Y;
                    var topLeft = new Vector2(t.Position.X - s.Origin.X * t.Scale.X, t.Position.Y - s.Origin.Y * t.Scale.Y);
                    // small padding to avoid pop at edges
                    const float pad = 8f;
                    var aabb = new RectangleF(topLeft.X - pad, topLeft.Y - pad, scaledW + pad * 2, scaledH + pad * 2);
                    inView = Intersects(view.Value, aabb);
                }

                if (!inView)
                {
                    LastCulledCount++;
                    continue;
                }

                int srcH2 = s.SourceRect?.Height ?? s.Texture.Height;
                float bottomY = t.Position.Y + (srcH2 - s.Origin.Y) * t.Scale.Y + s.SortOffsetY;
                _items.Add(new Item { KeyY = bottomY, KeyX = t.Position.X, T = t, S = s });
                LastVisibleCount++;
            }

            _items.Sort(static (a, b) =>
            {
                int cmp = a.KeyY.CompareTo(b.KeyY);
                if (cmp != 0) return cmp;
                cmp = a.KeyX.CompareTo(b.KeyX);
                if (cmp != 0) return cmp;
                return a.T.GetHashCode().CompareTo(b.T.GetHashCode());
            });

            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                _spriteBatch.Draw(it.S.Texture, it.T.Position, it.S.SourceRect, it.S.Color, it.T.Rotation, it.S.Origin, it.T.Scale, SpriteEffects.None, 0f);
            }
        }

        private static bool Intersects(in RectangleF a, in RectangleF b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }

        private readonly struct RectangleF
        {
            public readonly float Left;
            public readonly float Top;
            public readonly float Width;
            public readonly float Height;
            public float Right => Left + Width;
            public float Bottom => Top + Height;
            public RectangleF(float l, float t, float w, float h) { Left = l; Top = t; Width = w; Height = h; }
        }
    }
}