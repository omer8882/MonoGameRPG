using System;
using System.Collections.Generic;

namespace ANewWorld.Engine.Items
{
    public sealed class InventoryComponent
    {
        public InventoryComponent()
        {
            Stacks = new Dictionary<string, ItemStack>(StringComparer.OrdinalIgnoreCase);
            SlotOrder = [];
            SelectedIndex = -1;
        }

        public Dictionary<string, ItemStack> Stacks { get; }
        public List<string> SlotOrder { get; }
        public int SelectedIndex { get; set; }
    }

    public struct ItemStack
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
