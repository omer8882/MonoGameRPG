using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Components
{
    public struct Velocity
    {
        public Vector2 Value;

        public override string ToString() => Value.ToString();
    }
}