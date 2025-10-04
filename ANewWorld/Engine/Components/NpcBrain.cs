namespace ANewWorld.Engine.Components
{
    /// <summary>
    /// NPC behavior types
    /// </summary>
    public enum NpcBehaviorType
    {
        Idle,
        Patrol,
        Wander,
        FacePlayer,
        Interact
    }

    /// <summary>
    /// NPC AI brain state
    /// </summary>
    public struct NpcBrain
    {
        public NpcBehaviorType CurrentBehavior;
        public NpcBehaviorType DefaultBehavior;
        public NpcBehaviorType SavedBehavior; // for restoring after interaction
        public float StateTimer;

        public override string ToString() => CurrentBehavior.ToString();
    }
}
