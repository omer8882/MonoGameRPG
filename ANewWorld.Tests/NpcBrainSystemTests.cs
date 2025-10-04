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
    }
}
