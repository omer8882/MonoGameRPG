using System;

namespace ANewWorld.Engine.Items
{
    internal static class InventorySlotHelper
    {
        public static void RegisterSlot(InventoryComponent inventory, string itemId)
        {
            if (HasSlot(inventory, itemId))
                return;

            inventory.SlotOrder.Add(itemId);
            if (inventory.SelectedIndex < 0)
                inventory.SelectedIndex = inventory.SlotOrder.Count - 1;
        }

        public static void UnregisterSlot(InventoryComponent inventory, string itemId)
        {
            int index = inventory.SlotOrder.FindIndex(id => string.Equals(id, itemId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                inventory.SlotOrder.RemoveAt(index);
            }

            if (inventory.SlotOrder.Count == 0)
            {
                inventory.SelectedIndex = -1;
            }
            else if (inventory.SelectedIndex >= inventory.SlotOrder.Count)
            {
                inventory.SelectedIndex = inventory.SlotOrder.Count - 1;
            }
        }

        public static string? GetActiveItemId(InventoryComponent inventory)
        {
            if (inventory.SelectedIndex < 0 || inventory.SelectedIndex >= inventory.SlotOrder.Count)
                return null;

            return inventory.SlotOrder[inventory.SelectedIndex];
        }

        private static bool HasSlot(InventoryComponent inventory, string itemId) =>
            inventory.SlotOrder.Exists(id => string.Equals(id, itemId, StringComparison.OrdinalIgnoreCase));
    }
}
