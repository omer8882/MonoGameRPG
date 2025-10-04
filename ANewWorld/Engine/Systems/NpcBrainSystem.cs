using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;
using System;

namespace ANewWorld.Engine.Systems
{
    /// <summary>
    /// Manages NPC behavior state machine
    /// </summary>
    public sealed class NpcBrainSystem : ISystem<float>, IDisposable
    {
        private readonly World _world;
        private readonly EntitySet _npcs;
        
        public bool IsEnabled { get; set; } = true;
        
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
                        // Could trigger random wander after X seconds
                        break;
                        
                    case NpcBehaviorType.Patrol:
                        // Patrol logic handled by NpcMovementSystem
                        break;
                        
                    case NpcBehaviorType.Wander:
                        // Wander logic handled by NpcMovementSystem
                        break;
                        
                    case NpcBehaviorType.FacePlayer:
                        // Facing handled by NpcInteractionSystem
                        break;
                        
                    case NpcBehaviorType.Interact:
                        // Waiting for dialogue to end
                        break;
                }
                
                entity.Set(brain);
            }
        }
        
        public void Dispose()
        {
            _npcs?.Dispose();
        }
    }
}
