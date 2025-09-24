using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ANewWorld.Engine.Components
{
    public struct SpriteComponent
    {
        public Texture2D Texture;
        public Rectangle? SourceRect;
        public Color Color;
        public Vector2 Origin;
    }
}