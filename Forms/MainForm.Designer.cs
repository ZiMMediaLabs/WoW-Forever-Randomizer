namespace WoWForeverRandomizer.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.StartButton = new WoWForeverRandomizer.UI.GoldButton();
            this.SuspendLayout();
            //
            // StartButton
            //
            this.StartButton.Location = new System.Drawing.Point(92, 536);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(200, 48);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "START";
            this.StartButton.TextSizePx = 21F;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(384, 661);
            this.Controls.Add(this.StartButton);
            this.Name = "MainForm";
            this.Text = "WoW Forever Randomizer";
            this.ResumeLayout(false);
        }

        #endregion

        private WoWForeverRandomizer.UI.GoldButton StartButton;
    }
}
