using TiledSharp;

namespace ANewWorld.Engine.Tilemap
{
    public sealed class CollisionGridService
    {
        private const string DefaultCollisionLayerName = "Collision";
        private bool _enabled = false;
        private bool[,] _collisionGrid = default!;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public CollisionGridService(TmxMap map, string collisionLayerName = DefaultCollisionLayerName)
        {
            if (map == null) return;

            // Find layer by name (case-sensitive by default)
            TmxLayer? layer = null;
            foreach (var l in map.Layers)
            {
                if (l.Name == collisionLayerName)
                {
                    layer = l;
                    break;
                }
            }
            if (layer == null) return;

            _enabled = true;
            Width = map.Width;
            Height = map.Height;
            TileWidth = map.TileWidth;
            TileHeight = map.TileHeight;
            _collisionGrid = new bool[Width, Height];

            const uint GID_MASK = 0x1FFFFFFF;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (index >= 0 && index < layer.Tiles.Count)
                    {
                        uint raw = (uint)layer.Tiles[index].Gid;
                        uint gid = raw & GID_MASK;
                        _collisionGrid[x, y] = gid != 0; // non-zero means blocked
                    }
                    else
                    {
                        _collisionGrid[x, y] = false;
                    }
                }
            }
        }

        public bool IsBlocked(float worldX, float worldY)
        {
            if (!_enabled) return false;

            int tx = (int)(worldX / TileWidth);
            int ty = (int)(worldY / TileHeight);
            if (tx < 0 || ty < 0 || tx >= Width || ty >= Height)
                return true; // treat out-of-bounds as blocked
            return _collisionGrid[tx, ty];
        }
    }
}
