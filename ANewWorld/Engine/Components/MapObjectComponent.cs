using System.Collections.Generic;

namespace ANewWorld.Engine.Components
{
    public struct MapObjectComponent
    {
        public int Id;
        public string? Name;
        public string? Type;
        public Dictionary<string, string>? Properties;
        public int? Gid;
    }
}
