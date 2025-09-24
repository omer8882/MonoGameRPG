using DefaultEcs;

namespace ANewWorld.Engine.Ecs
{
    public sealed class DefaultEcsWorld : IEcsWorld
    {
        public World InnerWorld { get; }

        public DefaultEcsWorld()
        {
            InnerWorld = new World();
        }

        public void Dispose()
        {
            InnerWorld.Dispose();
        }
    }
}