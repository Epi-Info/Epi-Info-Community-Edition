namespace Epi.Windows.Globalization.Forms
{
    partial class Import
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFindDataSource = new System.Windows.Forms.Button();
            this.cmbDataSourcePlugIns = new System.Windows.Forms.ComboBox();
            this.dataGroupBox = new System.Windows.Forms.GroupBox();
            this.gbxShow = new System.Windows.Forms.GroupBox();
            this.lvDataSourceObjects = new System.Windows.Forms.ListView();
            this.columnHeaderItem = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblLanguage = new System.Windows.Forms.Label();
            this.ddlCultures = new System.Windows.Forms.ComboBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.dataGroupBox.SuspendLayout();
            this.gbxShow.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(252, 351);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(333, 351);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFindDataSource
            // 
            this.btnFindDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFindDataSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnFindDataSource.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnFindDataSource.Location = new System.Drawing.Point(6, 45);
            this.btnFindDataSource.Name = "btnFindDataSource";
            this.btnFindDataSource.Size = new System.Drawing.Size(147, 23);
            this.btnFindDataSource.TabIndex = 12;
            this.btnFindDataSource.Text = "Connect to Data Source";
            this.btnFindDataSource.Click += new System.EventHandler(this.btnFindDataSource_Click);
            // 
            // cmbDataSourcePlugIns
            // 
            this.cmbDataSourcePlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDataSourcePlugIns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataSourcePlugIns.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmbDataSourcePlugIns.Location = new System.Drawing.Point(15, 27);
            this.cmbDataSourcePlugIns.Name = "cmbDataSourcePlugIns";
            this.cmbDataSourcePlugIns.Size = new System.Drawing.Size(234, 21);
            this.cmbDataSourcePlugIns.TabIndex = 9;
            this.cmbDataSourcePlugIns.Visible = false;
            this.cmbDataSourcePlugIns.SelectedIndexChanged += new System.EventHandler(this.cmbDataSourcePlugIns_SelectedIndexChanged);
            // 
            // dataGroupBox
            // 
            this.dataGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGroupBox.Controls.Add(this.txtDataSource);
            this.dataGroupBox.Controls.Add(this.btnFindDataSource);
            this.dataGroupBox.Location = new System.Drawing.Point(6, 60);
            this.dataGroupBox.Name = "dataGroupBox";
            this.dataGroupBox.Size = new System.Drawing.Size(402, 80);
            this.dataGroupBox.TabIndex = 13;
            this.dataGroupBox.TabStop = false;
            this.dataGroupBox.Text = "Data Source";
            // 
            // gbxShow
            // 
            this.gbxShow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxShow.Controls.Add(this.lvDataSourceObjects);
            this.gbxShow.Controls.Add(this.cmbDataSourcePlugIns);
            this.gbxShow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.gbxShow.Location = new System.Drawing.Point(6, 146);
            this.gbxShow.Name = "gbxShow";
            this.gbxShow.Padding = new System.Windows.Forms.Padding(0);
            this.gbxShow.Size = new System.Drawing.Size(402, 199);
            this.gbxShow.TabIndex = 15;
            this.gbxShow.TabStop = false;
            this.gbxShow.Text = "Data Source Explorer";
            // 
            // lvDataSourceObjects
            // 
            this.lvDataSourceObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDataSourceObjects.BackColor = System.Drawing.SystemColors.Window;
            this.lvDataSourceObjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderItem});
            this.lvDataSourceObjects.FullRowSelect = true;
            this.lvDataSourceObjects.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvDataSourceObjects.Location = new System.Drawing.Point(4, 16);
            this.lvDataSourceObjects.MultiSelect = false;
            this.lvDataSourceObjects.Name = "lvDataSourceObjects";
            this.lvDataSourceObjects.Size = new System.Drawing.Size(392, 172);
            this.lvDataSourceObjects.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvDataSourceObjects.TabIndex = 1;
            this.lvDataSourceObjects.UseCompatibleStateImageBehavior = false;
            this.lvDataSourceObjects.View = System.Windows.Forms.View.Details;
            this.lvDataSourceObjects.SelectedIndexChanged += new System.EventHandler(this.lvDataSourceObjects_SelectedIndexChanged);
            this.lvDataSourceObjects.Resize += new System.EventHandler(this.lvDataSourceObjects_Resize);
            // 
            // columnHeaderItem
            // 
            this.columnHeaderItem.Text = "Item";
            this.columnHeaderItem.Width = 300;
            // 
            // lblLanguage
            // 
            this.lblLanguage.Location = new System.Drawing.Point(6, 9);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(402, 21);
            this.lblLanguage.TabIndex = 17;
            this.lblLanguage.Text = "Please choose the language or culture to import from the following list:";
            // 
            // ddlCultures
            // 
            this.ddlCultures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlCultures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlCultures.FormattingEnabled = true;
            this.ddlCultures.Location = new System.Drawing.Point(9, 33);
            this.ddlCultures.Name = "ddlCultures";
            this.ddlCultures.Size = new System.Drawing.Size(291, 21);
            this.ddlCultures.TabIndex = 16;
            this.ddlCultures.SelectedIndexChanged += new System.EventHandler(this.ddlCultures_SelectedIndexChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 392);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(417, 22);
            this.statusStrip.TabIndex = 18;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.lblLanguage);
            this.panelMain.Controls.Add(this.btnOK);
            this.panelMain.Controls.Add(this.ddlCultures);
            this.panelMain.Controls.Add(this.btnCancel);
            this.panelMain.Controls.Add(this.gbxShow);
            this.panelMain.Controls.Add(this.dataGroupBox);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(417, 414);
            this.panelMain.TabIndex = 19;
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(6, 19);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.ReadOnly = true;
            this.txtDataSource.Size = new System.Drawing.Size(390, 20);
            this.txtDataSource.TabIndex = 13;
            // 
            // Import
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(417, 414);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Import";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Language Database";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Import_FormClosing);
            this.Load += new System.EventHandler(this.Import_Load);
            this.dataGroupBox.ResumeLayout(false);
            this.dataGroupBox.PerformLayout();
            this.gbxShow.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFindDataSource;
        private System.Windows.Forms.ComboBox cmbDataSourcePlugIns;
        private System.Windows.Forms.GroupBox dataGroupBox;
        private System.Windows.Forms.GroupBox gbxShow;
        private System.Windows.Forms.ListView lvDataSourceObjects;
        private System.Windows.Forms.ColumnHeader columnHeaderItem;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox ddlCultures;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.TextBox txtDataSource;
    }
}