using TiledSharp;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Tilemap
{
    public sealed class CollisionGridService
    {
        private const string DefaultCollisionLayerName = "Collision";
        private bool _enabled = false;
        private bool[,] _collisionGrid = default!;
        private readonly List<RectangleF> _rects = new();
        private readonly List<Vector2[]> _polygons = new();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public CollisionGridService(TmxMap map, string collisionLayerName = DefaultCollisionLayerName)
        {
            if (map == null) return;

            // Tile layer collisions
            TmxLayer? layer = null;
            foreach (var l in map.Layers)
            {
                if (l.Name == collisionLayerName)
                {
                    layer = l;
                    break;
                }
            }

            Width = map.Width;
            Height = map.Height;
            TileWidth = map.TileWidth;
            TileHeight = map.TileHeight;

            if (layer != null)
            {
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
                _enabled = true;
            }

            // Object-layer rectangles/polygons with same name
            foreach (var og in map.ObjectGroups)
            {
                if (og.Name != collisionLayerName) continue;
                foreach (var o in og.Objects)
                {
                    // Skip tile objects (if Tile property exists and set)
                    var tileProp = o.GetType().GetProperty("Tile");
                    var tileVal = tileProp?.GetValue(o);

                    // Rectangles (no tile, with width/height)
                    if (tileVal == null && o.Width > 0 && o.Height > 0)
                    {
                        _rects.Add(new RectangleF((float)o.X, (float)o.Y, (float)o.Width, (float)o.Height));
                        _enabled = true;
                        continue;
                    }

                    // Polygons/Polylines via reflection to access points
                    var polyProp = o.GetType().GetProperty("Points") ?? o.GetType().GetProperty("Polygon") ?? o.GetType().GetProperty("PolylinePoints");
                    var polyVal = polyProp?.GetValue(o);
                    if (polyVal is System.Collections.IEnumerable enumerable)
                    {
                        var pts = new List<Vector2>();
                        foreach (var p in enumerable)
                        {
                            var pxProp = p.GetType().GetProperty("X");
                            var pyProp = p.GetType().GetProperty("Y");
                            if (pxProp != null && pyProp != null)
                            {
                                float px = 0, py = 0;
                                var vx = pxProp.GetValue(p);
                                var vy = pyProp.GetValue(p);
                                if (vx != null) float.TryParse(vx.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out px);
                                if (vy != null) float.TryParse(vy.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out py);
                                pts.Add(new Vector2((float)o.X + px, (float)o.Y + py));
                            }
                        }
                        if (pts.Count >= 3)
                        {
                            _polygons.Add(pts.ToArray());
                            _enabled = true;
                        }
                    }
                }
            }
        }

        public bool IsBlocked(float worldX, float worldY)
        {
            if (!_enabled) return false;

            // Tile grid
            if (_collisionGrid != null)
            {
                int tx = (int)(worldX / TileWidth);
                int ty = (int)(worldY / TileHeight);
                if (tx < 0 || ty < 0 || tx >= Width || ty >= Height)
                    return true; // treat out-of-bounds as blocked
                if (_collisionGrid[tx, ty])
                    return true;
            }

            // Rectangles
            for (int i = 0; i < _rects.Count; i++)
            {
                if (_rects[i].Contains(worldX, worldY))
                    return true;
            }

            // Polygons (point-in-polygon)
            for (int i = 0; i < _polygons.Count; i++)
            {
                if (PointInPolygon(_polygons[i], new Vector2(worldX, worldY)))
                    return true;
            }

            return false;
        }

        private readonly struct RectangleF
        {
            public readonly float X;
            public readonly float Y;
            public readonly float Width;
            public readonly float Height;
            public float Right => X + Width;
            public float Bottom => Y + Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(float px, float py) => px >= X && px <= Right && py >= Y && py <= Bottom;
        }

        private static bool PointInPolygon(Vector2[] polygon, Vector2 p)
        {
            // Early out: treat points on an edge as outside (so movement along edges is allowed)
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[j];
                var b = polygon[i];
                // Check if point lies exactly on segment a-b
                if (IsPointOnSegment(a, b, p))
                    return false; // on edge -> outside per tests
            }

            // Ray-casting algorithm (Jordan curve theorem)
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                bool intersects = ((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                                  (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X);
                if (intersects)
                    inside = !inside;
            }
            return inside;
        }

        private static bool IsPointOnSegment(in Vector2 a, in Vector2 b, in Vector2 p)
        {
            const float eps = 1e-5f;
            // Check collinearity via cross product ~ 0
            var ap = p - a;
            var ab = b - a;
            float cross = ap.X * ab.Y - ap.Y * ab.X;
            if (System.MathF.Abs(cross) > eps)
                return false;
            // Check within bounding box
            float dot = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);
            if (dot < -eps)
                return false;
            float ab2 = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
            if (dot - ab2 > eps)
                return false;
            return true;
        }
    }
}
