using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Audio
{
    // One-shot UI or global SFX
    public struct PlaySfx
    {
        public string Asset;   // Content asset name (without extension)
        public float Volume;   // 0..1
        public float Pitch;    // -1..1
        public float Pan;      // -1..1
    }

    // One-shot positional SFX in world space
    public struct PlaySfxAt
    {
        public string Asset;
        public Vector2 WorldPosition;
        public float Volume;
        public float Pitch;
    }

    // Start a looping SFX until stopped
    public struct StartLoop
    {
        public string Asset;
        public string? Key; // optional logical key; defaults to Asset when null
        public float Volume;
        public float Pitch;
        public float Pan;
    }

    // Stop a looping SFX
    public struct StopLoop
    {
        public string? Key; // match StartLoop key; if null, use Asset
        public string Asset; // fallback identifier
    }
}
