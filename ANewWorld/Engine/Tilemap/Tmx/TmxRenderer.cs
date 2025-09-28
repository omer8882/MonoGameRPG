using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TiledSharp;
using System.IO;
using System;

namespace ANewWorld.Engine.Tilemap.Tmx
{
    public sealed class TmxRenderer
    {
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<int, Texture2D> _gidToTexture;
        private readonly Dictionary<int, Rectangle> _gidToSourceRect;

        // Animated tiles: map any frame gid to its animation controller
        private readonly Dictionary<int, AnimatedTile> _animatedLookup = new();
        private readonly List<AnimatedTile> _animatedList = new();

        private sealed class AnimatedTile
        {
            public int[] Frames = Array.Empty<int>();
            public int[] DurationsMs = Array.Empty<int>();
            public int Index;
            public float TimerMs;
            public int CurrentGid => Frames.Length > 0 ? Frames[Index] : 0;
        }

        public TiledSharp.TmxMap Map { get; }

        public IReadOnlyDictionary<int, Texture2D> GidToTexture => _gidToTexture;
        public IReadOnlyDictionary<int, Rectangle> GidToSourceRect => _gidToSourceRect;

        public TmxRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, TiledSharp.TmxMap map)
        {
            _graphics = graphicsDevice;
            _spriteBatch = spriteBatch;
            Map = map;
            _gidToTexture = new Dictionary<int, Texture2D>();
            _gidToSourceRect = new Dictionary<int, Rectangle>();
        }

        private static string? TryResolveAssetName(string source)
        {
            var assetName = source.Replace("\\", "/").Replace("C:\\Users\\omer8\\Omer\\Dev\\Gaming\\A New World\\ANewWorld\\ANewWorld\\Content\\Maps\\The Fan-tasy Tileset (Free)\\Tiled\\Tilemaps\\..\\Tilesets\\".Replace("\\", "/"), "")
                                .Replace("../..", "Maps/The Fan-tasy Tileset (Free)").Replace("\\", "/");
            assetName = Path.ChangeExtension(assetName, null);
            return assetName;
        }

        private Texture2D? LoadTextureBySource(ContentManager content, string source)
        {
            Texture2D tex = null;
            var assetName = TryResolveAssetName(source);
            if (!string.IsNullOrEmpty(assetName))
            {
                try { tex = content.Load<Texture2D>(assetName!); } catch { tex = null; }
            }
            if (tex == null)
            {
                try { tex = content.Load<Texture2D>(Path.GetFileNameWithoutExtension(source)); } catch { tex = null; }
            }
            return tex;
        }

        public void BuildAtlas(ContentManager content)
        {
            _animatedLookup.Clear();
            _animatedList.Clear();

            foreach (var ts in Map.Tilesets)
            {
                var image = ts.Image;
                if (image != null && !string.IsNullOrEmpty(image.Source))
                {
                    // Spritesheet tileset
                    var tex = LoadTextureBySource(content, image.Source);
                    if (tex != null)
                    {
                        int w = ts.TileWidth;
                        int h = ts.TileHeight;
                        int cols = tex.Width / w;
                        int rows = tex.Height / h;
                        int tileCount = cols * rows;

                        int firstGid = ts.FirstGid;
                        for (int i = 0; i < tileCount; i++)
                        {
                            int gid = firstGid + i;
                            int x = i % cols;
                            int y = i / cols;
                            var rect = new Rectangle(x * w, y * h, w, h);
                            _gidToTexture[gid] = tex;
                            _gidToSourceRect[gid] = rect;
                        }
                    }
                }
                else
                {
                    // Collection of images tileset: each tile has its own image
                    if (ts.Tiles != null)
                    {
                        foreach (var kvp in ts.Tiles)
                        {
                            int localId = kvp.Key;
                            var tile = kvp.Value;
                            var tImg = tile.Image;
                            if (tImg == null || string.IsNullOrEmpty(tImg.Source))
                                continue;
                            var tex = LoadTextureBySource(content, tImg.Source);
                            if (tex == null) continue;

                            int gid = ts.FirstGid + localId;
                            var rect = new Rectangle(0, 0, tex.Width, tex.Height);
                            _gidToTexture[gid] = tex;
                            _gidToSourceRect[gid] = rect;
                        }
                    }
                }

                // Build animations for this tileset
                if (ts.Tiles != null)
                {
                    foreach (var kvp in ts.Tiles)
                    {
                        var tile = kvp.Value;
                        if (tile.AnimationFrames == null || tile.AnimationFrames.Count == 0)
                            continue;

                        var frames = new int[tile.AnimationFrames.Count];
                        var durs = new int[tile.AnimationFrames.Count];
                        for (int i = 0; i < tile.AnimationFrames.Count; i++)
                        {
                            var f = tile.AnimationFrames[i];
                            int tileLocalId = f.Id;
                            frames[i] = ts.FirstGid + tileLocalId;
                            durs[i] = f.Duration;
                        }

                        var anim = new AnimatedTile
                        {
                            Frames = frames,
                            DurationsMs = durs,
                            Index = 0,
                            TimerMs = 0
                        };
                        _animatedList.Add(anim);
                        // Map all frame gids and the base tile gid to this anim for easy lookup
                        foreach (var g in frames)
                            _animatedLookup[g] = anim;
                        _animatedLookup[ts.FirstGid + kvp.Key] = anim;
                    }
                }
            }
        }

        public void Update(float dtSeconds)
        {
            float dtMs = dtSeconds * 1000f;
            foreach (var anim in _animatedList)
            {
                if (anim.Frames.Length == 0) continue;
                anim.TimerMs += dtMs;
                // Advance frames while exceeding current duration (supports long dt)
                while (anim.TimerMs >= anim.DurationsMs[anim.Index])
                {
                    anim.TimerMs -= anim.DurationsMs[anim.Index];
                    anim.Index = (anim.Index + 1) % anim.Frames.Length;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix? transform = null)
        {
            const uint FLIP_H = 0x80000000;
            const uint FLIP_V = 0x40000000;
            const uint FLIP_D = 0x20000000; // not fully handled
            const uint GID_MASK = 0x1FFFFFFF;

            foreach (var layer in Map.Layers)
            {
                // respect visibility (default true)
                bool visible = true;
                var visProp = layer.GetType().GetProperty("Visible");
                if (visProp != null)
                {
                    var v = visProp.GetValue(layer);
                    if (v is bool vb) visible = vb;
                }
                if (!visible) continue;

                for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        int index = y * Map.Width + x;
                        if (index < 0 || index >= layer.Tiles.Count) continue;
                        uint raw = (uint)layer.Tiles[index].Gid;
                        if (raw == 0) continue;
                        bool flipH = (raw & FLIP_H) != 0;
                        bool flipV = (raw & FLIP_V) != 0;
                        bool flipD = (raw & FLIP_D) != 0;
                        int gid = (int)(raw & GID_MASK);

                        // Animated tiles: remap gid to current frame
                        if (_animatedLookup.TryGetValue(gid, out var anim))
                        {
                            gid = anim.CurrentGid;
                        }

                        if (!_gidToTexture.TryGetValue(gid, out var tex)) continue;
                        var src = _gidToSourceRect[gid];
                        var pos = new Vector2(x * Map.TileWidth, y * Map.TileHeight);

                        var effects = SpriteEffects.None;
                        if (flipH) effects |= SpriteEffects.FlipHorizontally;
                        if (flipV) effects |= SpriteEffects.FlipVertically;

                        spriteBatch.Draw(tex, pos, src, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
                    }
                }
            }
        }

        public int ResolveCurrentGid(int gid)
        {
            if (_animatedLookup.TryGetValue(gid, out var anim))
                return anim.CurrentGid;
            return gid;
        }

        public bool IsAnimated(int gid)
        {
            return _animatedLookup.ContainsKey(gid);
        }
    }
}
