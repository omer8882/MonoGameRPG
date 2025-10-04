using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class NpcMovementSystemTests
    {
        [Fact]
        public void Idle_NPC_Has_Zero_Velocity()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Idle });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = new Vector2(5, 5) }); // some initial velocity
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
        }

        [Fact]
        public void Patrol_NPC_Moves_Toward_First_Waypoint()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0), new Vector2(100, 100) },
                CurrentWaypointIndex = 0,
                WaitTimeAtWaypoint = 1.0f,
                WaitTimer = 0,
                Loop = true
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            var velocity = npc.Get<Velocity>().Value;
            velocity.Should().NotBe(Vector2.Zero);
            velocity.X.Should().BeGreaterThan(0); // moving right
        }

        [Fact]
        public void Patrol_NPC_Waits_At_Waypoint()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = new Vector2(100, 0) }); // at waypoint
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0), new Vector2(100, 100) },
                CurrentWaypointIndex = 0,
                WaitTimeAtWaypoint = 2.0f,
                WaitTimer = 1.5f, // currently waiting
                Loop = true
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.5f);

            // Assert
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
            npc.Get<PatrolPath>().WaitTimer.Should().BeApproximately(1.0f, 0.01f);
        }

        [Fact]
        public void Patrol_NPC_Advances_To_Next_Waypoint_When_Reached()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = new Vector2(99, 0) }); // very close to waypoint
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0), new Vector2(100, 100) },
                CurrentWaypointIndex = 0,
                WaitTimeAtWaypoint = 1.0f,
                WaitTimer = 0,
                Loop = true
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            var patrol = npc.Get<PatrolPath>();
            patrol.CurrentWaypointIndex.Should().Be(1); // advanced to next waypoint
            patrol.WaitTimer.Should().BeApproximately(1.0f, 0.01f); // started waiting
        }

        [Fact]
        public void Patrol_NPC_Loops_Back_To_First_Waypoint()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = new Vector2(100, 99) }); // at last waypoint
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0), new Vector2(100, 100) },
                CurrentWaypointIndex = 1, // at last waypoint
                WaitTimeAtWaypoint = 1.0f,
                WaitTimer = 0,
                Loop = true
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<PatrolPath>().CurrentWaypointIndex.Should().Be(0); // looped back
        }

        [Fact]
        public void Patrol_NPC_Stops_At_Last_Waypoint_When_Not_Looping()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = new Vector2(100, 99) }); // at last waypoint
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0), new Vector2(100, 100) },
                CurrentWaypointIndex = 1, // at last waypoint
                WaitTimeAtWaypoint = 1.0f,
                WaitTimer = 0,
                Loop = false
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            var patrol = npc.Get<PatrolPath>();
            patrol.CurrentWaypointIndex.Should().Be(1); // stayed at last
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
        }

        [Fact]
        public void Wander_NPC_Moves_Toward_Target()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Wander });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new WanderBehavior 
            { 
                OriginPoint = Vector2.Zero,
                WanderRadius = 50f,
                CurrentTarget = new Vector2(30, 30),
                WaitTime = 2.0f,
                WaitTimer = 0
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<Velocity>().Value.Should().NotBe(Vector2.Zero);
        }

        [Fact]
        public void Wander_NPC_Waits_Before_Picking_New_Target()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Wander });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new WanderBehavior 
            { 
                OriginPoint = Vector2.Zero,
                WanderRadius = 50f,
                CurrentTarget = Vector2.Zero,
                WaitTime = 2.0f,
                WaitTimer = 1.0f // currently waiting
            });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.5f);

            // Assert
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
            npc.Get<WanderBehavior>().WaitTimer.Should().BeApproximately(0.5f, 0.01f);
        }

        [Fact]
        public void Interact_Behavior_Stops_Movement()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Interact });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = new Vector2(10, 10) });
            
            var system = new NpcMovementSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
        }

        [Fact]
        public void IsEnabled_False_Skips_Movement_Updates()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            npc.Set(new Transform { Position = Vector2.Zero });
            npc.Set(new Velocity { Value = Vector2.Zero });
            npc.Set(new PatrolPath 
            { 
                Waypoints = new[] { new Vector2(100, 0) },
                CurrentWaypointIndex = 0,
                WaitTimer = 0,
                Loop = true
            });
            
            var system = new NpcMovementSystem(world) { IsEnabled = false };

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<Velocity>().Value.Should().Be(Vector2.Zero);
        }
    }
}
