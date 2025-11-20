using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Input;

namespace ANewWorld.Engine.Systems
{
    public sealed class InteractionSystem : ISystem<float>, System.IDisposable
    {
        private readonly World _world;
        private readonly InputActionService _input;
        private readonly EntitySet _interactables;
        private readonly EntitySet _players;

        public (Entity? entity, Vector2 position, float radius, string? prompt)? Current { get; private set; }
        public bool IsEnabled { get; set; } = true;

        public InteractionSystem(World world, InputActionService input)
        {
            _world = world;
            _input = input;
            _interactables = world.GetEntities().With<Transform>().With<Interactable>().AsSet();
            _players = world.GetEntities().With<Transform>().With<Tag>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            // Find player position (first matching Player tag)
            Vector2 playerPos = Vector2.Zero;
            foreach (var e in _players.GetEntities())
            {
                if (e.Get<Tag>().Value == "Player")
                {
                    playerPos = e.Get<Transform>().Position;
                    break;
                }
            }

            Entity? best = null;
            float bestDistSq = float.MaxValue;
            float radius = 0f;
            string? prompt = null;
            Vector2 pos = Vector2.Zero;

            foreach (var interactable in _interactables.GetEntities())
            {
                ref var t = ref interactable.Get<Transform>();
                ref var i = ref interactable.Get<Interactable>();
                if (!i.Enabled) continue;
                float r = i.Radius <= 0 ? 24f : i.Radius;
                float d2 = Vector2.DistanceSquared(playerPos, t.Position);
                if (d2 <= r * r && d2 < bestDistSq)
                {
                    best = interactable;
                    bestDistSq = d2;
                    radius = r;
                    prompt = i.Prompt;
                    pos = t.Position;
                }
            }

            Current = (best, pos, radius, prompt);

            // Start interaction on key
            if (best.HasValue && _input.IsActionJustPressed("Interact"))
            {
                var evt = _world.CreateEntity();
                evt.Set(new InteractionStarted { Target = best.Value });
                Current = null;//= Current with { entity = null }; // Clear current after interaction starts
            }
        }

        public void Dispose()
        {
            _interactables?.Dispose();
            _players?.Dispose();
        }
    }

    public struct InteractionStarted
    {
        public Entity Target;
    }
}
