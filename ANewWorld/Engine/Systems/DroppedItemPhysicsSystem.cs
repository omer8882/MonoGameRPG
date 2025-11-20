using System;
using System.Collections.Generic;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Items;
using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class DroppedItemPhysicsSystem : ISystem<float>, IDisposable
    {
        private readonly EntitySet _items;

        public bool IsEnabled { get; set; } = true;

        public DroppedItemPhysicsSystem(World world)
        {
            _items = world.GetEntities()
                .With<Transform>()
                .With<DroppedItemPhysics>()
                .AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            var toRemove = new List<Entity>(_items.Count);

            foreach (ref readonly var entity in _items.GetEntities())
            {
                var transform = entity.Get<Transform>();
                var physics = entity.Get<DroppedItemPhysics>();

                transform.Position += physics.Velocity * dt;

                var speed = physics.Velocity.Length();
                if (speed <= physics.MinimumSpeed)
                {
                    entity.Set(transform);
                    toRemove.Add(entity);
                    continue;
                }

                var drag = MathHelper.Clamp(physics.Drag, 0f, 20f);
                var decay = Math.Clamp(1f - drag * dt, 0f, 1f);
                physics.Velocity *= decay;

                entity.Set(physics);
                entity.Set(transform);
            }

            foreach (var entity in toRemove)
            {
                entity.Remove<DroppedItemPhysics>();
            }
        }

        public void Dispose()
        {
            _items.Dispose();
        }
    }
}
