using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Tilemap.Tmx;
using TiledSharp;
using System;

namespace ANewWorld.Engine.Tilemap
{
    public sealed class TileObjectSpawner
    {
        private readonly World _world;
        private readonly Dictionary<int, Rectangle> _gidToSourceRect;
        private readonly Dictionary<int, Texture2D> _gidToTexture;
        private readonly TmxRenderer? _tmxRenderer;

        public TileObjectSpawner(World world, Dictionary<int, Texture2D> gidToTexture, Dictionary<int, Rectangle> gidToSourceRect, TmxRenderer? tmxRenderer = null)
        {
            _world = world;
            _gidToTexture = gidToTexture;
            _gidToSourceRect = gidToSourceRect;
            _tmxRenderer = tmxRenderer;
        }

        // Existing MonoGame.Extended overload kept (unused now)
        public void SpawnObjects(TiledMap map) { /* no-op in TiledSharp mode */ }

        // TiledSharp overload
        public void SpawnObjects(TiledSharp.TmxMap map)
        {
            if (map == null) return;
            foreach (var group in map.ObjectGroups)
            {
                if (!group.Visible) continue;

                foreach (var obj in group.Objects)
                {
                    if (!obj.Visible) continue;

                    var e = _world.CreateEntity();

                    float x = (float)obj.X;
                    float y = (float)obj.Y;
                    int rawGid = obj.Tile?.Gid ?? 0;
                    int baseGid = rawGid & 0x1FFFFFFF;
                    int gid = baseGid;

                    if (_tmxRenderer != null && gid != 0)
                    {
                        gid = _tmxRenderer.ResolveCurrentGid(baseGid);
                    }

                    if (gid != 0 && _gidToSourceRect.TryGetValue(gid, out var orect))
                    {
                        y -= orect.Height; // convert bottom-left to top-left
                    }

                    var position = new Vector2(x, y);
                    e.Set(new Transform { Position = position, Rotation = 0f, Scale = Vector2.One });

                    e.Set(new MapObjectComponent { Id = obj.Id, Name = obj.Name, Type = obj.Type, Properties = obj.Properties, Gid = gid != 0 ? gid : null });

                    e.Set(new Name(obj.Name));

                    if(obj.Name.Contains("Fire"))
                    {
                        Console.WriteLine("");
                    }

                    if (gid != 0 && _gidToTexture.TryGetValue(gid, out var tex) && _gidToSourceRect.TryGetValue(gid, out var rect))
                    {
                        e.Set(new SpriteComponent
                        {
                            Texture = tex,
                            SourceRect = rect,
                            Color = Microsoft.Xna.Framework.Color.White,
                            Origin = Vector2.Zero
                        });

                        // If base gid is part of an animated sequence, tag the entity for animation updates
                        if (_tmxRenderer != null && _tmxRenderer.IsAnimated(baseGid))
                        {
                            e.Set(new ANewWorld.Engine.Components.AnimatedTileObject { BaseGid = baseGid, LastAppliedGid = gid });
                        }
                    }

                    e.Set(new Name(obj.Name));
                }
            }
        }
    }
}
