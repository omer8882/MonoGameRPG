using Xunit;
using DefaultEcs;
using ANewWorld.Engine.Components;

namespace ANewWorld.Tests
{
    public class HeadlessSmokeTests
    {
        [Fact]
        public void MovementUpdatesPositionOverTenTicks()
        {
            using var world = new World();
            var e = world.CreateEntity();
            e.Set(new Transform { Position = new Microsoft.Xna.Framework.Vector2(0,0), Rotation = 0f, Scale = Microsoft.Xna.Framework.Vector2.One });
            e.Set(new Velocity { Value = new Microsoft.Xna.Framework.Vector2(1,0) });

            var movement = new DefaultEcs.System.LinearSystem<float>(world.GetEntities().With<Transform>().With<Velocity>().AsSet(), (dt, entity) =>
            {
                var t = entity.Get<Transform>();
                var v = entity.Get<Velocity>();
                t.Position += v.Value * 10f * dt;
                entity.Set(t);
            });

            float initialX = e.Get<Transform>().Position.X;
            for (int i = 0; i < 10; i++)
            {
                movement.Update(1f/60f);
            }

            float finalX = e.Get<Transform>().Position.X;
            Assert.True(finalX > initialX);
        }
    }
}