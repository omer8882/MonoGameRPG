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

            var hasStack = inventory.Stacks.TryGetValue(itemId, out var stack);
            if (!hasStack)
            {
                stack = new ItemStack { ItemId = itemId, Quantity = 0 };
                InventorySlotHelper.RegisterSlot(inventory, itemId);
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
                InventorySlotHelper.UnregisterSlot(inventory, itemId);
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
            inventory.SlotOrder.Clear();
            inventory.SelectedIndex = -1;
        }

        public string? GetActiveItemId(Entity entity)
        {
            if (!entity.Has<InventoryComponent>()) return null;
            return InventorySlotHelper.GetActiveItemId(entity.Get<InventoryComponent>());
        }

        public bool SetSelectedIndex(Entity entity, int index)
        {
            if (!entity.Has<InventoryComponent>()) return false;
            var inventory = entity.Get<InventoryComponent>();
            if (inventory.SlotOrder.Count == 0) { inventory.SelectedIndex = -1; return false; }
            index = Math.Clamp(index, 0, inventory.SlotOrder.Count - 1);
            var changed = inventory.SelectedIndex != index;
            inventory.SelectedIndex = index;
            return changed;
        }

        public bool OffsetSelectedIndex(Entity entity, int offset)
        {
            if (offset == 0) return false;
            if (!entity.Has<InventoryComponent>()) return false;

            var inventory = entity.Get<InventoryComponent>();
            var count = inventory.SlotOrder.Count;
            if (count == 0)
            {
                inventory.SelectedIndex = -1;
                return false;
            }

            var current = inventory.SelectedIndex;
            if (current < 0)
                current = 0;

            var next = ((current + offset) % count + count) % count;
            var changed = next != inventory.SelectedIndex;
            inventory.SelectedIndex = next;
            return changed;
        }

        public bool TryGetActiveStack(Entity entity, out ItemStack stack)
        {
            stack = default;
            if (!entity.Has<InventoryComponent>()) return false;
            var inventory = entity.Get<InventoryComponent>();
            var activeId = InventorySlotHelper.GetActiveItemId(inventory);
            if (string.IsNullOrEmpty(activeId)) return false;
            return inventory.Stacks.TryGetValue(activeId, out stack);
        }

        public bool TryConsumeActiveItem(Entity entity, int quantity, out string itemId, out ItemDefinition definition)
        {
            itemId = string.Empty;
            definition = null!;
            if (quantity <= 0) return false;
            if (!entity.Has<InventoryComponent>()) return false;

            var inventory = entity.Get<InventoryComponent>();
            var activeId = InventorySlotHelper.GetActiveItemId(inventory);
            if (string.IsNullOrEmpty(activeId)) return false;
            if (!_itemService.TryGetDefinition(activeId, out definition)) return false;

            var removed = RemoveItem(entity, activeId, quantity);
            if (removed <= 0) return false;

            itemId = activeId;
            return true;
        }
    }
}
