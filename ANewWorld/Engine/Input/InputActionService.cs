using System;
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
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;
        private GamePadState _previousPadState;
        private GamePadState _currentPadState;
        private const int MouseWheelTick = 120;

        public bool OverlayActive { get; private set; } = true;

        public InputActionService(string path)
        {
            _path = path;
            LoadBindings();
            _previousState = Keyboard.GetState();
            _currentState = _previousState;
            _previousMouseState = Mouse.GetState();
            _currentMouseState = _previousMouseState;
            _previousPadState = GamePad.GetState(PlayerIndex.One);
            _currentPadState = _previousPadState;
        }

        public void LoadBindings()
        {
            _bindings = ContentLoader.LoadJson<Dictionary<string, string[]>>(_path);
        }

        // Call at start of frame
        public void Update()
        {
            _currentState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
            _currentPadState = GamePad.GetState(PlayerIndex.One);

            if (IsActionJustPressed("ToggleOverlay"))
                OverlayActive = !OverlayActive;
        }

        // Call once at end of frame
        public void EndFrame()
        {
            _previousState = _currentState;
            _previousMouseState = _currentMouseState;
            _previousPadState = _currentPadState;
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

        public int GetMouseWheelSteps()
        {
            var delta = _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (delta == 0)
                return 0;

            var steps = delta / MouseWheelTick;
            if (steps == 0)
                steps = Math.Sign(delta);

            return steps;
        }

        public int GetDPadHorizontalStep()
        {
            int step = 0;
            if (_currentPadState.DPad.Left == ButtonState.Pressed && _previousPadState.DPad.Left == ButtonState.Released)
                step--;
            if (_currentPadState.DPad.Right == ButtonState.Pressed && _previousPadState.DPad.Right == ButtonState.Released)
                step++;
            return Math.Clamp(step, -1, 1);
        }

        public int GetDPadVerticalStep()
        {
            int step = 0;
            if (_currentPadState.DPad.Up == ButtonState.Pressed && _previousPadState.DPad.Up == ButtonState.Released)
                step--;
            if (_currentPadState.DPad.Down == ButtonState.Pressed && _previousPadState.DPad.Down == ButtonState.Released)
                step++;
            return Math.Clamp(step, -1, 1);
        }
    }
}
