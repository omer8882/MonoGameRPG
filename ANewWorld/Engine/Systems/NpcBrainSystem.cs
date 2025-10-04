using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;
using System;

namespace ANewWorld.Engine.Systems
{
    /// <summary>
    /// Manages NPC behavior state machine and AI decision-making
    /// </summary>
    public sealed class NpcBrainSystem : ISystem<float>, IDisposable
    {
        private readonly World _world;
        private readonly EntitySet _npcs;
        
        public bool IsEnabled { get; set; } = true;
        
        // Config: idle NPCs can randomly start wandering after this time
        private const float IdleToWanderChance = 0.1f; // 10% chance per check
        private const float IdleCheckInterval = 5.0f;  // check every 5 seconds
        
        public NpcBrainSystem(World world)
        {
            _world = world;
            _npcs = world.GetEntities()
                .With<NpcTag>()
                .With<NpcBrain>()
                .AsSet();
        }
        
        public void Update(float dt)
        {
            if (!IsEnabled) return;
            
            foreach (ref readonly var entity in _npcs.GetEntities())
            {
                ref var brain = ref entity.Get<NpcBrain>();
                
                // Update state timer
                brain.StateTimer += dt;
                
                // Handle state-specific logic
                switch (brain.CurrentBehavior)
                {
                    case NpcBehaviorType.Idle:
                        UpdateIdleBehavior(ref brain, in entity, dt);
                        break;
                        
                    case NpcBehaviorType.Patrol:
                        // Patrol movement handled by NpcMovementSystem
                        // Could add: check for obstacles, detect player, alert other guards
                        break;
                        
                    case NpcBehaviorType.Wander:
                        UpdateWanderBehavior(ref brain, in entity, dt);
                        break;
                        
                    case NpcBehaviorType.FacePlayer:
                        // Facing handled by NpcInteractionSystem
                        break;
                        
                    case NpcBehaviorType.Interact:
                        // Waiting for dialogue to end
                        // NpcInteractionSystem will restore behavior
                        break;
                }
                
                entity.Set(brain);
            }
        }
        
        private void UpdateIdleBehavior(ref NpcBrain brain, in Entity entity, float dt)
        {
            // Example: Idle NPCs can randomly start wandering
            // (Only if they have WanderBehavior component and default is not Idle)
            if (brain.DefaultBehavior != NpcBehaviorType.Idle && 
                entity.Has<WanderBehavior>() && 
                brain.StateTimer > IdleCheckInterval)
            {
                var random = new Random();
                if (random.NextDouble() < IdleToWanderChance)
                {
                    brain.CurrentBehavior = NpcBehaviorType.Wander;
                    brain.StateTimer = 0;
                }
                else
                {
                    brain.StateTimer = 0; // reset check timer
                }
            }
        }
        
        private void UpdateWanderBehavior(ref NpcBrain brain, in Entity entity, float dt)
        {
            // Example: Wandering NPCs can return to idle after some time
            // (Only if their default behavior is Idle)
            if (brain.DefaultBehavior == NpcBehaviorType.Idle && 
                brain.StateTimer > 30.0f) // wander for 30 seconds, then idle
            {
                brain.CurrentBehavior = NpcBehaviorType.Idle;
                brain.StateTimer = 0;
            }
        }
        
        public void Dispose()
        {
            _npcs?.Dispose();
        }
    }
}
