using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace WoWForeverRandomizer.UI
{
    // Drawing helpers for the WoW look: outlined gradient text, framed panels and ringed portraits.
    public static class WowPainter
    {
        public static readonly Color GoldLight = Color.FromArgb(255, 236, 160);
        public static readonly Color Gold = Color.FromArgb(255, 209, 0);
        public static readonly Color GoldDark = Color.FromArgb(200, 140, 20);
        public static readonly Color FrameDark = Color.FromArgb(120, 86, 30);
        public static readonly Color FrameLight = Color.FromArgb(232, 190, 90);
        public static readonly Color Parchment = Color.FromArgb(235, 225, 200);

        public static void SetHighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        }

        // Draws outlined text filled with a vertical gradient (top color to bottom color).
        public static void DrawText(Graphics g, string text, FontFamily family, float sizePx, RectangleF bounds,
            Color top, Color bottom, float outlineWidth = 3f, StringAlignment alignment = StringAlignment.Center)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            using (var path = new GraphicsPath())
            using (var format = new StringFormat())
            {
                format.Alignment = alignment;
                format.LineAlignment = StringAlignment.Center;
                path.AddString(text, family, (int)Assets.StyleFor(family), sizePx, bounds, format);

                // Shrink text that is too wide for its box (e.g. long race names).
                RectangleF measured = path.GetBounds();
                if (measured.Width > bounds.Width - 8 && measured.Width > 0)
                {
                    float scale = (bounds.Width - 8) / measured.Width;
                    path.Reset();
                    path.AddString(text, family, (int)Assets.StyleFor(family), sizePx * scale, bounds, format);
                    measured = path.GetBounds();
                }

                if (outlineWidth > 0)
                {
                    using (var outline = new Pen(Color.FromArgb(230, 10, 6, 2), outlineWidth * 2) { LineJoin = LineJoin.Round })
                    {
                        g.DrawPath(outline, path);
                    }
                }

                RectangleF gradientRect = new RectangleF(measured.X, measured.Y - 1, Math.Max(1, measured.Width), Math.Max(2, measured.Height + 2));
                using (var fill = new LinearGradientBrush(gradientRect, top, bottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(fill, path);
                }
            }
        }

        public static void DrawGoldText(Graphics g, string text, FontFamily family, float sizePx, RectangleF bounds, float outlineWidth = 3f)
        {
            DrawText(g, text, family, sizePx, bounds, GoldLight, GoldDark, outlineWidth);
        }

        // Semi-transparent dark panel with a double gold border and corner studs.
        public static void DrawPanel(Graphics g, Rectangle rect, int fillAlpha = 190)
        {
            using (var fill = new LinearGradientBrush(rect, Color.FromArgb(fillAlpha, 20, 14, 8), Color.FromArgb(fillAlpha, 6, 4, 2), LinearGradientMode.Vertical))
            {
                g.FillRectangle(fill, rect);
            }

            using (var outer = new Pen(FrameDark, 3))
            using (var inner = new Pen(FrameLight, 1))
            {
                g.DrawRectangle(outer, rect);
                g.DrawRectangle(inner, Rectangle.Inflate(rect, -3, -3));
            }

            using (var stud = new SolidBrush(FrameLight))
            using (var studEdge = new Pen(FrameDark, 1))
            {
                foreach (Point corner in new[]
                {
                    new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top),
                    new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom)
                })
                {
                    var diamond = new[]
                    {
                        new Point(corner.X, corner.Y - 6), new Point(corner.X + 6, corner.Y),
                        new Point(corner.X, corner.Y + 6), new Point(corner.X - 6, corner.Y)
                    };
                    g.FillPolygon(stud, diamond);
                    g.DrawPolygon(studEdge, diamond);
                }
            }
        }

        // Horizontal gold divider line that fades out at both ends.
        public static void DrawDivider(Graphics g, int centerX, int y, int width)
        {
            var rect = new Rectangle(centerX - width / 2, y, width, 2);
            using (var brush = new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(0, FrameLight), FrameLight, Color.FromArgb(0, FrameLight) },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, rect);
            }
        }

        // Draws an image clipped to a circle with a thick gold ring (race portraits).
        public static void DrawRingedPortrait(Graphics g, Image image, Rectangle rect, Color glowColor, float glowStrength)
        {
            DrawGlow(g, rect, glowColor, glowStrength, true);

            using (var clip = new GraphicsPath())
            {
                clip.AddEllipse(rect);
                GraphicsState state = g.Save();
                g.SetClip(clip);
                // The WoW portrait icons have a thin square border - zoom past it.
                var source = new Rectangle(4, 4, image.Width - 8, image.Height - 8);
                g.DrawImage(image, rect, source, GraphicsUnit.Pixel);
                g.Restore(state);
            }

            using (var outer = new Pen(FrameDark, 7))
            using (var ring = new Pen(FrameLight, 3))
            {
                g.DrawEllipse(outer, rect);
                g.DrawEllipse(ring, rect);
            }
        }

        // Draws a square icon inside a gold frame (class icons).
        public static void DrawFramedIcon(Graphics g, Image image, Rectangle rect, Color glowColor, float glowStrength, float opacity = 1f)
        {
            DrawGlow(g, rect, glowColor, glowStrength, false);

            if (opacity >= 1f)
            {
                g.DrawImage(image, rect);
            }
            else
            {
                DrawImageFaded(g, image, rect, opacity);
            }

            using (var outer = new Pen(FrameDark, 4))
            using (var inner = new Pen(FrameLight, 1.5f))
            {
                g.DrawRectangle(outer, rect);
                g.DrawRectangle(inner, Rectangle.Inflate(rect, 1, 1));
            }
        }

        // Draws an image at reduced opacity and saturation (used for "not chosen" icons).
        public static void DrawImageFaded(Graphics g, Image image, Rectangle rect, float opacity, float gray = 0.6f)
        {
            // gray = how much color to remove (0 = full color, 1 = black & white)
            float r = 0.299f * gray, gr = 0.587f * gray, b = 0.114f * gray, keep = 1 - gray;
            var matrix = new ColorMatrix(new[]
            {
                new[] { keep + r, r, r, 0, 0 },
                new[] { gr, keep + gr, gr, 0, 0 },
                new[] { b, b, keep + b, 0, 0 },
                new[] { 0f, 0, 0, opacity, 0 },
                new[] { 0f, 0, 0, 0, 1 }
            });

            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(matrix);
                g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        // Soft glow behind an element; strength 0..1.
        public static void DrawGlow(Graphics g, Rectangle rect, Color color, float strength, bool round)
        {
            if (strength <= 0.01f)
            {
                return;
            }

            for (int i = 14; i > 0; i -= 2)
            {
                int alpha = (int)(strength * 18 * (15 - i) / 7f);
                alpha = Math.Max(0, Math.Min(255, alpha));
                Rectangle glowRect = Rectangle.Inflate(rect, i, i);
                using (var brush = new SolidBrush(Color.FromArgb(alpha, color)))
                {
                    if (round)
                    {
                        g.FillEllipse(brush, glowRect);
                    }
                    else
                    {
                        g.FillRectangle(brush, glowRect);
                    }
                }
            }
        }
    }
}
