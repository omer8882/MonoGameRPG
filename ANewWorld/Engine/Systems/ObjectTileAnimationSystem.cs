using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Tilemap.Tmx;
using System.Collections.Generic;
using System;

namespace ANewWorld.Engine.Systems
{
    public sealed class ObjectTileAnimationSystem : ISystem<float>, IDisposable
    {
        private readonly World _world;
        private readonly TmxRenderer _renderer;
        private readonly IReadOnlyDictionary<int, Texture2D> _gidToTexture;
        private readonly IReadOnlyDictionary<int, Rectangle> _gidToSourceRect;
        private readonly EntitySet _set;

        public bool IsEnabled { get; set; } = true;

        public ObjectTileAnimationSystem(World world, TmxRenderer renderer)
        {
            _world = world;
            _renderer = renderer;
            _gidToTexture = renderer.GidToTexture;
            _gidToSourceRect = renderer.GidToSourceRect;
            _set = world.GetEntities().With<AnimatedTileObject>().With<SpriteComponent>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;
            foreach (ref readonly var entity in _set.GetEntities())
            {
                ref var anim = ref entity.Get<AnimatedTileObject>();
                var curGid = _renderer.ResolveCurrentGid(anim.BaseGid);
                if (curGid == 0 || curGid == anim.LastAppliedGid)
                    continue;

                if (_gidToTexture.TryGetValue(curGid, out var tex) && _gidToSourceRect.TryGetValue(curGid, out var rect))
                {
                    ref var s = ref entity.Get<SpriteComponent>();
                    s.Texture = tex;
                    s.SourceRect = rect;
                    anim.LastAppliedGid = curGid;
                }
            }
        }

        public void Dispose()
        {
            _set?.Dispose();
        }
    }
}
