using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using System;

namespace ANewWorld.Engine.Systems
{
    /// <summary>
    /// Handles NPC interactions with player
    /// </summary>
    public sealed class NpcInteractionSystem : ISystem<float>, IDisposable
    {
        private readonly World _world;
        private readonly EntitySet _interactionEvents;
        private readonly EntitySet _npcs;
        private DialogueSystem? _dialogueSystem;
        
        public bool IsEnabled { get; set; } = true;
        
        public NpcInteractionSystem(World world)
        {
            _world = world;
            _interactionEvents = world.GetEntities()
                .With<InteractionStarted>()
                .AsSet();
            _npcs = world.GetEntities()
                .With<NpcTag>()
                .With<NpcBrain>()
                .AsSet();
        }
        
        public void SetDialogueSystem(DialogueSystem dialogueSystem)
        {
            _dialogueSystem = dialogueSystem;
        }
        
        public void Update(float dt)
        {
            if (!IsEnabled) return;
            
            // Handle interaction started
            foreach (ref readonly var eventEntity in _interactionEvents.GetEntities())
            {
                var interactionEvent = eventEntity.Get<InteractionStarted>();
                var target = interactionEvent.Target;
                
                if (!target.Has<NpcTag>()) continue;
                
                // Save current behavior and switch to Interact
                ref var brain = ref target.Get<NpcBrain>();
                brain.SavedBehavior = brain.CurrentBehavior;
                brain.CurrentBehavior = NpcBehaviorType.Interact;
                
                // Face the player
                if (target.Has<FacingDirection>() && target.Has<Transform>())
                {
                    var npcPos = target.Get<Transform>().Position;
                    var playerPos = GetPlayerPosition();
                    
                    if (playerPos.HasValue)
                    {
                        var direction = playerPos.Value - npcPos;
                        var facing = GetFacingFromDirection(direction);
                        target.Set(new FacingDirection { Value = facing });
                    }
                }
                
                target.Set(brain);
            }
            
            // Check if dialogue ended, restore behavior
            if (_dialogueSystem != null && !_dialogueSystem.IsActive)
            {
                foreach (ref readonly var npc in _npcs.GetEntities())
                {
                    ref var brain = ref npc.Get<NpcBrain>();
                    
                    if (brain.CurrentBehavior == NpcBehaviorType.Interact)
                    {
                        brain.CurrentBehavior = brain.SavedBehavior;
                        brain.StateTimer = 0;
                        npc.Set(brain);
                    }
                }
            }
        }
        
        private Vector2? GetPlayerPosition()
        {
            var players = _world.GetEntities().With<Transform>().With<Name>().AsSet();
            
            foreach (ref readonly var player in players.GetEntities())
            {
                if (player.Has<Name>() && player.Get<Name>().Value == "Player")
                {
                    return player.Get<Transform>().Position;
                }
            }
            
            return null;
        }
        
        private Facing GetFacingFromDirection(Vector2 direction)
        {
            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                return direction.X > 0 ? Facing.Right : Facing.Left;
            }
            else
            {
                return direction.Y > 0 ? Facing.Down : Facing.Up;
            }
        }
        
        public void Dispose()
        {
            _interactionEvents?.Dispose();
            _npcs?.Dispose();
        }
    }
}
