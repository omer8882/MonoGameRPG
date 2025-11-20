using ANewWorld.Engine.Components;
using ANewWorld.Engine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;

namespace ANewWorld.Engine.UI
{
    /// <summary>
    /// Renders interaction prompts above interactable entities
    /// </summary>
    public sealed class InteractionPromptRenderer
    {
        private Texture2D? _promptIcon;
        private Rectangle _sourceRect;
        private int size = 16;
        private readonly float _bobSpeed = 3f;
        private readonly float _bobAmount = 3f;
        private float _bobTimer = 0f;

        public void LoadContent(ContentManager content)
        {
            // Create a simple speech bubble icon (16x16 white rounded rectangle)
            //_promptIcon = CreatePromptTexture(graphicsDevice);
            _promptIcon = content.Load<Texture2D>("UI/GameplayUI");
            _sourceRect = new Rectangle(0, 0, size, size);
        }

        public void Update(float dt)
        {
            _bobTimer += dt * _bobSpeed;
        }

        public void Draw(SpriteBatch spriteBatch, InteractionSystem? interactionSystem, float worldOffsetY = -20f)
        {
            if (interactionSystem == null || _promptIcon == null) return;
            
            if (!interactionSystem.Current.HasValue || !interactionSystem.Current.Value.entity.HasValue) return;
            var interactableEntity = interactionSystem.Current;

            // Calculate bob offset
            float bobOffset = (float)Math.Sin(_bobTimer) * _bobAmount;

            var wh = interactableEntity!.Value.entity?.Get<SpriteComponent>().SourceRect?.Width ?? 0;
            var half = wh / 2;

            // Draw above the entity
            var position = interactableEntity!.Value.position;
            position.Y += bobOffset;


            // Draw centered above entity
            var iconRect = new Rectangle(
                (int)(position.X - 8),
                (int)(position.Y - half - 8),
                size,
                size
            );

            spriteBatch.Draw(_promptIcon, iconRect, _sourceRect, Color.White);
        }

        private Texture2D CreatePromptTexture(GraphicsDevice graphicsDevice)
        {
            // Create 16x16 speech bubble icon
            int size = 16;
            var texture = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];

            // Simple rounded rectangle (speech bubble shape)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    
                    // Border (white)
                    bool isBorder = x == 0 || x == size - 1 || y == 0 || y == size - 1 ||
                                   (y == 1 && (x < 2 || x > size - 3)) ||
                                   (y == size - 2 && (x < 2 || x > size - 3));
                    
                    // Fill (semi-transparent white)
                    bool isFill = x > 0 && x < size - 1 && y > 0 && y < size - 1;
                    
                    if (isBorder)
                        data[index] = Color.White;
                    else if (isFill)
                        data[index] = new Color(255, 255, 255, 200);
                    else
                        data[index] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}
