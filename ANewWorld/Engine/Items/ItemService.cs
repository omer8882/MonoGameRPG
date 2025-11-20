using System;
using System.Collections.Generic;
using System.IO;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Items
{
    public sealed class ItemService
    {
        private readonly Dictionary<string, ItemDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

        public ItemService(ItemDefinitionData itemData)
        {
            LoadDefinitions(itemData);
        }

        public void LoadDefinitions(ItemDefinitionData data)
        {
            if (data?.Items == null) return;

            foreach (var kvp in data.Items)
            {
                kvp.Value.Id = kvp.Key;
                _definitions[kvp.Key] = kvp.Value;
            }
        }

        public ItemDefinition? GetDefinition(string itemId) =>
            _definitions.GetValueOrDefault(itemId);

        public bool TryGetDefinition(string itemId, out ItemDefinition definition) =>
            _definitions.TryGetValue(itemId, out definition);

        public IReadOnlyDictionary<string, ItemDefinition> GetAllDefinitions() => _definitions;
    }
}
