
namespace Sdmsols.XTB.AutoNumberUpdater
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
            this.components = new System.ComponentModel.Container();
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tslAbout = new System.Windows.Forms.ToolStripLabel();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.StatusText = new System.Windows.Forms.TextBox();
            this.cmbSolution = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbEntities = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbAttributes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSample = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnFixAutoNumbers = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbStateCodes = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkAscending = new System.Windows.Forms.CheckBox();
            this.txtOrderAttribute = new System.Windows.Forms.TextBox();
            this.lblOrderAttribute = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslAbout,
            this.tsbClose,
            this.tssSeparator1});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(1025, 27);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tslAbout
            // 
            this.tslAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslAbout.IsLink = true;
            this.tslAbout.Name = "tslAbout";
            this.tslAbout.Size = new System.Drawing.Size(88, 24);
            this.tslAbout.Text = "by MayankP";
            this.tslAbout.Click += new System.EventHandler(this.tslAbout_Click);
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(107, 24);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(0, 230);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(988, 27);
            this.progressBar.TabIndex = 26;
            this.progressBar.Visible = false;
            // 
            // StatusText
            // 
            this.StatusText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusText.Location = new System.Drawing.Point(0, 265);
            this.StatusText.Margin = new System.Windows.Forms.Padding(4);
            this.StatusText.Multiline = true;
            this.StatusText.Name = "StatusText";
            this.StatusText.ReadOnly = true;
            this.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StatusText.Size = new System.Drawing.Size(1000, 414);
            this.StatusText.TabIndex = 27;
            // 
            // cmbSolution
            // 
            this.cmbSolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSolution.Enabled = false;
            this.cmbSolution.FormattingEnabled = true;
            this.cmbSolution.Location = new System.Drawing.Point(109, 46);
            this.cmbSolution.Margin = new System.Windows.Forms.Padding(4);
            this.cmbSolution.Name = "cmbSolution";
            this.cmbSolution.Size = new System.Drawing.Size(525, 24);
            this.cmbSolution.TabIndex = 28;
            this.cmbSolution.SelectedIndexChanged += new System.EventHandler(this.cmbSolution_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 49);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 16);
            this.label8.TabIndex = 29;
            this.label8.Text = "Solution";
            // 
            // cmbEntities
            // 
            this.cmbEntities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntities.Enabled = false;
            this.cmbEntities.FormattingEnabled = true;
            this.cmbEntities.Location = new System.Drawing.Point(109, 82);
            this.cmbEntities.Margin = new System.Windows.Forms.Padding(4);
            this.cmbEntities.Name = "cmbEntities";
            this.cmbEntities.Size = new System.Drawing.Size(525, 24);
            this.cmbEntities.TabIndex = 31;
            this.cmbEntities.SelectedIndexChanged += new System.EventHandler(this.cmbEntities_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 86);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 16);
            this.label1.TabIndex = 30;
            this.label1.Text = "Entity";
            // 
            // cmbAttributes
            // 
            this.cmbAttributes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAttributes.Enabled = false;
            this.cmbAttributes.FormattingEnabled = true;
            this.cmbAttributes.Location = new System.Drawing.Point(109, 156);
            this.cmbAttributes.Margin = new System.Windows.Forms.Padding(4);
            this.cmbAttributes.Name = "cmbAttributes";
            this.cmbAttributes.Size = new System.Drawing.Size(525, 24);
            this.cmbAttributes.TabIndex = 33;
            this.cmbAttributes.SelectedIndexChanged += new System.EventHandler(this.cmbAttributes_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 160);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 16);
            this.label2.TabIndex = 32;
            this.label2.Text = "Attribute";
            // 
            // txtSample
            // 
            this.txtSample.BackColor = System.Drawing.SystemColors.Window;
            this.txtSample.Enabled = false;
            this.txtSample.Location = new System.Drawing.Point(109, 193);
            this.txtSample.Margin = new System.Windows.Forms.Padding(4);
            this.txtSample.Name = "txtSample";
            this.txtSample.ReadOnly = true;
            this.txtSample.Size = new System.Drawing.Size(525, 22);
            this.txtSample.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 197);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 16);
            this.label3.TabIndex = 35;
            this.label3.Text = "Next number";
            // 
            // btnFixAutoNumbers
            // 
            this.btnFixAutoNumbers.Enabled = false;
            this.btnFixAutoNumbers.Location = new System.Drawing.Point(665, 46);
            this.btnFixAutoNumbers.Margin = new System.Windows.Forms.Padding(4);
            this.btnFixAutoNumbers.Name = "btnFixAutoNumbers";
            this.btnFixAutoNumbers.Size = new System.Drawing.Size(323, 97);
            this.btnFixAutoNumbers.TabIndex = 37;
            this.btnFixAutoNumbers.Text = "Fix Auto Numbers (Updates Records which are missing auto number value)";
            this.btnFixAutoNumbers.UseVisualStyleBackColor = true;
            this.btnFixAutoNumbers.Click += new System.EventHandler(this.btnFixAutoNumbers_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 124);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 16);
            this.label4.TabIndex = 38;
            this.label4.Text = "State Code";
            // 
            // cmbStateCodes
            // 
            this.cmbStateCodes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStateCodes.Enabled = false;
            this.cmbStateCodes.FormattingEnabled = true;
            this.cmbStateCodes.Location = new System.Drawing.Point(109, 119);
            this.cmbStateCodes.Margin = new System.Windows.Forms.Padding(4);
            this.cmbStateCodes.Name = "cmbStateCodes";
            this.cmbStateCodes.Size = new System.Drawing.Size(525, 24);
            this.cmbStateCodes.TabIndex = 39;
            this.cmbStateCodes.SelectedIndexChanged += new System.EventHandler(this.cmbStateCodes_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkAscending);
            this.groupBox1.Controls.Add(this.txtOrderAttribute);
            this.groupBox1.Controls.Add(this.lblOrderAttribute);
            this.groupBox1.Location = new System.Drawing.Point(665, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(323, 73);
            this.groupBox1.TabIndex = 40;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Order";
            // 
            // chkAscending
            // 
            this.chkAscending.AutoSize = true;
            this.chkAscending.Checked = true;
            this.chkAscending.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAscending.Location = new System.Drawing.Point(206, 48);
            this.chkAscending.Name = "chkAscending";
            this.chkAscending.Size = new System.Drawing.Size(93, 20);
            this.chkAscending.TabIndex = 43;
            this.chkAscending.Text = "Ascending";
            this.chkAscending.UseVisualStyleBackColor = true;
            // 
            // txtOrderAttribute
            // 
            this.txtOrderAttribute.Location = new System.Drawing.Point(107, 18);
            this.txtOrderAttribute.Name = "txtOrderAttribute";
            this.txtOrderAttribute.Size = new System.Drawing.Size(190, 22);
            this.txtOrderAttribute.TabIndex = 42;
            this.txtOrderAttribute.Text = "createdon";
            
            // 
            // lblOrderAttribute
            // 
            this.lblOrderAttribute.AutoSize = true;
            this.lblOrderAttribute.Location = new System.Drawing.Point(7, 18);
            this.lblOrderAttribute.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOrderAttribute.Name = "lblOrderAttribute";
            this.lblOrderAttribute.Size = new System.Drawing.Size(92, 16);
            this.lblOrderAttribute.TabIndex = 41;
            this.lblOrderAttribute.Text = "Order Attribute";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // AutoNumberUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmbStateCodes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnFixAutoNumbers);
            this.Controls.Add(this.txtSample);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbAttributes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbEntities);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbSolution);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AutoNumberUpdater";
            this.Size = new System.Drawing.Size(1025, 681);
            this.ConnectionUpdated += new XrmToolBox.Extensibility.PluginControlBase.ConnectionUpdatedHandler(this.AutoNumberUpdater_ConnectionUpdated);
            this.Load += new System.EventHandler(this.AutoNumberUpdater_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        
        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox StatusText;
        private System.Windows.Forms.ComboBox cmbSolution;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbEntities;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbAttributes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSample;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnFixAutoNumbers;
        private System.Windows.Forms.ToolStripLabel tslAbout;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbStateCodes;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtOrderAttribute;
        private System.Windows.Forms.Label lblOrderAttribute;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.CheckBox chkAscending;
    }
}
