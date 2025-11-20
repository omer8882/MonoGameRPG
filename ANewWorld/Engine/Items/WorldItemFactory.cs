using System;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Extensions;
using DefaultEcs;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Items
{
    public sealed class WorldItemFactory : System.IDisposable
    {
        private readonly World _world;
        private readonly ItemService _itemService;
        private readonly ItemIconCache _iconCache;

        public WorldItemFactory(World world, ItemService itemService, ItemIconCache iconCache)
        {
            _world = world;
            _itemService = itemService;
            _iconCache = iconCache;
        }

        public Entity SpawnWorldItem(
            string itemId,
            int quantity,
            Vector2 position,
            Vector2? initialImpulse = null,
            float pickupRadius = 36f,
            float drag = 4f)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

            var definition = _itemService.GetDefinition(itemId) ??
                throw new ArgumentException($"Unknown item id '{itemId}'.", nameof(itemId));

            var texture = _iconCache.GetIcon(definition);
            var entity = _world.CreateEntity();

            entity.Set(new Transform
            {
                Position = position,
                Rotation = 0f,
                Scale = Vector2.One
            });

            entity.Set(new SpriteComponent
            {
                Texture = texture,
                SourceRect = null,
                Color = Color.White,
                Origin = new Vector2(texture.Width / 2f, texture.Height - 4f),
                SortOffsetY = -4f
            });

            entity.Set(new Name($"{definition.DisplayName} Item"));

            var maxStack = Math.Max(1, definition.MaxStack);
            var clampedQuantity = Math.Clamp(quantity, 1, maxStack);

            var worldItem = new WorldItemComponent
            {
                ItemId = definition.Id,
                DisplayName = definition.DisplayName,
                Quantity = clampedQuantity,
                PickupRadius = pickupRadius
            };
            entity.Set(worldItem);

            entity.Set(new Interactable
            {
                Enabled = true,
                Prompt = BuildPrompt(worldItem.DisplayName, worldItem.Quantity),
                Radius = pickupRadius
            });

            if (initialImpulse.HasValue)
            {
                entity.Set(new DroppedItemPhysics
                {
                    Velocity = initialImpulse.Value,
                    Drag = drag,
                    MinimumSpeed = 5f
                });
            }

            return entity;
        }

        public static string BuildPrompt(string displayName, int quantity) =>
            quantity > 1 ? $"Pick up {displayName} x{quantity}" : $"Pick up {displayName}";

        public void Dispose()
        {
            // Intentionally left blank; icon cache handles texture lifetime.
        }
    }
}
