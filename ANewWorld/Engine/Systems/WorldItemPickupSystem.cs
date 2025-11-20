using System;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Items;
using DefaultEcs;
using DefaultEcs.System;

namespace ANewWorld.Engine.Systems
{
    public sealed class WorldItemPickupSystem : ISystem<float>, IDisposable
    {
        private readonly InventoryService _inventoryService;
        private readonly EntitySet _interactionEvents;
        private readonly EntitySet _players;

        public bool IsEnabled { get; set; } = true;

        public WorldItemPickupSystem(World world, InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _interactionEvents = world.GetEntities().With<InteractionStarted>().AsSet();
            _players = world.GetEntities().With<InventoryComponent>().With<Tag>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            var player = GetPlayer();
            if (!player.HasValue) return;

            foreach (ref readonly var evt in _interactionEvents.GetEntities())
            {
                var interaction = evt.Get<InteractionStarted>();
                var target = interaction.Target;
                if (!target.Has<WorldItemComponent>())
                    continue;

                var item = target.Get<WorldItemComponent>();
                var added = _inventoryService.AddItem(player.Value, item.ItemId, item.Quantity);
                if (added <= 0)
                {
                    evt.Dispose();
                    continue;
                }

                if (added >= item.Quantity)
                {
                    target.Dispose();
                }
                else
                {
                    item.Quantity -= added;
                    target.Set(item);

                    if (target.Has<Interactable>())
                    {
                        var interactable = target.Get<Interactable>();
                        interactable.Prompt = WorldItemFactory.BuildPrompt(item.DisplayName, item.Quantity);
                        target.Set(interactable);
                    }
                }

                evt.Dispose();
            }
        }

        private Entity? GetPlayer()
        {
            foreach (ref readonly var entity in _players.GetEntities())
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
            _interactionEvents.Dispose();
            _players.Dispose();
        }
    }
}
