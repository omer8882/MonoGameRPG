using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Input
{
    public sealed class InputActionService
    {
        private readonly string _path;
        private Dictionary<string, string[]> _bindings = [];
        private KeyboardState _previousState;
        private KeyboardState _currentState;

        public bool OverlayActive { get; private set; } = true;

        public InputActionService(string path)
        {
            _path = path;
            LoadBindings();
            _previousState = Keyboard.GetState();
            _currentState = _previousState;
        }

        public void LoadBindings()
        {
            _bindings = ContentLoader.LoadJson<Dictionary<string, string[]>>(_path);
        }

        // Call at start of frame
        public void Update()
        {
            _currentState = Keyboard.GetState();

            if (IsActionJustPressed("ToggleOverlay"))
                OverlayActive = !OverlayActive;
        }

        // Call once at end of frame
        public void EndFrame()
        {
            _previousState = _currentState;
        }

        public bool IsActionJustPressed(string action)
        {
            if (!_bindings.TryGetValue(action, out var keys)) return false;
            foreach (var k in keys)
            {
                if (System.Enum.TryParse<Keys>(k, out var key))
                {
                    if (_currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key))
                        return true;
                }
            }
            return false;
        }

        public bool IsActionActive(string action)
        {
            if (!_bindings.TryGetValue(action, out var keys)) return false;
            foreach (var k in keys)
            {
                if (System.Enum.TryParse<Keys>(k, out var key))
                {
                    if (_currentState.IsKeyDown(key))
                        return true;
                }
            }
            return false;
        }
    }
}
