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
                var action = moving ? MovementAction.Walk : MovementAction.Idle;
                var newKey = new MovementAnimationKey(action, face.Value);
                if (!newKey.Equals(anim.StateKey))
                {
                    anim.StateKey = newKey;
                    anim.FrameIndex = 0;
                    anim.Timer = 0f;
                }
            }
        }

        public void Dispose() => _set?.Dispose();
    }
}
