namespace Epi.Windows.AnalysisDashboard
{
    partial class DashboardMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardMainForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveHtml = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDataSource = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnAddRelate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.lblDataSource = new System.Windows.Forms.ToolStripLabel();
            this.txtDataSource = new System.Windows.Forms.ToolStripLabel();
            this.lblRecordsPrefix = new System.Windows.Forms.ToolStripLabel();
            this.txtRecordCount = new System.Windows.Forms.ToolStripLabel();
            this.lblRecordCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1072, 628);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1072, 655);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.btnSave,
            this.btnSaveHtml,
            this.toolStripSeparator1,
            this.btnDataSource,
            this.toolStripBtnAddRelate,
            this.toolStripSeparator2,
            this.lblDataSource,
            this.txtDataSource,
            this.lblRecordsPrefix,
            this.txtRecordCount,
            this.lblRecordCount});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(140, 27);
            this.toolStrip1.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 24);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Enabled = false;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(23, 24);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveHtml
            // 
            this.btnSaveHtml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveHtml.Enabled = false;
            this.btnSaveHtml.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveHtml.Image")));
            this.btnSaveHtml.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSaveHtml.Name = "btnSaveHtml";
            this.btnSaveHtml.Size = new System.Drawing.Size(23, 24);
            this.btnSaveHtml.Text = "Save as HTML";
            this.btnSaveHtml.Click += new System.EventHandler(this.btnSaveHtml_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnDataSource
            // 
            this.btnDataSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDataSource.Image = ((System.Drawing.Image)(resources.GetObject("btnDataSource.Image")));
            this.btnDataSource.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDataSource.Name = "btnDataSource";
            this.btnDataSource.Size = new System.Drawing.Size(23, 24);
            this.btnDataSource.Text = "Set Data Source";
            this.btnDataSource.Click += new System.EventHandler(this.btnDataSource_Click);
            // 
            // toolStripBtnAddRelate
            // 
            this.toolStripBtnAddRelate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBtnAddRelate.Enabled = false;
            this.toolStripBtnAddRelate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBtnAddRelate.Image")));
            this.toolStripBtnAddRelate.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripBtnAddRelate.Name = "toolStripBtnAddRelate";
            this.toolStripBtnAddRelate.Size = new System.Drawing.Size(24, 24);
            this.toolStripBtnAddRelate.Text = "Add Related Data";
            this.toolStripBtnAddRelate.Click += new System.EventHandler(this.toolStripBtnAddRelate_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // lblDataSource
            // 
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(93, 24);
            this.lblDataSource.Text = "Data Source:";
            this.lblDataSource.Visible = false;
            // 
            // txtDataSource
            // 
            this.txtDataSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(0, 24);
            this.txtDataSource.Visible = false;
            // 
            // lblRecordsPrefix
            // 
            this.lblRecordsPrefix.AutoSize = false;
            this.lblRecordsPrefix.Name = "lblRecordsPrefix";
            this.lblRecordsPrefix.Size = new System.Drawing.Size(5, 22);
            this.lblRecordsPrefix.Text = "(";
            this.lblRecordsPrefix.Visible = false;
            // 
            // txtRecordCount
            // 
            this.txtRecordCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRecordCount.Name = "txtRecordCount";
            this.txtRecordCount.Size = new System.Drawing.Size(0, 24);
            this.txtRecordCount.Visible = false;
            // 
            // lblRecordCount
            // 
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(67, 24);
            this.lblRecordCount.Text = "records )";
            this.lblRecordCount.Visible = false;
            // 
            // DashboardMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1072, 655);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DashboardMainForm";
            this.Text = "Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DashboardMainForm_FormClosing);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnSaveHtml;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnDataSource;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel lblRecordCount;
        private System.Windows.Forms.ToolStripLabel txtRecordCount;
        private System.Windows.Forms.ToolStripLabel lblDataSource;
        private System.Windows.Forms.ToolStripLabel txtDataSource;
        private System.Windows.Forms.ToolStripLabel lblRecordsPrefix;
        private System.Windows.Forms.ToolStripButton toolStripBtnAddRelate;
    }
}

