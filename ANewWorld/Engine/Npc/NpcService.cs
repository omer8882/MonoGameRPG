using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Npc
{
    public sealed class NpcService
    {
        private readonly Dictionary<string, NpcDefinition> _definitions = [];
        private readonly Dictionary<string, MapSpawnData> _spawnRules = [];

        public NpcService()
        {
            string basePath = Path.Combine("Data", "NPCs");
            LoadDefinitions(Path.Combine(basePath, "npcs.json"));
            LoadSpawnRules(Path.Combine(basePath, "npc_spawns.json"));
        }

        public void LoadDefinitions(string path)
        {
            var data = ContentLoader.LoadJson<NpcDefinitionData>(path);

            if (data?.Npcs is null) return;
            
            foreach (var kvp in data.Npcs)
            {
                kvp.Value.Id = kvp.Key;
                _definitions[kvp.Key] = kvp.Value;
            }
        }
        
        public void LoadSpawnRules(string path)
        {
            var data = ContentLoader.LoadJson<NpcSpawnData>(path);

            if (data?.Spawns == null) return;
            
            foreach (var kvp in data.Spawns)
            {
                kvp.Value.MapId = kvp.Key;
                _spawnRules[kvp.Key] = kvp.Value;
            }
        }
        
        public NpcDefinition? GetDefinition(string npcId) => 
            _definitions.GetValueOrDefault(npcId);
        
        public MapSpawnData? GetSpawnRulesForMap(string mapId) => 
            _spawnRules.GetValueOrDefault(mapId);
    }
}
