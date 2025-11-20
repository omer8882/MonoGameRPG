using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ANewWorld.Engine.Items
{
    public sealed class ItemIconCache : IDisposable
    {
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, Texture2D> _cache = new(StringComparer.OrdinalIgnoreCase);

        public ItemIconCache(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content;
            _graphicsDevice = graphicsDevice;
        }

        public Texture2D GetIcon(ItemDefinition definition)
        {
            var iconPath = definition.Icon;
            if (string.IsNullOrWhiteSpace(iconPath))
                throw new InvalidOperationException($"Item '{definition.Id}' is missing an icon path.");

            return GetIcon(iconPath);
        }

        public Texture2D GetIcon(string iconPath)
        {
            var relativePath = NormalizeRelativePath(iconPath);
            //if (_cache.TryGetValue(relativePath, out var cached))
            //    return cached;

            //var absolutePath = Path.Combine(AppContext.BaseDirectory, _content.RootDirectory, relativePath);
            //if (!File.Exists(absolutePath))
            //    throw new FileNotFoundException($"Icon '{relativePath}' was not found relative to content root.", absolutePath);

            //using var stream = File.OpenRead(absolutePath);
            //var texture = Texture2D.FromStream(_graphicsDevice, stream);
            var texture = _content.Load<Texture2D>(iconPath);
            texture.Name = relativePath;
            _cache[relativePath] = texture;
            return texture;
        }

        private static string NormalizeRelativePath(string raw)
        {
            var normalized = raw.Replace('\\', '/');
            if (normalized.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("Content/".Length);
            }

            return normalized.TrimStart('/');
        }

        public void Dispose()
        {
            foreach (var texture in _cache.Values)
            {
                texture.Dispose();
            }
            _cache.Clear();
        }
    }
}
