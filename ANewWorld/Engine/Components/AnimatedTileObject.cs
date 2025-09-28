namespace ANewWorld.Engine.Components
{
    public struct AnimatedTileObject
    {
        public int BaseGid;        // masked gid without flip flags
        public int LastAppliedGid; // last frame gid applied to the sprite
    }
}
