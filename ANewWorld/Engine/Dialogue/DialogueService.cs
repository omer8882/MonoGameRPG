using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Dialogue
{
    public sealed class DialogueService
    {
        private readonly Dictionary<string, DialogueGraph> _graphs = new();
        public DialogueContext Context { get; } = new DialogueContext();

        private ContentManager content;

        public DialogueService(ContentManager content) 
        {
            this.content = content;
            string basePath = Path.Combine("Data", "NPCs");
            Load(Path.Combine(basePath, "dialogues.json"));
        }

        public void Load(string path)
        {
            var data = content.LoadJson<DialogueData>(path);
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

        public bool CheckConditions(List<DialogueCondition>? conds)
        {
            if (conds == null || conds.Count == 0) return true;
            foreach (var c in conds)
            {
                if (!Context.Flags.TryGetValue(c.Flag, out var val)) val = false;
                if (val != c.Equals) return false;
            }
            return true;
        }

        public void ApplyActions(List<DialogueAction>? actions)
        {
            if (actions == null) return;
            foreach (var a in actions)
            {
                if (!string.IsNullOrEmpty(a.SetFlag))
                    Context.Flags[a.SetFlag] = a.Value;
            }
        }

        public string Substitute(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new StringBuilder(text);
            foreach (var kv in Context.Vars)
            {
                sb.Replace("{" + kv.Key + "}", kv.Value);
            }
            return sb.ToString();
        }
    }

    public sealed class DialogueContext
    {
        public Dictionary<string, bool> Flags { get; } = new();
        public Dictionary<string, string> Vars { get; } = new();
    }
}
