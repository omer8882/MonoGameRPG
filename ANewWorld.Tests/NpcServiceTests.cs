using Xunit;
using ANewWorld.Engine.Npc;
using FluentAssertions;
using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;

namespace ANewWorld.Tests
{
    public class NpcServiceTests : IDisposable
    {
        private readonly string _testRootPath;
        private const string TestNpcsPath = "Content/Data/npcs.json";
        private const string TestSpawnsPath = "Content/Data/npc_spawns.json";
        private readonly ContentManager _contentManager;
        private readonly string _basePath;

        public NpcServiceTests()
        {
            // Use unique directory per test instance to avoid parallel test conflicts
            _testRootPath = $"TestContent_NpcService_{Guid.NewGuid():N}";
            _basePath = Path.GetFullPath(_testRootPath);
            
            // Create test content directory and files (NpcService expects Content/Data subdirectory)
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(Path.Combine(_basePath, "Content"));
            Directory.CreateDirectory(Path.Combine(_basePath, "Content", "Data"));
            
            // Create test NPC definitions
            var npcs = new
            {
                Npcs = new Dictionary<string, object>
                {
                    { 
                        "village_elder", 
                        new 
                        { 
                            DisplayName = "Elder Thomas",
                            DefaultBehavior = "Idle",
                            DialogueId = "elder_greeting",
                            Speed = 50f
                        } 
                    },
                    {
                        "town_guard",
                        new
                        {
                            DisplayName = "Town Guard",
                            DefaultBehavior = "Patrol",
                            DialogueId = "npc_question",
                            Speed = 60f,
                            PatrolWaypoints = new[] 
                            { 
                                new { X = 100f, Y = 100f },
                                new { X = 200f, Y = 100f },
                                new { X = 200f, Y = 200f },
                                new { X = 100f, Y = 200f }
                            },
                            PatrolLoop = true
                        }
                    }
                }
            };
            
            var npcsPath = Path.Combine(_basePath, TestNpcsPath);
            File.WriteAllText(npcsPath, JsonSerializer.Serialize(npcs));

            // Create test spawn rules
            var spawns = new
            {
                Spawns = new Dictionary<string, object>
                {
                    {
                        "beginning_fields",
                        new
                        {
                            Npcs = new[]
                            {
                                new
                                {
                                    NpcId = "village_elder",
                                    SpawnPoint = new { X = 300f, Y = 300f },
                                    Conditions = new { } // empty conditions
                                }
                            }
                        }
                    }
                }
            };
            
            var spawnsPath = Path.Combine(_basePath, TestSpawnsPath);
            File.WriteAllText(spawnsPath, JsonSerializer.Serialize(spawns));

            // Create ContentManager with test root
            var serviceProvider = new GameServiceContainer();
            _contentManager = new ContentManager(serviceProvider, _basePath);
        }

        [Fact]
        public void Constructor_Loads_Definitions_And_Spawn_Rules()
        {
            // Arrange & Act
            var service = new NpcService(_contentManager);

            // Assert
            var elder = service.GetDefinition("village_elder");
            elder.Should().NotBeNull();
            elder!.DisplayName.Should().Be("Elder Thomas");
        }

        [Fact]
        public void GetDefinition_Returns_Null_For_Unknown_NPC()
        {
            // Arrange
            var service = new NpcService(_contentManager);

            // Act
            var result = service.GetDefinition("unknown_npc");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetSpawnRulesForMap_Returns_Null_For_Unknown_Map()
        {
            // Arrange
            var service = new NpcService(_contentManager);

            // Act
            var result = service.GetSpawnRulesForMap("unknown_map");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Loaded_NPC_Has_Correct_Properties()
        {
            // Arrange
            var service = new NpcService(_contentManager);

            // Act
            var guard = service.GetDefinition("town_guard");

            // Assert
            guard.Should().NotBeNull();
            guard!.DisplayName.Should().Be("Town Guard");
            guard.DefaultBehavior.Should().Be("Patrol");
            guard.DialogueId.Should().Be("npc_question");
            guard.PatrolWaypoints.Should().NotBeNull();
            guard.PatrolWaypoints!.Length.Should().Be(4);
            guard.PatrolLoop.Should().BeTrue();
        }

        [Fact]
        public void Loaded_Spawn_Rules_Have_Correct_Properties()
        {
            // Arrange
            var service = new NpcService(_contentManager);

            // Act
            var spawns = service.GetSpawnRulesForMap("beginning_fields");

            // Assert
            spawns.Should().NotBeNull();
            spawns!.MapId.Should().Be("beginning_fields");
            
            var elderSpawn = spawns.Npcs.Find(n => n.NpcId == "village_elder");
            elderSpawn.Should().NotBeNull();
            elderSpawn!.SpawnPoint.X.Should().Be(300);
            elderSpawn.SpawnPoint.Y.Should().Be(300);
        }

        [Fact]
        public void GetSpawnRulesForMap_Returns_Valid_Data()
        {
            // Arrange
            var service = new NpcService(_contentManager);

            // Act
            var spawns = service.GetSpawnRulesForMap("beginning_fields");

            // Assert
            spawns.Should().NotBeNull();
            spawns!.Npcs.Should().HaveCountGreaterThan(0);
        }

        public void Dispose()
        {
            _contentManager?.Dispose();
            
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
