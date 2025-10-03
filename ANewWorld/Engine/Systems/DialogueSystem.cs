using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Dialogue;
using ANewWorld.Engine.Input;
using System.Collections.Generic;
using System;
using ANewWorld.Engine.Audio;

namespace ANewWorld.Engine.Systems
{
    public sealed class DialogueSystem : ISystem<float>, System.IDisposable
    {
        private readonly World _world;
        private readonly DialogueService _service;
        private readonly EntitySet _events;
        private readonly InputActionService _input;
        private readonly AudioBus _audioBus;

        public bool IsActive { get; private set; }
        public Entity? CurrentNpc { get; private set; }
        private DialogueGraph? _graph;
        private DialogueNode? _node;

        public string? NpcName => CurrentNpc.HasValue && CurrentNpc.Value.Has<Name>() ? CurrentNpc.Value.Get<Name>().Value : null;

        // UI state for choices
        public IReadOnlyList<DialogueChoice>? CurrentChoices => _node?.Choices;
        public int SelectedChoice { get; private set; } = 0;

        // Required by ISystem<T>
        public bool IsEnabled { get; set; } = true;

        private float _typeTimer = 0f;
        private int _visibleChars = int.MaxValue; // reveal count; int.MaxValue means full
        private const float CharsPerSecond = 60f; // tweak typing speed
        private float _tickAccum = 0f;
        private const float TickInterval = 0.02f; // 50Hz tick

        private bool _loopActive = false;
        private const string LoopKey = "dialogue_type_loop";
        private const string LoopAsset = "Sounds/Effects/typewriter"; // 4s dynamic clip

        public DialogueSystem(World world, DialogueService service, InputActionService input, AudioBus audioBus)
        {
            _world = world;
            _service = service;
            _input = input;
            _audioBus = audioBus;
            _events = world.GetEntities().With<InteractionStarted>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            if (!IsActive)
            {
                // Check for new dialogue to start
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
                            if (_node != null && _service.CheckConditions(_node.Conditions))
                                _service.ApplyActions(_node.Actions);
                            CurrentNpc = target;
                            IsActive = _node != null;
                            SelectedChoice = 0;
                            ResetTypewriter();
                        }
                    }
                    e.Dispose();
                    if (IsActive) break;
                }
                return;
            }

            if (_node == null)
            {
                IsActive = false;
                StopLoopIfActive();
                return;
            }

            // Typewriter update for current text
            UpdateTypewriter(dt);

            var hasChoices = _node.Choices != null && _node.Choices.Count > 0;
            if (hasChoices)
            {
                // Navigate choices with Up/Down, confirm with Interact
                if (_input.IsActionJustPressed("MoveUp"))
                    SelectedChoice = (SelectedChoice - 1 + _node.Choices!.Count) % _node.Choices!.Count;
                if (_input.IsActionJustPressed("MoveDown"))
                    SelectedChoice = (SelectedChoice + 1) % _node.Choices!.Count;
                if (_input.IsActionJustPressed("Advance"))
                {
                    // if not fully revealed, reveal all; else confirm
                    if (!IsFullyRevealed())
                    {
                        RevealAll();
                    }
                    else
                    {
                        var idx = SelectedChoice;
                        if (!_service.CheckConditions(_node.Choices![idx].Conditions))
                        {
                            idx = _node.Choices!.FindIndex(c => _service.CheckConditions(c.Conditions));
                            if (idx < 0) { IsActive = false; return; }
                        }
                        _service.ApplyActions(_node.Choices![idx].Actions);
                        var nextId = _node.Choices![idx].Next;
                        _node = _graph?.Nodes.Find(n => n.Id == nextId);
                        if (_node != null)
                        {
                            if (_service.CheckConditions(_node.Conditions))
                                _service.ApplyActions(_node.Actions);
                            SelectedChoice = 0;
                            ResetTypewriter();
                        }
                        if (_node == null) { IsActive = false; StopLoopIfActive(); }
                    }
                }
            }
            else
            {
                // Linear: advance on Interact
                if (_input.IsActionJustPressed("Advance"))
                {
                    if (!IsFullyRevealed())
                    {
                        RevealAll();
                    }
                    else
                    {
                        var nextId = _node.Next;
                        if (!string.IsNullOrEmpty(nextId))
                        {
                            var n = _graph?.Nodes.Find(n => n.Id == nextId);
                            if (n != null && _service.CheckConditions(n.Conditions))
                            {
                                _service.ApplyActions(n.Actions);
                                _node = n;
                                ResetTypewriter();
                            }
                            else
                            {
                                _node = null;
                            }
                        }
                        else
                        {
                            _node = null;
                        }
                        if (_node == null) { IsActive = false; StopLoopIfActive(); }
                    }
                }
            }
        }

        private void ResetTypewriter()
        {
            _typeTimer = 0f;
            _visibleChars = 0;
            if (!_loopActive)
            {
                _audioBus.Publish(new StartLoop { Asset = LoopAsset, Key = LoopKey, Volume = 0.35f, Pitch = 0f, Pan = 0f });
                _loopActive = true;
            }
        }

        private void UpdateTypewriter(float dt)
        {
            var full = _service.Substitute(_node?.Text ?? string.Empty);
            if (_visibleChars >= full.Length)
            {
                _visibleChars = int.MaxValue;
                if (_loopActive)
                {
                    _audioBus.Publish(new StopLoop { Key = LoopKey, Asset = LoopAsset });
                    _loopActive = false;
                }
                return;
            }
            _typeTimer += dt * CharsPerSecond;
            int add = (int)_typeTimer;
            if (add > 0)
            {
                _visibleChars = Math.Min(full.Length, _visibleChars + add);
                _typeTimer -= add;
            }
        }

        private bool IsFullyRevealed()
        {
            var full = _service.Substitute(_node?.Text ?? string.Empty);
            return _visibleChars >= full.Length || _visibleChars == int.MaxValue;
        }

        private void RevealAll()
        {
            _visibleChars = int.MaxValue;
            if (_loopActive)
            {
                _audioBus.Publish(new StopLoop { Key = LoopKey, Asset = LoopAsset });
                _loopActive = false;
            }
            //_audioBus.Publish(new PlaySfx { Asset = "Audio/type_done", Volume = 0.5f });
        }

        public string? CurrentText
        {
            get
            {
                var full = _service.Substitute(_node?.Text ?? string.Empty);
                if (_visibleChars == int.MaxValue) return full;
                if (_visibleChars <= 0) return string.Empty;
                if (_visibleChars >= full.Length) return full;
                return full[.._visibleChars];
            }
        }

        private void StopLoopIfActive()
        {
            if (_loopActive)
            {
                _audioBus.Publish(new StopLoop { Key = LoopKey, Asset = LoopAsset });
                _loopActive = false;
            }
        }

        public void Dispose() => _events?.Dispose();
    }
}
