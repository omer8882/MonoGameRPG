using Xunit;
using DefaultEcs;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using FluentAssertions;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;

namespace ANewWorld.Tests
{
    public class NpcInteractionSystemTests : IDisposable
    {
        private readonly string _testRootPath;
        private const string TestBindingsPath = "test_input_bindings.json";
        private const string TestDialoguesPath = "Content/Data/NPCs/dialogues.json";
        private readonly ContentManager _contentManager;
        private readonly string _basePath;
        private readonly string _originalDirectory;

        public NpcInteractionSystemTests()
        {
            // Save original directory to restore later
            _originalDirectory = Directory.GetCurrentDirectory();
            
            // Use unique directory per test instance to avoid parallel test conflicts
            _testRootPath = $"TestContent_NpcInteraction_{Guid.NewGuid():N}";
            _basePath = Path.GetFullPath(_testRootPath);
            
            // Create test content directory structure (DialogueService expects Data/NPCs subdirectory)
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(Path.Combine(_basePath, "Content", "Data"));
            Directory.CreateDirectory(Path.Combine(_basePath, "Content", "Data", "NPCs"));
            
            // Create test input bindings
            var bindings = new Dictionary<string, string[]>
            {
                { "Interact", ["E"] },
                { "Continue", [ "Space", "E" ] }
            };
            
            var bindingsPath = Path.Combine(_basePath, "Content", TestBindingsPath);
            File.WriteAllText(bindingsPath, JsonSerializer.Serialize(bindings));

            // Create test dialogues
            var dialogues = new
            {
                Dialogues = new Dictionary<string, object>
                {
                    { "test_dialogue", new { Nodes = Array.Empty<object>() } }
                }
            };
            
            var dialoguesPath = Path.Combine(_basePath, TestDialoguesPath);
            File.WriteAllText(dialoguesPath, JsonSerializer.Serialize(dialogues));

            // Create ContentManager with test root (for InputActionService)
            var serviceProvider = new GameServiceContainer();
            _contentManager = new ContentManager(serviceProvider, _basePath);
            
            // Set the current directory to the test root so DialogueService can find the files
            Directory.SetCurrentDirectory(_basePath);
        }

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
            
            // Create services (DialogueService no longer needs ContentManager)
            var dialogueService = new Engine.Dialogue.DialogueService();
            var inputService = new Engine.Input.InputActionService(TestBindingsPath);
            var audioBus = new Engine.Audio.AudioBus();
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

        public void Dispose()
        {
            _contentManager?.Dispose();
            
            // Restore original directory
            try
            {
                Directory.SetCurrentDirectory(_originalDirectory);
            }
            catch
            {
                // Ignore errors restoring directory
            }
            
            // Clean up test directory
            try
            {
                if (Directory.Exists(_basePath))
                    Directory.Delete(_basePath, recursive: true);
            }
            catch (IOException)
            {
                // Ignore cleanup errors
            }
        }
    }
}
