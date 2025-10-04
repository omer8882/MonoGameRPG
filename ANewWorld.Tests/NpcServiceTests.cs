using Xunit;
using ANewWorld.Engine.Npc;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace ANewWorld.Tests
{
    public class NpcServiceTests
    {
        [Fact]
        public void LoadDefinitions_Reads_From_File()
        {
            // Arrange
            var service = new NpcService();

            // Act
            service.LoadDefinitions("Content/npcs.json");

            // Assert
            var elder = service.GetDefinition("village_elder");
            elder.Should().NotBeNull();
            elder!.DisplayName.Should().Be("Elder Thomas");
        }

        [Fact]
        public void LoadDefinitions_Handles_Missing_File()
        {
            // Arrange
            var service = new NpcService();

            // Act
            service.LoadDefinitions("nonexistent.json");

            // Assert - should not crash
            service.GetDefinition("any").Should().BeNull();
        }

        [Fact]
        public void GetDefinition_Returns_Null_For_Unknown_NPC()
        {
            // Arrange
            var service = new NpcService();
            service.LoadDefinitions("Content/npcs.json");

            // Act
            var result = service.GetDefinition("unknown_npc");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void LoadSpawnRules_Reads_From_File()
        {
            // Arrange
            var service = new NpcService();

            // Act
            service.LoadSpawnRules("Content/npc_spawns.json");

            // Assert
            var spawns = service.GetSpawnRulesForMap("beginning_fields");
            spawns.Should().NotBeNull();
            spawns!.Npcs.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void LoadSpawnRules_Handles_Missing_File()
        {
            // Arrange
            var service = new NpcService();

            // Act
            service.LoadSpawnRules("nonexistent.json");

            // Assert - should not crash
            service.GetSpawnRulesForMap("any").Should().BeNull();
        }

        [Fact]
        public void GetSpawnRulesForMap_Returns_Null_For_Unknown_Map()
        {
            // Arrange
            var service = new NpcService();
            service.LoadSpawnRules("Content/npc_spawns.json");

            // Act
            var result = service.GetSpawnRulesForMap("unknown_map");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Loaded_NPC_Has_Correct_Properties()
        {
            // Arrange
            var service = new NpcService();
            service.LoadDefinitions("Content/npcs.json");

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
            var service = new NpcService();
            service.LoadSpawnRules("Content/npc_spawns.json");

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
    }
}
