using DefaultEcs;
using Microsoft.Xna.Framework.Input;
using ANewWorld.Engine.Components;
using Microsoft.Xna.Framework;
using ANewWorld.Engine.Input;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Systems
{
    public sealed class PlayerMovementInputSystem
    {
        private readonly World _world;
        private readonly InputActionService _actions;

        public PlayerMovementInputSystem(World world, InputActionService actions)
        {
            _world = world;
            _actions = actions;
        }

        public void Update(float delta)
        {
            _actions.Update();

            var player = _world.GetPlayer();
            var vel = player.Get<Velocity>();
            vel.Value = Vector2.Zero;
            if (_actions.IsActionActive("MoveUp")) vel.Value.Y = -1;
            if (_actions.IsActionActive("MoveDown")) vel.Value.Y = 1;
            if (_actions.IsActionActive("MoveLeft")) vel.Value.X = -1;
            if (_actions.IsActionActive("MoveRight")) vel.Value.X = 1;
            player.Set(vel);
        }
    }
}