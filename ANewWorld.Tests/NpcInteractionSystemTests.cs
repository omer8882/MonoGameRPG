using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class NpcInteractionSystemTests
    {
        [Fact]
        public void InteractionStarted_Switches_NPC_To_Interact_Behavior()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Patrol,
                DefaultBehavior = NpcBehaviorType.Patrol,
                SavedBehavior = NpcBehaviorType.Patrol
            });
            npc.Set(new Transform { Position = new Vector2(100, 100) });
            npc.Set(new FacingDirection { Value = Facing.Down });
            
            var interactionEvent = world.CreateEntity();
            interactionEvent.Set(new InteractionStarted { Target = npc });
            
            var system = new NpcInteractionSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            var brain = npc.Get<NpcBrain>();
            brain.CurrentBehavior.Should().Be(NpcBehaviorType.Interact);
            brain.SavedBehavior.Should().Be(NpcBehaviorType.Patrol);
        }

        [Fact]
        public void NPC_Faces_Player_On_Interaction()
        {
            // Arrange
            using var world = new World();
            
            // Create player
            var player = world.CreateEntity();
            player.Set(new Transform { Position = new Vector2(100, 50) }); // above NPC
            player.Set(new Name("Player"));
            
            // Create NPC
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Idle });
            npc.Set(new Transform { Position = new Vector2(100, 100) });
            npc.Set(new FacingDirection { Value = Facing.Down });
            
            var interactionEvent = world.CreateEntity();
            interactionEvent.Set(new InteractionStarted { Target = npc });
            
            var system = new NpcInteractionSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<FacingDirection>().Value.Should().Be(Facing.Up); // facing player
        }

        [Fact]
        public void NPC_Restores_Behavior_After_Dialogue_Ends()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain 
            { 
                CurrentBehavior = NpcBehaviorType.Interact,
                DefaultBehavior = NpcBehaviorType.Wander,
                SavedBehavior = NpcBehaviorType.Wander
            });
            
            // Create a mock dialogue system
            var dialogueService = new ANewWorld.Engine.Dialogue.DialogueService();
            var inputService = new ANewWorld.Engine.Input.InputActionService("Content/input_bindings.json");
            var audioBus = new ANewWorld.Engine.Audio.AudioBus();
            var dialogueSystem = new DialogueSystem(world, dialogueService, inputService, audioBus);
            
            var system = new NpcInteractionSystem(world);
            system.SetDialogueSystem(dialogueSystem);

            // Act - dialogue is inactive
            system.Update(0.016f);

            // Assert - behavior restored
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Wander);
        }

        [Fact]
        public void IsEnabled_False_Skips_Interaction_Processing()
        {
            // Arrange
            using var world = new World();
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Patrol });
            
            var interactionEvent = world.CreateEntity();
            interactionEvent.Set(new InteractionStarted { Target = npc });
            
            var system = new NpcInteractionSystem(world) { IsEnabled = false };

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<NpcBrain>().CurrentBehavior.Should().Be(NpcBehaviorType.Patrol); // unchanged
        }

        [Fact]
        public void Non_NPC_Interaction_Is_Ignored()
        {
            // Arrange
            using var world = new World();
            var nonNpc = world.CreateEntity();
            nonNpc.Set(new Transform { Position = Vector2.Zero });
            // No NpcTag
            
            var interactionEvent = world.CreateEntity();
            interactionEvent.Set(new InteractionStarted { Target = nonNpc });
            
            var system = new NpcInteractionSystem(world);

            // Act
            system.Update(0.016f);

            // Assert - should not crash or throw
            system.Should().NotBeNull();
        }

        [Theory]
        [InlineData(150, 100, Facing.Right)]  // player to right
        [InlineData(50, 100, Facing.Left)]    // player to left
        [InlineData(100, 50, Facing.Up)]      // player above
        [InlineData(100, 150, Facing.Down)]   // player below
        public void NPC_Faces_Correct_Direction_Based_On_Player_Position(float playerX, float playerY, Facing expectedFacing)
        {
            // Arrange
            using var world = new World();
            
            var player = world.CreateEntity();
            player.Set(new Transform { Position = new Vector2(playerX, playerY) });
            player.Set(new Name("Player"));
            
            var npc = world.CreateEntity();
            npc.Set(new NpcTag());
            npc.Set(new NpcBrain { CurrentBehavior = NpcBehaviorType.Idle });
            npc.Set(new Transform { Position = new Vector2(100, 100) });
            npc.Set(new FacingDirection { Value = Facing.Down });
            
            var interactionEvent = world.CreateEntity();
            interactionEvent.Set(new InteractionStarted { Target = npc });
            
            var system = new NpcInteractionSystem(world);

            // Act
            system.Update(0.016f);

            // Assert
            npc.Get<FacingDirection>().Value.Should().Be(expectedFacing);
        }
    }
}
