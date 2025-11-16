using System.Collections.Generic;

namespace ANewWorld.Engine.Items
{
    public sealed class ItemDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int MaxStack { get; set; } = 1;
        public Dictionary<string, float>? Properties { get; set; }
    }

    public sealed class ItemDefinitionData
    {
        public Dictionary<string, ItemDefinition> Items { get; set; } = new();
    }
}
