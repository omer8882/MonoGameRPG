using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DefaultEcs;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Tilemap;
using System.Linq;
using ANewWorld.Engine.Extensions;

namespace ANewWorld.Engine.Debug
{
    public sealed class DebugOverlayService
    {
        private readonly SpriteFont _font;
        public DebugOverlayService(SpriteFont font)
        {
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch, World world, float fps, CollisionGridService? collisionGrid = null)
        {
            int entityCount = world.GetEntities().AsSet().Count;
            Vector2 playerPos = Vector2.Zero;
            int tileX = 0, tileY = 0;
            bool blocked = false;
            var player = world.GetPlayer();
            playerPos = player.Get<Transform>().Position;
            if (collisionGrid != null)
            {
                tileX = (int)(playerPos.X / collisionGrid.TileWidth);
                tileY = (int)(playerPos.Y / collisionGrid.TileHeight);
                blocked = collisionGrid.IsBlocked(playerPos.X, playerPos.Y);
            }
            string debugText = $"Entities: {entityCount}\n" +
                               $"FPS: {fps:F1}\n" +
                               $"Player Pos: {playerPos.X:F1}, {playerPos.Y:F1}\n" +
                               $"Player Tile: {tileX}, {tileY}, Blocked: {blocked}";
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: null);
            spriteBatch.DrawString(_font, debugText, new Vector2(16, 16), Color.Yellow);
            spriteBatch.End();
        }
    }
}
