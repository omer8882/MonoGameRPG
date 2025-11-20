using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DefaultEcs;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Tilemap;
using System.Linq;
using ANewWorld.Engine.Extensions;
using ANewWorld.Engine.Tilemap.Tmx;
using ANewWorld.Engine.Systems;

namespace ANewWorld.Engine.Debug
{
    public sealed class DebugOverlayService
    {
        private readonly SpriteFont _font;
        private Texture2D? _white;
        public DebugOverlayService(SpriteFont font)
        {
            _font = font;
        }

        private Texture2D GetWhite(GraphicsDevice gd)
        {
            if (_white == null)
            {
                _white = new Texture2D(gd, 1, 1);
                _white.SetData(new[] { Color.White });
            }
            return _white;
        }

        public void Draw(
            SpriteBatch spriteBatch,
            World world,
            float fps,
            CollisionGridService? collisionGrid = null,
            TmxRenderer? tmx = null,
            Vector2? cameraPos = null,
            float cameraZoom = 1f,
            int? visibleEntitiesInView = null,
            int? culledEntities = null,
            InteractionSystem? interaction = null,
            int virtualWidth = 600,
            int virtualHeight = 600)
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
            if (visibleEntitiesInView.HasValue && culledEntities.HasValue)
            {
                debugText += $"\nEnt Visible: {visibleEntitiesInView.Value}, Culled: {culledEntities.Value}";
            }

            if (interaction is not null && interaction.Current is not null && interaction.Current.Value.entity.HasValue)
            {
                var prompt = interaction.Current?.prompt ?? "Interact";
                debugText += $"\n[E] {prompt}";
            }

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: null);
            spriteBatch.DrawString(_font, debugText, new Vector2(16, 16), Color.Yellow);

            // Minimap
            if (tmx != null)
            {
                var gd = spriteBatch.GraphicsDevice;
                var white = GetWhite(gd);
                var miniPos = new Vector2(16, 500);
                var worldW = tmx.Map.Width * tmx.Map.TileWidth;
                var worldH = tmx.Map.Height * tmx.Map.TileHeight;
                float scale = 150f / System.MathF.Max(worldW, worldH);
                var miniRect = new Rectangle((int)miniPos.X, (int)miniPos.Y, (int)(worldW * scale), (int)(worldH * scale));
                spriteBatch.Draw(white, miniRect, new Color(255, 255, 255, 30));

                var p = new Point((int)(miniPos.X + playerPos.X * scale), (int)(miniPos.Y + playerPos.Y * scale));
                spriteBatch.Draw(white, new Rectangle(p.X - 1, p.Y - 1, 3, 3), Color.Lime);

                if (cameraPos.HasValue)
                {
                    float viewW = virtualWidth / cameraZoom;
                    float viewH = virtualHeight / cameraZoom;
                    var camLeft = cameraPos.Value.X - viewW / 2f;
                    var camTop = cameraPos.Value.Y - viewH / 2f;
                    var camRect = new Rectangle(
                        (int)(miniPos.X + camLeft * scale),
                        (int)(miniPos.Y + camTop * scale),
                        (int)(viewW * scale),
                        (int)(viewH * scale));
                    DrawRect(spriteBatch, white, camRect, Color.Red);
                }

                var culled = tmx.LastVisibleWorld;
                var culledRect = new Rectangle(
                    (int)(miniPos.X + culled.X * scale),
                    (int)(miniPos.Y + culled.Y * scale),
                    (int)(culled.Width * scale),
                    (int)(culled.Height * scale));
                DrawRect(spriteBatch, white, culledRect, Color.Cyan);
            }

            spriteBatch.End();
        }

        private static void DrawRect(SpriteBatch sb, Texture2D white, Rectangle r, Color c)
        {
            sb.Draw(white, new Rectangle(r.Left, r.Top, r.Width, 1), c);
            sb.Draw(white, new Rectangle(r.Left, r.Bottom - 1, r.Width, 1), c);
            sb.Draw(white, new Rectangle(r.Left, r.Top, 1, r.Height), c);
            sb.Draw(white, new Rectangle(r.Right - 1, r.Top, 1, r.Height), c);
        }
    }
}
