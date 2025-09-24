using DefaultEcs;
using ANewWorld.Engine.Components;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class MovementSystem
    {
        private readonly World _world;
        private readonly float _speed = 100f; // pixels per second

        public MovementSystem(World world)
        {
            _world = world;
        }

        public void Update(float dt)
        {
            var set = _world.GetEntities().With<Transform>().With<Velocity>().AsSet();
            foreach (var entity in set.GetEntities())
            {
                var t = entity.Get<Transform>();
                var v = entity.Get<Velocity>();
                t.Position += v.Value * _speed * dt;
                entity.Set(t);
            }
        }
    }
}