using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Npc;
using ANewWorld.Engine.Dialogue;
using ANewWorld.Engine.Extensions;
using System.Collections.Generic;
using System;

namespace ANewWorld.Engine.Systems
{
    /// <summary>
    /// Spawns and despawns NPCs based on map and game state
    /// </summary>
    public sealed class NpcSpawnerSystem
    {
        private readonly World _world;
        private readonly NpcService _npcService;
        private readonly DialogueService _dialogueService;
        private readonly ContentManager _content;
        private readonly HashSet<string> _spawnedNpcs = [];
        
        public NpcSpawnerSystem(World world, NpcService npcService, DialogueService dialogueService, ContentManager content)
        {
            _world = world;
            _npcService = npcService;
            _dialogueService = dialogueService;
            _content = content;
        }
        
        public void SpawnNpcsForMap(string mapId)
        {
            var spawnData = _npcService.GetSpawnRulesForMap(mapId);
            if (spawnData == null) return;
            
            foreach (var rule in spawnData.Npcs)
            {
                if (!ShouldSpawn(rule.Conditions))
                    continue;
                
                var definition = _npcService.GetDefinition(rule.NpcId);
                if (definition == null) continue;
                
                var uniqueKey = $"{mapId}_{rule.NpcId}_{rule.SpawnPoint.X}_{rule.SpawnPoint.Y}";
                if (_spawnedNpcs.Contains(uniqueKey))
                    continue; // already spawned
                
                SpawnNpc(definition, rule.SpawnPoint);
                _spawnedNpcs.Add(uniqueKey);
            }
        }
        
        private bool ShouldSpawn(NpcSpawnCondition conditions)
        {
            // Check required flags
            if (conditions.RequiredFlags != null)
            {
                foreach (var flag in conditions.RequiredFlags)
                {
                    if (!_dialogueService.Context.Flags.GetValueOrDefault(flag))
                        return false;
                }
            }
            
            // Check forbidden flags
            if (conditions.ForbiddenFlags != null)
            {
                foreach (var flag in conditions.ForbiddenFlags)
                {
                    if (_dialogueService.Context.Flags.GetValueOrDefault(flag))
                        return false;
                }
            }
            
            // TODO: Check time of day when time system is implemented
            // TODO: Check quest stage when quest system is implemented
            // TODO: Check player level when level system is implemented
            
            return true;
        }
        
        private void SpawnNpc(NpcDefinition def, Vector2 position)
        {
            var entity = _world.CreateEntity();
            
            // Core components
            entity.Set(new NpcTag());
            entity.Set(new NpcData 
            { 
                Id = def.Id, 
                DisplayName = def.DisplayName, 
                Description = def.Description 
            });
            entity.Set(new Transform { Position = position, Rotation = 0f, Scale = Vector2.One });
            entity.Set(new FacingDirection { Value = Facing.Down });
            entity.Set(new Velocity { Value = Vector2.Zero });
            entity.Set(new Name(def.DisplayName));
            
            // Brain
            var behavior = Enum.Parse<NpcBehaviorType>(def.DefaultBehavior, ignoreCase: true);
            entity.Set(new NpcBrain 
            { 
                CurrentBehavior = behavior, 
                DefaultBehavior = behavior,
                SavedBehavior = behavior,
                StateTimer = 0
            });
            
            // Behavior-specific components
            if (behavior == NpcBehaviorType.Patrol && def.PatrolWaypoints != null && def.PatrolWaypoints.Length > 0)
            {
                entity.Set(new PatrolPath
                {
                    Waypoints = def.PatrolWaypoints,
                    CurrentWaypointIndex = 0,
                    WaitTimeAtWaypoint = def.PatrolWaitTime,
                    WaitTimer = 0,
                    Loop = def.PatrolLoop
                });
            }
            
            if (behavior == NpcBehaviorType.Wander)
            {
                entity.Set(new WanderBehavior
                {
                    OriginPoint = position,
                    WanderRadius = def.WanderRadius,
                    CurrentTarget = position,
                    WaitTime = def.WanderWaitTime,
                    WaitTimer = def.WanderWaitTime // start by waiting
                });
            }
            
            // Interactable
            entity.Set(new Interactable 
            { 
                Enabled = true, 
                Radius = def.InteractRadius, 
                Prompt = "Talk" 
            });
            
            // Dialogue
            if (def.DialogueId.NotNullOrEmpty())
            {
                entity.Set(new DialogueComponent { DialogueId = def.DialogueId! });
            }
            
            // Sprite & Animation
            try
            {
                var texture = _content.Load<Texture2D>(def.SpriteSheet);
                entity.Set(new SpriteComponent 
                { 
                    Texture = texture, 
                    SourceRect = new Rectangle(0, 0, def.SpriteWidth, def.SpriteHeight), 
                    Origin = new Vector2(def.SpriteWidth / 2f, def.SpriteHeight / 2f), 
                    Color = Color.White 
                });
                
                // Build animation clips from definition
                var animationClips = NpcAnimationBuilder.BuildAnimationClips(def);
                entity.Set(new SpriteAnimatorComponent 
                { 
                    Clips = animationClips,
                    StateKey = new MovementAnimationKey(MovementAction.Idle, Facing.Down),
                    FrameIndex = 0,
                    Timer = 0
                });
            }
            catch
            {
                // If sprite loading fails, NPC still spawns but without visuals
            }
        }
        
        public void DespawnNpcsForMap(string mapId)
        {
            // Remove spawn keys for this map
            var toRemove = new List<string>();
            foreach (var key in _spawnedNpcs)
            {
                if (key.StartsWith(mapId + "_"))
                {
                    toRemove.Add(key);
                }
            }
            
            foreach (var key in toRemove)
            {
                _spawnedNpcs.Remove(key);
            }
            
            // Dispose NPC entities
            var npcs = _world.GetEntities().With<NpcTag>().AsSet();
            foreach (ref readonly var npc in npcs.GetEntities())
            {
                npc.Dispose();
            }
            npcs.Dispose();
        }
        
        public void RefreshSpawns(string mapId)
        {
            DespawnNpcsForMap(mapId);
            SpawnNpcsForMap(mapId);
        }
    }
}
