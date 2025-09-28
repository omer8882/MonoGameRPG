using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Components
{
    public struct SpriteAnimatorComponent
    {
        public Dictionary<string, AnimationClip> Clips; // state name -> clip
        public string State;        // current state key
        public int FrameIndex;      // current frame in clip
        public float Timer;         // seconds accumulated
    }

    public struct AnimationClip
    {
        public List<Rectangle> Frames; // source rectangles per frame
        public float FrameDuration;     // seconds per frame
        public bool Loop;               // should loop
    }
}
