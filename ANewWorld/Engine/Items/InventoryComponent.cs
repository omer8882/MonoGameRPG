using System.Collections.Generic;

namespace ANewWorld.Engine.Items
{
    public sealed class InventoryComponent
    {
        public InventoryComponent()
        {
            Stacks = new Dictionary<string, ItemStack>(System.StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, ItemStack> Stacks { get; }
    }

    public struct ItemStack
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
