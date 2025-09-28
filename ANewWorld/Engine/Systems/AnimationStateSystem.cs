using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;

namespace ANewWorld.Engine.Systems
{
    public sealed class AnimationStateSystem : ISystem<float>, System.IDisposable
    {
        private readonly EntitySet _set;
        public bool IsEnabled { get; set; } = true;

        public AnimationStateSystem(World world)
        {
            _set = world.GetEntities().With<SpriteAnimatorComponent>().With<Velocity>().With<FacingDirection>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;
            foreach (ref readonly var e in _set.GetEntities())
            {
                ref var v = ref e.Get<Velocity>();
                ref var anim = ref e.Get<SpriteAnimatorComponent>();
                ref var face = ref e.Get<FacingDirection>();

                var moving = v.Value.LengthSquared() > 0.0001f;
                string baseState = moving ? "Walk" : "Idle";
                string dir = face.Value switch
                {
                    Facing.Up => "Up",
                    Facing.Left => "Left",
                    Facing.Right => "Right",
                    _ => "Down"
                };
                var newState = baseState + dir;
                if (anim.State != newState)
                {
                    anim.State = newState;
                    anim.FrameIndex = 0;
                    anim.Timer = 0f;
                }
            }
        }

        public void Dispose() => _set?.Dispose();
    }
}
