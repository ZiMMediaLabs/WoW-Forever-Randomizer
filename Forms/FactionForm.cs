using System;
using System.Drawing;
using System.Windows.Forms;
using WoWForeverRandomizer.UI;

namespace WoWForeverRandomizer.Forms
{
    // Faction select: Alliance, Horde or Either. Clicking a tile starts the roll.
    public partial class FactionForm : WowFormBase
    {
        public FactionForm()
        {
            InitializeComponent();
            SetBackground("Images/bg_faction.png");
        }

        // All three tiles share this handler - the tile itself knows which faction it is.
        private void FactionTile_Click(object sender, EventArgs e)
        {
            var tile = (FactionTile)sender;
            Navigator.GoTo(this, new ResultForm(tile.Choice));
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Navigator.GoTo(this, new MainForm());
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                BackButton_Click(this, EventArgs.Empty);
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

            WowPainter.DrawGoldText(g, "CHOOSE YOUR FACTION", Assets.BodyFont, 23f, new RectangleF(0, 16, ClientSize.Width, 34));
            WowPainter.DrawDivider(g, ClientSize.Width / 2, 54, 300);
        }
    }
}
