using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ANewWorld.Engine.Systems;
using System.Collections.Generic;

namespace ANewWorld.Engine.UI
{
    public sealed class DialogueHud
    {
        private readonly SpriteFont _font;
        private Texture2D? _white;
        public int VirtualWidth { get; set; } = 600;
        public int VirtualHeight { get; set; } = 600;

        private readonly List<string> _wrapLines = new(16);

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

            // Panel in virtual coordinates
            var panel = new Rectangle(12, VirtualHeight - 140, VirtualWidth - 24, 128);
            // Shadow
            spriteBatch.Draw(white, new Rectangle(panel.X + 3, panel.Y + 3, panel.Width, panel.Height), new Color(0, 0, 0, 100));
            // Panel background
            spriteBatch.Draw(white, panel, new Color(20, 20, 30, 210));
            // Border
            DrawRect(spriteBatch, white, panel, new Color(200, 200, 220, 180));

            // Name box
            var name = dialogue.NpcName ?? "";
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameSize = _font.MeasureString(name);
                var nameRect = new Rectangle(panel.X + 12, panel.Y - 24, (int)nameSize.X + 14, 22);
                spriteBatch.Draw(white, nameRect, new Color(20, 20, 30, 230));
                DrawRect(spriteBatch, white, nameRect, new Color(200, 200, 220, 180));
                spriteBatch.DrawString(_font, name, new Vector2(nameRect.X + 7, nameRect.Y + 2), Color.White);
            }

            // Text area with wrapping
            var pad = new Point(16, 12);
            var textArea = new Rectangle(panel.X + pad.X, panel.Y + pad.Y, panel.Width - pad.X * 2, 60);
            var text = dialogue.CurrentText ?? string.Empty;
            WrapText(text, textArea.Width);
            int ty = textArea.Y;
            for (int i = 0; i < _wrapLines.Count; i++)
            {
                spriteBatch.DrawString(_font, _wrapLines[i], new Vector2(textArea.X, ty), Color.White);
                ty += (int)_font.LineSpacing;
            }

            // Choices
            var choices = dialogue.CurrentChoices;
            if (choices != null && choices.Count > 0)
            {
                int y = panel.Bottom - 24 - (choices.Count * (int)_font.LineSpacing);
                for (int i = 0; i < choices.Count; i++)
                {
                    var c = choices[i];
                    var color = (i == dialogue.SelectedChoice) ? Color.Yellow : Color.LightGray;
                    spriteBatch.DrawString(_font, $"> {c.Text}", new Vector2(panel.X + pad.X, y), color);
                    y += (int)_font.LineSpacing;
                }
            }
            else
            {
                var hint = "[Space/Enter] Continue";
                var size = _font.MeasureString(hint);
                spriteBatch.DrawString(_font, hint, new Vector2(panel.Right - pad.X - size.X, panel.Bottom - pad.Y - size.Y), Color.Gray);
            }
        }

        private void WrapText(string text, int maxWidth)
        {
            _wrapLines.Clear();
            if (string.IsNullOrEmpty(text)) return;
            var words = text.Split(' ');
            string line = "";
            foreach (var w in words)
            {
                var test = string.IsNullOrEmpty(line) ? w : line + " " + w;
                if (_font.MeasureString(test).X <= maxWidth)
                {
                    line = test;
                }
                else
                {
                    if (!string.IsNullOrEmpty(line))
                        _wrapLines.Add(line);
                    line = w;
                }
            }
            if (!string.IsNullOrEmpty(line))
                _wrapLines.Add(line);
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
