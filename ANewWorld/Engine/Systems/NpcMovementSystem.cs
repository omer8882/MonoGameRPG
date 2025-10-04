using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using System;

namespace ANewWorld.Engine.Systems
{
    /// <summary>
    /// Handles NPC movement based on their current behavior
    /// </summary>
    public sealed class NpcMovementSystem : ISystem<float>, IDisposable
    {
        private readonly World _world;
        private readonly EntitySet _npcs;
        
        public bool IsEnabled { get; set; } = true;
        
        private const float MoveSpeed = 50f; // pixels per second
        private const float WaypointReachedDistance = 5f;
        
        public NpcMovementSystem(World world)
        {
            _world = world;
            _npcs = world.GetEntities()
                .With<NpcTag>()
                .With<NpcBrain>()
                .With<Transform>()
                .With<Velocity>()
                .AsSet();
        }
        
        public void Update(float dt)
        {
            if (!IsEnabled) return;
            
            foreach (ref readonly var entity in _npcs.GetEntities())
            {
                ref var brain = ref entity.Get<NpcBrain>();
                ref var transform = ref entity.Get<Transform>();
                ref var velocity = ref entity.Get<Velocity>();
                
                switch (brain.CurrentBehavior)
                {
                    case NpcBehaviorType.Idle:
                    case NpcBehaviorType.FacePlayer:
                    case NpcBehaviorType.Interact:
                        velocity.Value = Vector2.Zero;
                        break;
                        
                    case NpcBehaviorType.Patrol:
                        if (entity.Has<PatrolPath>())
                        {
                            UpdatePatrol(in entity, ref transform, ref velocity, dt);
                        }
                        break;
                        
                    case NpcBehaviorType.Wander:
                        if (entity.Has<WanderBehavior>())
                        {
                            UpdateWander(in entity, ref transform, ref velocity, dt);
                        }
                        break;
                }
                
                entity.Set(velocity);
            }
        }
        
        private void UpdatePatrol(in Entity entity, ref Transform transform, ref Velocity velocity, float dt)
        {
            ref var patrol = ref entity.Get<PatrolPath>();
            
            if (patrol.Waypoints == null || patrol.Waypoints.Length == 0)
            {
                velocity.Value = Vector2.Zero;
                return;
            }
            
            // If waiting at waypoint
            if (patrol.WaitTimer > 0)
            {
                patrol.WaitTimer -= dt;
                velocity.Value = Vector2.Zero;
                entity.Set(patrol);
                return;
            }
            
            var target = patrol.Waypoints[patrol.CurrentWaypointIndex];
            var direction = target - transform.Position;
            var distance = direction.Length();
            
            if (distance < WaypointReachedDistance)
            {
                // Reached waypoint
                patrol.WaitTimer = patrol.WaitTimeAtWaypoint;
                
                // Move to next waypoint
                patrol.CurrentWaypointIndex++;
                
                if (patrol.CurrentWaypointIndex >= patrol.Waypoints.Length)
                {
                    if (patrol.Loop)
                    {
                        patrol.CurrentWaypointIndex = 0;
                    }
                    else
                    {
                        patrol.CurrentWaypointIndex = patrol.Waypoints.Length - 1;
                        velocity.Value = Vector2.Zero;
                    }
                }
                
                entity.Set(patrol);
            }
            else
            {
                // Move toward waypoint
                direction.Normalize();
                velocity.Value = direction * MoveSpeed * dt;
            }
        }
        
        private void UpdateWander(in Entity entity, ref Transform transform, ref Velocity velocity, float dt)
        {
            ref var wander = ref entity.Get<WanderBehavior>();
            
            // If waiting
            if (wander.WaitTimer > 0)
            {
                wander.WaitTimer -= dt;
                velocity.Value = Vector2.Zero;
                entity.Set(wander);
                return;
            }
            
            var direction = wander.CurrentTarget - transform.Position;
            var distance = direction.Length();
            
            if (distance < WaypointReachedDistance)
            {
                // Reached target, pick new one
                wander.WaitTimer = wander.WaitTime;
                
                var random = new Random();
                var angle = (float)(random.NextDouble() * Math.PI * 2);
                var radius = (float)(random.NextDouble() * wander.WanderRadius);
                wander.CurrentTarget = wander.OriginPoint + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
                
                entity.Set(wander);
            }
            else
            {
                // Move toward target
                direction.Normalize();
                velocity.Value = direction * MoveSpeed * dt;
            }
        }
        
        public void Dispose()
        {
            _npcs?.Dispose();
        }
    }
}
