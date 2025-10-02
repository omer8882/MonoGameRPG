using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Systems;

namespace ANewWorld.Engine.UI
{
    public sealed class DialogueHud
    {
        private readonly SpriteFont _font;
        private Texture2D? _white;
        public DialogueHud(SpriteFont font)
        {
            _font = font;
        }

        private Texture2D GetWhite(GraphicsDevice gd)
        {
            if (_white == null)
            {
                _white = new Texture2D(gd, 1, 1);
                _white.SetData([Color.White]);
            }
            return _white;
        }

        public void Draw(SpriteBatch spriteBatch, DialogueSystem dialogue)
        {
            if (!dialogue.IsActive) return;
            var gd = spriteBatch.GraphicsDevice;
            var white = GetWhite(gd);
            var panel = new Rectangle(16, gd.Viewport.Height - 140, gd.Viewport.Width - 32, 124);
            spriteBatch.Draw(white, panel, new Color(0, 0, 0, 200));

            var text = dialogue.CurrentText ?? string.Empty;
            spriteBatch.DrawString(_font, text, new Vector2(panel.X + 12, panel.Y + 12), Color.White);

            var choices = dialogue.CurrentChoices;
            if (choices != null && choices.Count > 0)
            {
                int y = panel.Y + 60;
                for (int i = 0; i < choices.Count; i++)
                {
                    var c = choices[i];
                    var color = (i == dialogue.SelectedChoice) ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(_font, $"> {c.Text}", new Vector2(panel.X + 12, y), color);
                    y += 22;
                }
            }
            else
            {
                spriteBatch.DrawString(_font, "[E] Continue", new Vector2(panel.Right - 140, panel.Bottom - 24), Color.Gray);
            }
        }
    }
}
