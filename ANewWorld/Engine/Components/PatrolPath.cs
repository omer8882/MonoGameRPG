using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Components
{
    /// <summary>
    /// Patrol path for NPCs with Patrol behavior
    /// </summary>
    public struct PatrolPath
    {
        public Vector2[] Waypoints;
        public int CurrentWaypointIndex;
        public float WaitTimeAtWaypoint;
        public float WaitTimer;
        public bool Loop;
    }
}
