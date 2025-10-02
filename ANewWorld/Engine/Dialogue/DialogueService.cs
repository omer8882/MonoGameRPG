using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ANewWorld.Engine.Dialogue
{
    public sealed class DialogueService
    {
        private readonly Dictionary<string, DialogueGraph> _graphs = new();

        private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<DialogueData>(json, options);
            if (data == null) return;
            foreach (var kv in data.Dialogues)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key))
                {
                    kv.Value.Id = kv.Key;
                    _graphs[kv.Key] = kv.Value;
                }
            }
        }

        public DialogueGraph? Get(string id)
        {
            _graphs.TryGetValue(id, out var g);
            return g;
        }
    }
}
