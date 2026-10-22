using System.Drawing;
using System.Windows.Forms;

namespace WoWForeverRandomizer.UI
{
    // Base form for every screen: fixed 384 x 661 size, app icon, double buffering and a background image.
    public class WowFormBase : Form
    {
        public WowFormBase()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(384, 661);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.Stretch;
            KeyPreview = true;

            if (!DesignMode && LicenseManagerUsageIsRuntime())
            {
                Icon = Assets.GetAppIcon();
            }
        }

        protected void SetBackground(string imagePath)
        {
            BackgroundImage = Assets.GetImage(imagePath);
            Invalidate();
        }

        // DesignMode is not yet set inside a constructor, so check the license manager instead.
        private static bool LicenseManagerUsageIsRuntime()
        {
            return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Runtime;
        }
    }
}
