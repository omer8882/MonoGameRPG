using System;
using DefaultEcs;

namespace ANewWorld.Engine.Items
{
    public sealed class InventoryService
    {
        private readonly ItemService _itemService;

        public InventoryService(ItemService itemService)
        {
            _itemService = itemService;
        }

        public InventoryComponent EnsureInventory(Entity entity)
        {
            if (!entity.Has<InventoryComponent>())
            {
                entity.Set(new InventoryComponent());
            }

            return entity.Get<InventoryComponent>();
        }

        public int AddItem(Entity entity, string itemId, int quantity)
        {
            if (quantity <= 0) return 0;

            var definition = _itemService.GetDefinition(itemId) ??
                throw new ArgumentException($"Unknown item id '{itemId}'.", nameof(itemId));

            var inventory = EnsureInventory(entity);

            if (!inventory.Stacks.TryGetValue(itemId, out var stack))
            {
                stack = new ItemStack { ItemId = itemId, Quantity = 0 };
            }

            var maxStack = Math.Max(1, definition.MaxStack);
            var space = maxStack - stack.Quantity;
            if (space <= 0)
            {
                inventory.Stacks[itemId] = stack;
                return 0;
            }

            var added = Math.Min(space, quantity);
            stack.Quantity += added;
            inventory.Stacks[itemId] = stack;
            return added;
        }

        public int RemoveItem(Entity entity, string itemId, int quantity)
        {
            if (quantity <= 0) return 0;
            if (!entity.Has<InventoryComponent>()) return 0;

            var inventory = entity.Get<InventoryComponent>();
            if (!inventory.Stacks.TryGetValue(itemId, out var stack) || stack.Quantity <= 0) return 0;

            var removed = Math.Min(stack.Quantity, quantity);
            stack.Quantity -= removed;

            if (stack.Quantity <= 0)
            {
                inventory.Stacks.Remove(itemId);
            }
            else
            {
                inventory.Stacks[itemId] = stack;
            }

            return removed;
        }

        public int GetQuantity(Entity entity, string itemId)
        {
            if (!entity.Has<InventoryComponent>()) return 0;
            var inventory = entity.Get<InventoryComponent>();
            return inventory.Stacks.TryGetValue(itemId, out var stack) ? stack.Quantity : 0;
        }

        public bool ContainsAtLeast(Entity entity, string itemId, int quantity)
        {
            if (quantity <= 0) return true;
            return GetQuantity(entity, itemId) >= quantity;
        }

        public void Clear(Entity entity)
        {
            if (!entity.Has<InventoryComponent>()) return;
            var inventory = entity.Get<InventoryComponent>();
            inventory.Stacks.Clear();
        }
    }
}
