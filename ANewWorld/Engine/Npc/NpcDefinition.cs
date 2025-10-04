using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Npc
{
    public class NpcDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SpriteSheet { get; set; } = string.Empty;
        public string DefaultBehavior { get; set; } = "Idle";
        public string? DialogueId { get; set; }
        public float InteractRadius { get; set; } = 32f;
        
        // Animation clips (future: move to external JSON)
        public Dictionary<string, AnimationClipData>? AnimationClips { get; set; }
        
        // Patrol-specific
        public Vector2[]? PatrolWaypoints { get; set; }
        public bool PatrolLoop { get; set; } = true;
        public float PatrolWaitTime { get; set; } = 2.0f;
        
        // Wander-specific
        public float WanderRadius { get; set; } = 50f;
        public float WanderWaitTime { get; set; } = 3.0f;
    }

    public class AnimationClipData
    {
        public int[] Frames { get; set; } = [];
        public float Duration { get; set; } = 0.1f;
    }

    public class NpcDefinitionData
    {
        public Dictionary<string, NpcDefinition> Npcs { get; set; } = new();
    }
}
