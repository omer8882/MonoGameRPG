using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace ANewWorld.Engine.Tilemap
{
    public sealed class TilemapService
    {
        public TiledMap Map { get; }
        private readonly TiledMapRenderer _renderer;

        public TilemapService(GraphicsDevice graphicsDevice, TiledMap map)
        {
            Map = map;
            _renderer = new TiledMapRenderer(graphicsDevice, map);
        }

        public void Update(GameTime gameTime)
        {
            _renderer.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix? transformMatrix = null)
        {
            _renderer.Draw(transformMatrix ?? Matrix.Identity);
        }
    }
}
