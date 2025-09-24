using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Text.Json;

namespace ANewWorld.Engine.Input
{
    public sealed class InputActionService
    {
        private readonly string _path;
        private Dictionary<string, string[]> _bindings = [];
        private KeyboardState _previousState;

        public bool OverlayActive { get; private set; } = true;

        public InputActionService(string path)
        {
            _path = path;
            LoadBindings();
            _previousState = Keyboard.GetState();
        }

        public void LoadBindings()
        {
            if (!File.Exists(_path)) return;
            var json = File.ReadAllText(_path);
            _bindings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? new Dictionary<string, string[]>();
        }

        public void Update()
        {
            var ks = Keyboard.GetState();

            if (IsActionPressed("ToggleOverlay", ks))
            {
                OverlayActive = !OverlayActive;
            }

            _previousState = ks;
        }

        private bool IsActionPressed(string action, KeyboardState ks)
        {
            if (!_bindings.TryGetValue(action, out var keys)) return false;
            foreach (var k in keys)
            {
                if (System.Enum.TryParse<Keys>(k, out var key))
                {
                    if (ks.IsKeyDown(key) && !_previousState.IsKeyDown(key)) 
                        return true;
                }
            }
            return false;
        }

        public bool IsActionActive(string action)
        {
            var ks = Keyboard.GetState();
            if (!_bindings.TryGetValue(action, out var keys)) return false;
            foreach (var k in keys)
            {
                if (System.Enum.TryParse<Keys>(k, out var key))
                {
                    if (ks.IsKeyDown(key)) 
                        return true;
                }
            }
            return false;
        }
    }
}
