
namespace AutoNumberUpdater
{
    partial class AutoNumberUpdater
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbFixAutoNumber = new System.Windows.Forms.ToolStripButton();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.StatusText = new System.Windows.Forms.TextBox();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbFixAutoNumber});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(2789, 52);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(211, 45);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 52);
            // 
            // tsbFixAutoNumber
            // 
            this.tsbFixAutoNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbFixAutoNumber.Name = "tsbFixAutoNumber";
            this.tsbFixAutoNumber.Size = new System.Drawing.Size(368, 45);
            this.tsbFixAutoNumber.Text = "Fix AutoNumber (Contact)";
            this.tsbFixAutoNumber.Click += new System.EventHandler(this.tsbFixAutoNumber_Click);
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressBar.Location = new System.Drawing.Point(0, 52);
            this.progressBar.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(2789, 38);
            this.progressBar.TabIndex = 26;
            this.progressBar.Visible = false;
            // 
            // StatusText
            // 
            this.StatusText.Dock = System.Windows.Forms.DockStyle.Top;
            this.StatusText.Location = new System.Drawing.Point(0, 90);
            this.StatusText.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.StatusText.Multiline = true;
            this.StatusText.Name = "StatusText";
            this.StatusText.ReadOnly = true;
            this.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StatusText.Size = new System.Drawing.Size(2789, 1246);
            this.StatusText.TabIndex = 27;
            // 
            // AutoNumberUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "AutoNumberUpdater";
            this.Size = new System.Drawing.Size(2789, 1319);
            this.Load += new System.EventHandler(this.AutoNumberUpdater_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbFixAutoNumber;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox StatusText;
    }
}
