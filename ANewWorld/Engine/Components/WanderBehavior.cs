using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Components
{
    /// <summary>
    /// Wander behavior for NPCs
    /// </summary>
    public struct WanderBehavior
    {
        public Vector2 OriginPoint;
        public float WanderRadius;
        public Vector2 CurrentTarget;
        public float WaitTime;
        public float WaitTimer;
    }
}
