using ANewWorld.Engine.Components;
using ANewWorld.Engine.Input;
using ANewWorld.Engine.Items;
using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class PlayerInventoryInputSystem : ISystem<float>, System.IDisposable
    {
        private static readonly string[] SlotActions =
        {
            "SelectItem1",
            "SelectItem2",
            "SelectItem3",
            "SelectItem4"
        };

        private readonly InputActionService _input;
        private readonly InventoryService _inventory;
        private readonly WorldItemFactory _itemFactory;
        private readonly EntitySet _playerSet;

        public bool IsEnabled { get; set; } = true;

        public PlayerInventoryInputSystem(World world, InputActionService input, InventoryService inventory, WorldItemFactory itemFactory)
        {
            _input = input;
            _inventory = inventory;
            _itemFactory = itemFactory;
            _playerSet = world.GetEntities()
                .With<InventoryComponent>()
                .With<Transform>()
                .With<Tag>()
                .With<FacingDirection>()
                .AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            var player = GetPlayer();
            if (!player.HasValue) return;

            HandleSelection(player.Value);
            HandleDrop(player.Value);
        }

        private void HandleSelection(Entity player)
        {
            for (int i = 0; i < SlotActions.Length; i++)
            {
                if (_input.IsActionJustPressed(SlotActions[i]))
                {
                    _inventory.SetSelectedIndex(player, i);
                }
            }

            var wheelSteps = _input.GetMouseWheelSteps();
            if (wheelSteps != 0)
            {
                _inventory.OffsetSelectedIndex(player, -wheelSteps);
            }

            var dPadStep = _input.GetDPadHorizontalStep();
            if (dPadStep == 0)
                dPadStep = _input.GetDPadVerticalStep();

            if (dPadStep != 0)
            {
                _inventory.OffsetSelectedIndex(player, dPadStep);
            }
        }

        private void HandleDrop(Entity player)
        {
            if (!_input.IsActionJustPressed("DropItem"))
                return;

            if (!_inventory.TryConsumeActiveItem(player, 1, out var itemId, out var definition))
                return;

            var transform = player.Get<Transform>();
            var facing = player.Get<FacingDirection>().Value;
            var direction = FacingToDirection(facing);
            if (direction == Vector2.Zero)
                direction = Vector2.UnitY;

            var spawnPosition = transform.Position + direction * 18f;
            var impulse = direction * 150f;

            _itemFactory.SpawnWorldItem(itemId, 1, spawnPosition, impulse);
        }

        private static Vector2 FacingToDirection(Facing facing) => facing switch
        {
            Facing.Up => new Vector2(0f, -1f),
            Facing.Down => new Vector2(0f, 1f),
            Facing.Left => new Vector2(-1f, 0f),
            Facing.Right => new Vector2(1f, 0f),
            _ => Vector2.UnitY
        };

        private Entity? GetPlayer()
        {
            foreach (ref readonly var entity in _playerSet.GetEntities())
            {
                if (entity.Get<Tag>().Value == "Player")
                {
                    return entity;
                }
            }

            return null;
        }

        public void Dispose()
        {
            _playerSet.Dispose();
        }
    }
}
