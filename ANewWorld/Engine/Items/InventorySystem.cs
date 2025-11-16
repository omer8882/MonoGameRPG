using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using DefaultEcs.System;

namespace ANewWorld.Engine.Items
{
    public sealed class InventorySystem : ISystem<float>, IDisposable
    {
        private readonly ItemService _itemService;
        private readonly EntitySet _inventories;

        public bool IsEnabled { get; set; } = true;

        public InventorySystem(World world, ItemService itemService)
        {
            _itemService = itemService;
            _inventories = world.GetEntities().With<InventoryComponent>().AsSet();
        }

        public void Update(float state)
        {
            if (!IsEnabled) return;

            foreach (var entity in _inventories.GetEntities())
            {
                var inventory = entity.Get<InventoryComponent>();
                if (inventory.Stacks.Count == 0) continue;

                List<string>? toRemove = null;

                foreach (var itemId in inventory.Stacks.Keys.ToList())
                {
                    var stack = inventory.Stacks[itemId];
                    var definition = _itemService.GetDefinition(itemId);

                    if (definition is null || stack.Quantity <= 0)
                    {
                        toRemove ??= new List<string>();
                        toRemove.Add(itemId);
                        continue;
                    }

                    var maxStack = Math.Max(1, definition.MaxStack);
                    if (stack.Quantity > maxStack)
                    {
                        stack.Quantity = maxStack;
                        inventory.Stacks[itemId] = stack;
                    }
                }

                if (toRemove is not null)
                {
                    foreach (var itemId in toRemove)
                    {
                        inventory.Stacks.Remove(itemId);
                    }
                }
            }
        }

        public void Dispose()
        {
            _inventories.Dispose();
        }
    }
}
