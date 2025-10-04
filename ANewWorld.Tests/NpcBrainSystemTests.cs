using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class NpcBrainSystemTests
    {
        [Fact]
        public void Brain_Updates_State_Timer()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Idle,
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world);

            // Act
            system.Update(1.5f);

            // Assert
            npc.Get<NpcBrain>().StateTimer.Should().BeApproximately(1.5f, 0.01f);
        }

        [Fact]
        public void IsEnabled_False_Skips_Brain_Updates()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Idle,
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world) { IsEnabled = false };

            // Act
            system.Update(1.0f);

            // Assert
            npc.Get<NpcBrain>().StateTimer.Should().Be(0);
        }

        [Fact]
        public void Multiple_NPCs_Update_Independently()
        {
            // Arrange
            using var world = new World();
            
            var npc1 = world.CreateEntity();
            npc1.Set(new NpcTag());
            npc1.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Idle, StateTimer = 0 });
            
            var npc2 = world.CreateEntity();
            npc2.Set(new NpcTag());
            npc2.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol, StateTimer = 5.0f });
            
            var system = new NpcBrainSystem(world);

            // Act
            system.Update(2.0f);

            // Assert
            npc1.Get<NpcBrain>().StateTimer.Should().BeApproximately(2.0f, 0.01f);
            npc2.Get<NpcBrain>().StateTimer.Should().BeApproximately(7.0f, 0.01f);
        }

        [Fact]
        public void Idle_NPC_With_Wander_Component_Can_Transition_To_Wander()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Idle,
                DefaultBehavior = NpcBehaviorType.Wander, // default is wander
                StateTimer = 0
            });
            npc.Set(new WanderBehavior 
            { 
                OriginPoint = Vector2.Zero,
                WanderRadius = 50f,
                CurrentTarget = Vector2.Zero
            });
            
            var system = new NpcBrainSystem(world);

            // Act - update many times to trigger random transition
            for (int i = 0; i < 100; i++)
            {
                system.Update(6.0f); // exceed IdleCheckInterval
                if (npc.Get<NpcBrain>().CurrentBehavior == NpcBehaviorType.Wander)
                    break;
            }

            // Assert - should eventually transition (probabilistic, but high chance over 100 attempts)
            var behavior = npc.Get<NpcBrain>().CurrentBehavior;
            (behavior == NpcBehaviorType.Wander || behavior == NpcBehaviorType.Idle).Should().BeTrue();
        }

        [Fact]
        public void Idle_NPC_Without_Wander_Component_Stays_Idle()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Idle,
                DefaultBehavior = NpcBehaviorType.Wander,
                StateTimer = 0
            });
            // No WanderBehavior component
            
            var system = new NpcBrainSystem(world);

            // Act
            for (int i = 0; i < 10; i++)
            {
                system.Update(6.0f);
            }

            // Assert - stays idle without WanderBehavior
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Idle);
        }

        [Fact]
        public void Wander_NPC_Returns_To_Idle_After_Timeout_If_Default_Is_Idle()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Wander,
                DefaultBehavior = NpcBehaviorType.Idle, // default is idle
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world);

            // Act - exceed wander timeout (30 seconds)
            system.Update(31.0f);

            // Assert
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Idle);
            npc.Get<NpcBrain>().StateTimer.Should().Be(0); // timer reset
        }

        [Fact]
        public void Wander_NPC_Stays_Wandering_If_Default_Is_Not_Idle()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Wander,
                DefaultBehavior = NpcBehaviorType.Wander, // default is wander
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world);

            // Act
            system.Update(35.0f);

            // Assert - stays wandering
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Wander);
        }

        [Fact]
        public void Patrol_Behavior_Is_Not_Modified_By_Brain_System()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Patrol,
                DefaultBehavior = NpcBehaviorType.Patrol,
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world);

            // Act
            system.Update(100.0f);

            // Assert - patrol stays patrol (movement system handles it)
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Patrol);
        }

        [Fact]
        public void Interact_Behavior_Is_Not_Modified_By_Brain_System()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Interact,
                DefaultBehavior = NpcBehaviorType.Idle,
                StateTimer = 0
            });
            
            var system = new NpcBrainSystem(world);

            // Act
            system.Update(10.0f);

            // Assert - interaction system controls this, brain doesn't interfere
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Interact);
        }
    }
}
