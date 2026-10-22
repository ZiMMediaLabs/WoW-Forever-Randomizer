using System;
using System.Drawing;
using System.Windows.Forms;
using WoWForeverRandomizer.UI;

namespace WoWForeverRandomizer.Forms
{
    // Title screen
    public partial class MainForm : WowFormBase
    {
        public MainForm()
        {
            InitializeComponent();
            SetBackground("Images/bg_title.png");
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Navigator.GoTo(this, new FactionForm());
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Enter starts the app, just like clicking START.
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                StartButton_Click(this, EventArgs.Empty);
            }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DesignMode)
            {
                return;
            }

            Graphics g = e.Graphics;
            WowPainter.SetHighQuality(g);
            int centerX = ClientSize.Width / 2;

            // World of Warcraft logo
            Image logo = Assets.GetImage("Images/wow_logo.png");
            int logoWidth = 350;
            int logoHeight = logo.Height * logoWidth / logo.Width;
            g.DrawImage(logo, new Rectangle(centerX - logoWidth / 2, 34, logoWidth, logoHeight));

            int y = 34 + logoHeight;

            // "FOREVER" sub-logo
            WowPainter.DrawText(g, "FOREVER", Assets.TitleFont, 46f, new RectangleF(0, y - 8, ClientSize.Width, 60),
                Color.FromArgb(214, 238, 255), Color.FromArgb(70, 150, 230), 4f);
            WowPainter.DrawDivider(g, centerX, y + 56, 260);

            // App title
            WowPainter.DrawGoldText(g, "RACE & CLASS", Assets.BodyFont, 25f, new RectangleF(0, y + 64, ClientSize.Width, 34));
            WowPainter.DrawGoldText(g, "RANDOMIZER", Assets.BodyFont, 18f, new RectangleF(0, y + 96, ClientSize.Width, 28), 2.5f);

            // Dice (the character-creation "randomize" button art)
            Image dice = Assets.GetImage("Images/dice.png");
            g.DrawImage(dice, new Rectangle(centerX - 40, StartButton.Top - 74, 80, 54));

            // Footer
            WowPainter.DrawText(g, "ZiMMedia Labs", Assets.BodyFont, 11f, new RectangleF(0, ClientSize.Height - 34, ClientSize.Width, 20),
                WowPainter.Parchment, Color.FromArgb(170, 160, 140), 1.5f);
        }
    }
}
