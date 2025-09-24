using DefaultEcs;

namespace ANewWorld.Engine.Ecs
{
    public interface IEcsWorld : System.IDisposable
    {
        World InnerWorld { get; }
    }
}