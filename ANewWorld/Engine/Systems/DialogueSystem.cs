using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Dialogue;
using ANewWorld.Engine.Input;
using System.Collections.Generic;

namespace ANewWorld.Engine.Systems
{
    public sealed class DialogueSystem : ISystem<float>, System.IDisposable
    {
        private readonly World _world;
        private readonly DialogueService _service;
        private readonly EntitySet _events;
        private readonly InputActionService _input;

        public bool IsActive { get; private set; }
        public Entity? CurrentNpc { get; private set; }
        private DialogueGraph? _graph;
        private DialogueNode? _node;

        // UI state for choices
        public IReadOnlyList<DialogueChoice>? CurrentChoices => _node?.Choices;
        public int SelectedChoice { get; private set; } = 0;

        // Required by ISystem<T>
        public bool IsEnabled { get; set; } = true;

        public DialogueSystem(World world, DialogueService service, InputActionService input)
        {
            _world = world;
            _service = service;
            _input = input;
            _events = world.GetEntities().With<InteractionStarted>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            if (!IsActive)
            {
                foreach (ref readonly var e in _events.GetEntities())
                {
                    var target = e.Get<InteractionStarted>().Target;
                    if (target.Has<DialogueComponent>())
                    {
                        var id = target.Get<DialogueComponent>().DialogueId;
                        var g = _service.Get(id);
                        if (g != null)
                        {
                            _graph = g;
                            _node = g.Nodes.Find(n => n.Id == g.Start) ?? (g.Nodes.Count > 0 ? g.Nodes[0] : null);
                            CurrentNpc = target;
                            IsActive = _node != null;
                            SelectedChoice = 0;
                        }
                    }
                    e.Dispose();
                    if (IsActive) break;
                }
                return;
            }

            // Active: handle advancement
            if (_node == null)
            {
                IsActive = false;
                return;
            }

            var hasChoices = _node.Choices != null && _node.Choices.Count > 0;
            if (hasChoices)
            {
                // Navigate choices with Up/Down, confirm with Interact
                if (_input.IsActionJustPressed("MoveUp"))
                    SelectedChoice = (SelectedChoice - 1 + _node.Choices!.Count) % _node.Choices!.Count;
                if (_input.IsActionJustPressed("MoveDown"))
                    SelectedChoice = (SelectedChoice + 1) % _node.Choices!.Count;
                if (_input.IsActionJustPressed("Interact"))
                {
                    var nextId = _node.Choices![SelectedChoice].Next;
                    _node = _graph?.Nodes.Find(n => n.Id == nextId);
                    SelectedChoice = 0;
                    if (_node == null) IsActive = false;
                }
            }
            else
            {
                // Linear: advance on Interact
                if (_input.IsActionJustPressed("Interact"))
                {
                    if (!string.IsNullOrEmpty(_node.Next))
                        _node = _graph?.Nodes.Find(n => n.Id == _node.Next);
                    else
                        _node = null;
                    if (_node == null) IsActive = false;
                }
            }
        }

        public string? CurrentText => _node?.Text;

        public void Dispose() => _events?.Dispose();
    }
}
