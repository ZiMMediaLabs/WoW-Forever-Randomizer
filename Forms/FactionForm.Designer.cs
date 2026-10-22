namespace WoWForeverRandomizer.Forms
{
    partial class FactionForm
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
            this.AllianceTile = new WoWForeverRandomizer.UI.FactionTile();
            this.HordeTile = new WoWForeverRandomizer.UI.FactionTile();
            this.EitherTile = new WoWForeverRandomizer.UI.FactionTile();
            this.BackButton = new WoWForeverRandomizer.UI.GoldButton();
            this.SuspendLayout();
            //
            // AllianceTile
            //
            this.AllianceTile.Choice = WoWForeverRandomizer.Data.FactionChoice.Alliance;
            this.AllianceTile.Location = new System.Drawing.Point(22, 66);
            this.AllianceTile.Name = "AllianceTile";
            this.AllianceTile.Size = new System.Drawing.Size(340, 166);
            this.AllianceTile.TabIndex = 0;
            this.AllianceTile.Click += new System.EventHandler(this.FactionTile_Click);
            //
            // HordeTile
            //
            this.HordeTile.Choice = WoWForeverRandomizer.Data.FactionChoice.Horde;
            this.HordeTile.Location = new System.Drawing.Point(22, 230);
            this.HordeTile.Name = "HordeTile";
            this.HordeTile.Size = new System.Drawing.Size(340, 166);
            this.HordeTile.TabIndex = 1;
            this.HordeTile.Click += new System.EventHandler(this.FactionTile_Click);
            //
            // EitherTile
            //
            this.EitherTile.Choice = WoWForeverRandomizer.Data.FactionChoice.Either;
            this.EitherTile.Location = new System.Drawing.Point(22, 394);
            this.EitherTile.Name = "EitherTile";
            this.EitherTile.Size = new System.Drawing.Size(340, 166);
            this.EitherTile.TabIndex = 2;
            this.EitherTile.Click += new System.EventHandler(this.FactionTile_Click);
            //
            // BackButton
            //
            this.BackButton.Location = new System.Drawing.Point(112, 590);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(160, 44);
            this.BackButton.TabIndex = 3;
            this.BackButton.Text = "BACK";
            this.BackButton.TextSizePx = 18F;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            //
            // FactionForm
            //
            this.ClientSize = new System.Drawing.Size(384, 661);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.EitherTile);
            this.Controls.Add(this.HordeTile);
            this.Controls.Add(this.AllianceTile);
            this.Name = "FactionForm";
            this.Text = "Choose Your Faction";
            this.ResumeLayout(false);
        }

        #endregion

        private WoWForeverRandomizer.UI.FactionTile AllianceTile;
        private WoWForeverRandomizer.UI.FactionTile HordeTile;
        private WoWForeverRandomizer.UI.FactionTile EitherTile;
        private WoWForeverRandomizer.UI.GoldButton BackButton;
    }
}
