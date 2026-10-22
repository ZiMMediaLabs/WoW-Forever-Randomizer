namespace WoWForeverRandomizer.Forms
{
    partial class ResultForm
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
            this.components = new System.ComponentModel.Container();
            this.RerollButton = new WoWForeverRandomizer.UI.GoldButton();
            this.BackButton = new WoWForeverRandomizer.UI.GoldButton();
            this.AnimationTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            //
            // RerollButton
            //
            this.RerollButton.Location = new System.Drawing.Point(92, 538);
            this.RerollButton.Name = "RerollButton";
            this.RerollButton.PlayClickSound = false;
            this.RerollButton.Size = new System.Drawing.Size(200, 48);
            this.RerollButton.TabIndex = 0;
            this.RerollButton.Text = "REROLL";
            this.RerollButton.TextSizePx = 20F;
            this.RerollButton.Click += new System.EventHandler(this.RerollButton_Click);
            //
            // BackButton
            //
            this.BackButton.Location = new System.Drawing.Point(112, 594);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(160, 44);
            this.BackButton.TabIndex = 1;
            this.BackButton.Text = "BACK";
            this.BackButton.TextSizePx = 18F;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            //
            // AnimationTimer
            //
            this.AnimationTimer.Interval = 16;
            this.AnimationTimer.Tick += new System.EventHandler(this.AnimationTimer_Tick);
            //
            // ResultForm
            //
            this.ClientSize = new System.Drawing.Size(384, 661);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.RerollButton);
            this.Name = "ResultForm";
            this.Text = "Your Destiny";
            this.ResumeLayout(false);
        }

        #endregion

        private WoWForeverRandomizer.UI.GoldButton RerollButton;
        private WoWForeverRandomizer.UI.GoldButton BackButton;
        private System.Windows.Forms.Timer AnimationTimer;
    }
}
