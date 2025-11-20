using System;
using System.Globalization;
using ANewWorld.Engine.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ANewWorld.Engine.UI
{
    public sealed class InventoryHud : IDisposable
    {
        private readonly SpriteFont _font;
        private readonly ItemIconCache _iconCache;
        private Texture2D? _whitePixel;

        public int SlotSize { get; set; } = 48;
        public int SlotPadding { get; set; } = 6;
        public int SlotSpacing { get; set; } = 10;
        public int Margin { get; set; } = 20;
        public int MaxSlots { get; set; } = 10;

        public InventoryHud(SpriteFont font, ItemIconCache iconCache)
        {
            _font = font;
            _iconCache = iconCache;
        }

        public void Draw(SpriteBatch spriteBatch, InventoryComponent inventory, ItemService itemService, int viewportWidth, int viewportHeight)
        {
            if (inventory.SlotOrder.Count == 0) return;

            var white = GetWhite(spriteBatch.GraphicsDevice);
            var slotsToDraw = Math.Max(MaxSlots, inventory.SlotOrder.Count);
            var totalWidth = slotsToDraw * SlotSize + (slotsToDraw - 1) * SlotSpacing;
            var startX = (viewportWidth - totalWidth) / 2;
            var y = viewportHeight - SlotSize - Margin;

            for (int i = 0; i < slotsToDraw; i++)
            {
                var slotRect = new Rectangle(startX + i * (SlotSize + SlotSpacing), y, SlotSize, SlotSize);
                DrawSlotBackground(spriteBatch, white, slotRect, i == inventory.SelectedIndex);

                var keyLabel = GetSlotKeyLabel(i);
                if (!string.IsNullOrEmpty(keyLabel))
                {
                    spriteBatch.DrawString(_font, keyLabel, new Vector2(slotRect.X + 4, slotRect.Y + 2), Color.Gray);
                }

                if (i >= inventory.SlotOrder.Count)
                    continue;

                var itemId = inventory.SlotOrder[i];
                if (!inventory.Stacks.TryGetValue(itemId, out var stack))
                    continue;

                if (!itemService.TryGetDefinition(itemId, out var definition))
                    continue;

                var texture = _iconCache.GetIcon(definition);
                var iconSize = SlotSize - SlotPadding * 2;
                var iconRect = new Rectangle(
                    slotRect.X + SlotPadding,
                    slotRect.Y + SlotPadding + 4,
                    iconSize,
                    iconSize - 8);
                spriteBatch.Draw(texture, iconRect, Color.White);

                if (stack.Quantity > 1)
                {
                    var qty = stack.Quantity.ToString(CultureInfo.InvariantCulture);
                    var size = _font.MeasureString(qty);
                    var qtyPos = new Vector2(slotRect.Right - size.X - 6, slotRect.Bottom - size.Y - 4);
                    spriteBatch.DrawString(_font, qty, qtyPos, Color.White);
                }
            }
        }

        private void DrawSlotBackground(SpriteBatch spriteBatch, Texture2D white, Rectangle rect, bool selected)
        {
            var bgColor = selected ? new Color(25, 30, 60, 220) : new Color(15, 15, 25, 200);
            var borderColor = selected ? new Color(110, 180, 255) : new Color(80, 80, 110);
            spriteBatch.Draw(white, rect, bgColor);
            DrawBorder(spriteBatch, white, rect, borderColor);
        }

        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D white, Rectangle rect, Color color)
        {
            spriteBatch.Draw(white, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
            spriteBatch.Draw(white, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
            spriteBatch.Draw(white, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
            spriteBatch.Draw(white, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
        }

        private static string? GetSlotKeyLabel(int index) => (index+1).ToString();

        private Texture2D GetWhite(GraphicsDevice device)
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(device, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }

            return _whitePixel;
        }

        public void Dispose()
        {
            _whitePixel?.Dispose();
            _whitePixel = null;
        }
    }
}
