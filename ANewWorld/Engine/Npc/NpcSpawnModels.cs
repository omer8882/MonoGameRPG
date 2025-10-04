using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ANewWorld.Engine.Npc
{
    public class NpcSpawnCondition
    {
        public List<string>? RequiredFlags { get; set; }
        public List<string>? ForbiddenFlags { get; set; }
        public string? TimeOfDay { get; set; }
        public string? QuestStage { get; set; }
        public int? MinPlayerLevel { get; set; }
    }

    public class NpcSpawnRule
    {
        public string NpcId { get; set; } = string.Empty;
        public Vector2 SpawnPoint { get; set; }
        public NpcSpawnCondition Conditions { get; set; } = new();
    }

    public class MapSpawnData
    {
        public string MapId { get; set; } = string.Empty;
        public List<NpcSpawnRule> Npcs { get; set; } = new();
    }

    public class NpcSpawnData
    {
        public Dictionary<string, MapSpawnData> Spawns { get; set; } = new();
    }
}
