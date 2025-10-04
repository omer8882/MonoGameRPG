using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Components
{
    public enum MovementAction
    {
        Idle,
        Walk
    }

    public readonly struct MovementAnimationKey
    {
        public readonly MovementAction Action;
        public readonly Facing Direction;

        public MovementAnimationKey(MovementAction action, Facing direction)
        {
            Action = action;
            Direction = direction;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Action * 397) ^ (int)Direction;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is MovementAnimationKey other && other.Action == Action && other.Direction == Direction;
        }

        public override string ToString() => $"({Action} {Direction})";
    }

    public struct SpriteAnimatorComponent
    {
        public Dictionary<MovementAnimationKey, AnimationClip> Clips; // typed state key -> clip
        public MovementAnimationKey StateKey; // current state key
        public int FrameIndex;        // current frame in clip
        public float Timer;           // seconds accumulated

        public override string ToString() => $"State: {StateKey}, Frame: {FrameIndex+1}/{Clips[StateKey].Frames.Count}";
    }

    public struct AnimationClip
    {
        public List<Rectangle> Frames; // source rectangles per frame
        public float FrameDuration;     // seconds per frame
        public bool Loop = true;               // should loop

        public AnimationClip(List<Rectangle> frames, float frameDuration)
        {
            Frames = frames;
            FrameDuration = frameDuration;
            Loop = true;
        }
    }
}
