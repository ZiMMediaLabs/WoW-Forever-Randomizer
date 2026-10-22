using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WoWForeverRandomizer.Data;

namespace WoWForeverRandomizer.UI
{
    // Clickable faction banner (Alliance / Horde / Either) that glows gold on hover.
    public class FactionTile : Control
    {
        private bool _isHovered;
        private FactionChoice _choice;

        public FactionTile()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.SupportsTransparentBackColor
                     | ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(324, 150);
        }

        [Category("Behavior"), DefaultValue(FactionChoice.Alliance)]
        public FactionChoice Choice
        {
            get { return _choice; }
            set
            {
                _choice = value;
                Invalidate();
            }
        }

        private string ImagePath
        {
            get
            {
                switch (_choice)
                {
                    case FactionChoice.Horde:
                        return "Images/tile_horde.png";
                    case FactionChoice.Either:
                        return "Images/tile_any.png";
                    default:
                        return "Images/tile_alliance.png";
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            WowPainter.SetHighQuality(g);

            // Leave an 8px margin around the art so there is room for the hover glow.
            var art = new Rectangle(8, 8, Width - 16, Height - 16);

            if (DesignMode)
            {
                g.FillRectangle(Brushes.DimGray, art);
                TextRenderer.DrawText(g, _choice.ToString(), Font, art, Color.Gold);
                return;
            }

            if (_isHovered)
            {
                WowPainter.DrawGlow(g, art, WowPainter.Gold, 1f, false);
                g.DrawImage(Assets.GetImage(ImagePath), art);
                using (var pen = new Pen(WowPainter.Gold, 2))
                {
                    g.DrawRectangle(pen, Rectangle.Inflate(art, 1, 1));
                }
            }
            else
            {
                // Dimmed until hovered
                WowPainter.DrawImageFaded(g, Assets.GetImage(ImagePath), art, 1f, 0.3f);
            }
        }
    }
}
