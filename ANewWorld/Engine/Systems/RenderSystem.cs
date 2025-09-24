using DefaultEcs;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Components;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class RenderSystem
    {
        private readonly World _world;
        private readonly SpriteBatch _spriteBatch;

        public RenderSystem(World world, SpriteBatch spriteBatch)
        {
            _world = world;
            _spriteBatch = spriteBatch;
        }

        public void Update(float dt)
        {
            var set = _world.GetEntities().With<Transform>().With<SpriteComponent>().AsSet();
            foreach (var entity in set.GetEntities())
            {
                var t = entity.Get<Transform>();
                var s = entity.Get<SpriteComponent>();
                _spriteBatch.Draw(s.Texture, t.Position, s.SourceRect, s.Color, t.Rotation, s.Origin, t.Scale, SpriteEffects.None, 0f);
            }
        }
    }
}