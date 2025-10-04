using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;

namespace ANewWorld.Engine.Systems
{
    public sealed class FacingDirectionSystem : ISystem<float>, System.IDisposable
    {
        private readonly EntitySet _set;
        public bool IsEnabled { get; set; } = true;

        public FacingDirectionSystem(World world)
        {
            _set = world.GetEntities().With<Velocity>().With<FacingDirection>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;
            foreach (ref readonly var entity in _set.GetEntities())
            {
                ref var v = ref entity.Get<Velocity>();
                ref var f = ref entity.Get<FacingDirection>();
                if (v.Value.LengthSquared() <= 0.0001f)
                    continue;

                if (System.MathF.Abs(v.Value.X) > System.MathF.Abs(v.Value.Y))
                    f.Value = v.Value.X >= 0 ? Facing.Right : Facing.Left;
                else
                    f.Value = v.Value.Y >= 0 ? Facing.Down : Facing.Up;
            }
        }

        public void Dispose() => _set?.Dispose();
    }
}
