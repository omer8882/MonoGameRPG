using System.Collections.Generic;

namespace ANewWorld.Engine.Dialogue
{
    public sealed class DialogueData
    {
        public Dictionary<string, DialogueGraph> Dialogues { get; set; } = new();
    }

    public sealed class DialogueGraph
    {
        public string Id { get; set; } = string.Empty;
        public List<DialogueNode> Nodes { get; set; } = new();
        public string Start { get; set; } = "start";
    }

    public sealed class DialogueChoice
    {
        public string Text { get; set; } = string.Empty;
        public string Next { get; set; } = string.Empty;
    }

    public sealed class DialogueNode
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Next { get; set; }
        public List<DialogueChoice>? Choices { get; set; }
    }
}
