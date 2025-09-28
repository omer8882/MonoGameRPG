using TiledSharp;

namespace ANewWorld.Engine.Tilemap.Tmx
{
    public static class TmxLoader
    {
        public static TiledSharp.TmxMap LoadFromFile(string path)
        {
            return new TiledSharp.TmxMap(path);
        }
    }
}
