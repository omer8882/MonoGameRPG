using DefaultEcs;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Tilemap;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class CollisionSystem
    {
        private readonly World _world;
        private readonly CollisionGridService _collisionGrid;
        private const float slidingScale = 0.7f; // reduce speed when sliding along wall

        public CollisionSystem(World world, CollisionGridService collisionGrid)
        {
            _world = world;
            _collisionGrid = collisionGrid;
        }

        private bool Blocked(float x, float y) => _collisionGrid.IsBlocked(x, y);

        public void Update(float dt)
        {
            var set = _world.GetEntities().With<Transform>().With<Velocity>().AsSet();
            foreach (var entity in set.GetEntities())
            {
                var t = entity.Get<Transform>();
                var v = entity.Get<Velocity>();
                Vector2 intended = t.Position + v.Value * dt * 100f; // match movement speed
                if (!Blocked(intended.X, intended.Y))
                {
                    return; // no collision, proceed as normal
                }

                // Checing both x and y seperately to allow sliding along walls
                if (Blocked(t.Position.X, intended.Y))
                {
                    v.Value = new Vector2(v.Value.X * slidingScale, 0);
                    entity.Set(v);
                }
                if(Blocked(intended.X, t.Position.Y))
                {
                    v.Value = new Vector2(0, v.Value.Y * slidingScale);
                    entity.Set(v);
                }
            }
        }
    }
}
