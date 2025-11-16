using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ANewWorld.Engine.Extensions;
using ANewWorld.Engine.Items;
using DefaultEcs;
using FluentAssertions;
using Xunit;

namespace ANewWorld.Tests
{
    public sealed class ItemServiceTests : IDisposable
    {
        private readonly string _testRootPath;
        private readonly string _originalDirectory;

        public ItemServiceTests()
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            _testRootPath = $"TestContent_ItemService_{Guid.NewGuid():N}";
            var basePath = Path.GetFullPath(_testRootPath);

            Directory.CreateDirectory(basePath);
            var contentPath = Path.Combine(basePath, "Content", "Data", "Items", "Icons");
            Directory.CreateDirectory(contentPath);

            var items = new
            {
                Items = new Dictionary<string, object>
                {
                    {
                        "healing_potion",
                        new
                        {
                            DisplayName = "Healing Potion",
                            Description = "Restores health.",
                            Icon = "Data/Items/Icons/healing_potion.png",
                            MaxStack = 10,
                            Properties = new Dictionary<string, int>
                            {
                                { "heal", 25 }
                            }
                        }
                    },
                    {
                        "mana_tonic",
                        new
                        {
                            DisplayName = "Mana Tonic",
                            Description = "Restores mana.",
                            Icon = "Data/Items/Icons/mana_tonic.png",
                            MaxStack = 5,
                            Properties = new Dictionary<string, int>
                            {
                                { "mana", 15 }
                            }
                        }
                    }
                }
            };

            var jsonPath = Path.Combine(basePath, "Content", "Data", "Items", "items.json");
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
            File.WriteAllText(jsonPath, json);

            // create placeholder icons so relative paths resolve if needed later
            File.WriteAllBytes(Path.Combine(contentPath, "healing_potion.png"), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(contentPath, "mana_tonic.png"), Array.Empty<byte>());

            Directory.SetCurrentDirectory(basePath);
            ContentLoader.ContentManager = null;
        }

        [Fact]
        public void Constructor_Loads_Definitions()
        {
            var service = new ItemService();

            var potion = service.GetDefinition("healing_potion");
            potion.Should().NotBeNull();
            potion!.DisplayName.Should().Be("Healing Potion");
            potion.MaxStack.Should().Be(10);
        }

        [Fact]
        public void Add_And_Remove_Items_From_Inventory()
        {
            var service = new ItemService();
            var inventoryService = new InventoryService(service);
            using var world = new World();
            var entity = world.CreateEntity();

            inventoryService.AddItem(entity, "healing_potion", 6).Should().Be(6);
            inventoryService.AddItem(entity, "healing_potion", 10).Should().Be(4); // fills to max 10
            inventoryService.GetQuantity(entity, "healing_potion").Should().Be(10);

            inventoryService.RemoveItem(entity, "healing_potion", 3).Should().Be(3);
            inventoryService.ContainsAtLeast(entity, "healing_potion", 7).Should().BeTrue();
            inventoryService.RemoveItem(entity, "healing_potion", 20).Should().Be(7);
            inventoryService.GetQuantity(entity, "healing_potion").Should().Be(0);
        }

        [Fact]
        public void AddItem_Throws_For_Unknown_Id()
        {
            var service = new ItemService();
            var inventoryService = new InventoryService(service);
            using var world = new World();
            var entity = world.CreateEntity();

            Action act = () => inventoryService.AddItem(entity, "unknown", 1);
            act.Should().Throw<ArgumentException>();
        }

        public void Dispose()
        {
            try
            {
                Directory.SetCurrentDirectory(_originalDirectory);
            }
            catch
            {
            }

            try
            {
                var basePath = Path.GetFullPath(_testRootPath);
                if (Directory.Exists(basePath))
                {
                    Directory.Delete(basePath, recursive: true);
                }
            }
            catch
            {
            }
        }
    }
}
