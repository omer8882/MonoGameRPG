using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DefaultEcs;
using ANewWorld.Engine.Components;

namespace ANewWorld.Engine.Debug
{
    public sealed class DebugOverlayService
    {
        private readonly SpriteFont _font;
        public DebugOverlayService(SpriteFont font)
        {
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch, World ecsWorld, float fps)
        {
            int entityCount = ecsWorld.GetEntities().AsSet().Count;
            Vector2 playerPos = Vector2.Zero;
            var set = ecsWorld.GetEntities().With<Transform>().With<Velocity>().AsSet();
            foreach (var entity in set.GetEntities())
            {
                playerPos = entity.Get<Transform>().Position;
                break;
            }
            string debugText = $"Entities: {entityCount}\n" +
                               $"FPS: {fps:F1}\n" +
                               $"Player Pos: {playerPos.X:F1}, {playerPos.Y:F1}";
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: null);
            spriteBatch.DrawString(_font, debugText, new Vector2(16, 16), Color.Yellow);
            spriteBatch.End();
        }
    }
}
