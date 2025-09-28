using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;

namespace ANewWorld.Engine.Systems
{
    public sealed class AnimationSystem : ISystem<float>, System.IDisposable
    {
        private readonly EntitySet _set;
        public bool IsEnabled { get; set; } = true;

        public AnimationSystem(World world)
        {
            _set = world.GetEntities().With<SpriteAnimatorComponent>().With<SpriteComponent>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;
            foreach (ref readonly var e in _set.GetEntities())
            {
                ref var anim = ref e.Get<SpriteAnimatorComponent>();
                ref var sprite = ref e.Get<SpriteComponent>();
                if (!anim.Clips.TryGetValue(anim.StateKey, out var clip) || clip.Frames == null || clip.Frames.Count == 0)
                    continue;

                anim.Timer += dt;
                while (anim.Timer >= clip.FrameDuration)
                {
                    anim.Timer -= clip.FrameDuration;
                    anim.FrameIndex++;
                    if (anim.FrameIndex >= clip.Frames.Count)
                    {
                        if (clip.Loop)
                            anim.FrameIndex = 0;
                        else
                            anim.FrameIndex = clip.Frames.Count - 1; // clamp
                    }
                }
                var index = anim.FrameIndex;
                if (index < 0 || index >= clip.Frames.Count) index = 0;
                sprite.SourceRect = clip.Frames[index];
            }
        }

        public void Dispose() => _set?.Dispose();
    }
}
