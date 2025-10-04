using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Content;
using ANewWorld.Engine.Serialization;

namespace ANewWorld.Engine.Npc
{
    public sealed class NpcService
    {
        private readonly Dictionary<string, NpcDefinition> _definitions = new();
        private readonly Dictionary<string, MapSpawnData> _spawnRules = new();

        private readonly JsonSerializerOptions options = new()
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new Vector2JsonConverter() }
        };


        public void LoadDefinitions(string path)
        {
            if (!File.Exists(path)) return;
            
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<NpcDefinitionData>(json, options);
            
            if (data?.Npcs == null) return;
            
            foreach (var kvp in data.Npcs)
            {
                kvp.Value.Id = kvp.Key;
                _definitions[kvp.Key] = kvp.Value;
            }
        }
        
        public void LoadSpawnRules(string path)
        {
            if (!File.Exists(path)) return;
            
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<NpcSpawnData>(json, options);
            
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
