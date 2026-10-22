using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WoWForeverRandomizer.UI
{
    // Red-and-gold WoW-style button with normal, hover, pressed and disabled states.
    public class GoldButton : Control
    {
        private bool _isHovered;
        private bool _isPressed;
        private bool _isKeyPressed;

        public GoldButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.SupportsTransparentBackColor
                     | ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(200, 48);
            TabStop = true;
        }

        // Off for buttons that play their own sound (Reroll)
        [Category("Behavior"), DefaultValue(true)]
        public bool PlayClickSound { get; set; } = true;

        [Category("Appearance"), DefaultValue(19f)]
        public float TextSizePx { get; set; } = 19f;

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            if (PlayClickSound)
            {
                Assets.PlaySound("click.wav");
            }
            base.OnClick(e);
        }

        // Let Enter / Space press the button when it has keyboard focus.
        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                _isKeyPressed = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            // Only "click" if the key went down on this button too (not on the previous screen).
            if (_isKeyPressed && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                _isKeyPressed = false;
                OnClick(EventArgs.Empty);
            }

            base.OnKeyUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            WowPainter.SetHighQuality(g);
            var bounds = new Rectangle(0, 0, Width, Height);

            if (DesignMode)
            {
                g.FillRectangle(Brushes.DarkRed, bounds);
                TextRenderer.DrawText(g, Text, Font, bounds, Color.Gold);
                return;
            }

            if (!Enabled)
            {
                WowPainter.DrawImageFaded(g, Assets.GetImage("Images/btn_up.png"), bounds, 0.55f);
            }
            else
            {
                string imagePath = _isPressed ? "Images/btn_down.png"
                                 : _isHovered ? "Images/btn_hover.png"
                                 : "Images/btn_up.png";
                g.DrawImage(Assets.GetImage(imagePath), bounds);
            }

            // Text sits slightly lower when pressed, like the real button.
            int offset = _isPressed ? 2 : 0;
            var textRect = new RectangleF(4, offset - 1, Width - 8, Height);
            Color top = Enabled ? (_isHovered ? Color.White : WowPainter.GoldLight) : Color.FromArgb(150, 150, 150);
            Color bottom = Enabled ? WowPainter.Gold : Color.FromArgb(100, 100, 100);
            WowPainter.DrawText(g, Text, Assets.BodyFont, TextSizePx, textRect, top, bottom, 2f);

            if (Focused && ShowFocusCues)
            {
                using (var pen = new Pen(Color.FromArgb(120, WowPainter.GoldLight)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                {
                    g.DrawRectangle(pen, 12, 7, Width - 25, Height - 15);
                }
            }
        }
    }
}
