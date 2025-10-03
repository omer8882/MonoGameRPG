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

    public sealed class DialogueCondition
    {
        public string Flag { get; set; } = string.Empty;
        public bool Equals { get; set; } = true;
    }

    public sealed class DialogueAction
    {
        public string SetFlag { get; set; } = string.Empty;
        public bool Value { get; set; } = true;
    }

    public sealed class DialogueChoice
    {
        public string Text { get; set; } = string.Empty;
        public string Next { get; set; } = string.Empty;
        public List<DialogueCondition>? Conditions { get; set; }
        public List<DialogueAction>? Actions { get; set; }
    }

    public sealed class DialogueNode
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Next { get; set; }
        public List<DialogueChoice>? Choices { get; set; }
        public List<DialogueCondition>? Conditions { get; set; }
        public List<DialogueAction>? Actions { get; set; }
    }
}
