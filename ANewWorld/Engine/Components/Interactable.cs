namespace ANewWorld.Engine.Components
{
    public struct Interactable
    {
        public float Radius;       // in world pixels
        public string? Prompt;     // e.g., "Talk", "Open"
        public bool Enabled;

        public override string ToString()
        {
            return $"Interactable(Radius={Radius}, Prompt={Prompt}, Enabled={Enabled})";
        }
    }
}
