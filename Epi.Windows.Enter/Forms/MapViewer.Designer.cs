namespace Epi.Windows.Enter
{
    partial class MapViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapViewer));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAsImage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDataLayer = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.caseClusterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.choroplethToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withShapeFileBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withMapServerBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withKMLBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dotDensityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withShapeFileBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.withMapServerBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.withKMLBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnReference = new System.Windows.Forms.ToolStripDropDownButton();
            this.fromMapServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromShapeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromKMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnTimeLapse = new System.Windows.Forms.ToolStripButton();
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1344, 788);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1344, 815);
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
            this.btnSaveAsImage,
            this.toolStripSeparator1,
            this.btnDataLayer,
            this.btnReference,
            this.btnTimeLapse,
        });
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(627, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Visible = false;
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Enabled = false;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 24);
            this.btnOpen.Text = "Open";
            this.btnOpen.Visible = false;
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
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAsImage
            // 
            this.btnSaveAsImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAsImage.Enabled = false;
            this.btnSaveAsImage.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAsImage.Image")));
            this.btnSaveAsImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSaveAsImage.Name = "btnSaveAsImage";
            this.btnSaveAsImage.Size = new System.Drawing.Size(23, 24);
            this.btnSaveAsImage.Text = "Save as Image";
            this.btnSaveAsImage.Visible = false;
            this.btnSaveAsImage.Click += new System.EventHandler(this.btnSaveAsImage_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            this.toolStripSeparator1.Visible = false;
            // 
            // btnDataLayer
            // 
            this.btnDataLayer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.caseClusterToolStripMenuItem,
            this.choroplethToolStripMenuItem,
            this.dotDensityToolStripMenuItem});
            this.btnDataLayer.Enabled = false;
            this.btnDataLayer.Image = ((System.Drawing.Image)(resources.GetObject("btnDataLayer.Image")));
            this.btnDataLayer.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDataLayer.Name = "btnDataLayer";
            this.btnDataLayer.Size = new System.Drawing.Size(142, 24);
            this.btnDataLayer.Text = "Add Data Layer";
            this.btnDataLayer.Visible = false;
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(189, 24);
            this.toolStripMenuItem1.Text = "Spot Map";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // caseClusterToolStripMenuItem
            // 
            this.caseClusterToolStripMenuItem.Name = "caseClusterToolStripMenuItem";
            this.caseClusterToolStripMenuItem.Size = new System.Drawing.Size(189, 24);
            this.caseClusterToolStripMenuItem.Text = "Case Cluster";
            this.caseClusterToolStripMenuItem.Click += new System.EventHandler(this.caseClusterToolStripMenuItem_Click);
            // 
            // choroplethToolStripMenuItem
            // 
            this.choroplethToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.withShapeFileBoundariesToolStripMenuItem,
            this.withMapServerBoundariesToolStripMenuItem,
            this.withKMLBoundariesToolStripMenuItem});
            this.choroplethToolStripMenuItem.Name = "choroplethToolStripMenuItem";
            this.choroplethToolStripMenuItem.Size = new System.Drawing.Size(189, 24);
            this.choroplethToolStripMenuItem.Text = "Choropleth";
            // 
            // withShapeFileBoundariesToolStripMenuItem
            // 
            this.withShapeFileBoundariesToolStripMenuItem.Name = "withShapeFileBoundariesToolStripMenuItem";
            this.withShapeFileBoundariesToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.withShapeFileBoundariesToolStripMenuItem.Text = "With Shape File Boundaries";
            this.withShapeFileBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withShapeFileBoundariesToolStripMenuItem_Click);
            // 
            // withMapServerBoundariesToolStripMenuItem
            // 
            this.withMapServerBoundariesToolStripMenuItem.Name = "withMapServerBoundariesToolStripMenuItem";
            this.withMapServerBoundariesToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.withMapServerBoundariesToolStripMenuItem.Text = "With Map Server Boundaries";
            this.withMapServerBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withMapServerBoundariesToolStripMenuItem_Click);
            // 
            // withKMLBoundariesToolStripMenuItem
            // 
            this.withKMLBoundariesToolStripMenuItem.Name = "withKMLBoundariesToolStripMenuItem";
            this.withKMLBoundariesToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.withKMLBoundariesToolStripMenuItem.Text = "With KML Boundaries";
            this.withKMLBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withKMLBoundariesToolStripMenuItem_Click);
            // 
            // dotDensityToolStripMenuItem
            // 
            this.dotDensityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.withShapeFileBoundariesToolStripMenuItem1,
            this.withMapServerBoundariesToolStripMenuItem1,
            this.withKMLBoundariesToolStripMenuItem1});
            this.dotDensityToolStripMenuItem.Name = "dotDensityToolStripMenuItem";
            this.dotDensityToolStripMenuItem.Size = new System.Drawing.Size(189, 24);
            this.dotDensityToolStripMenuItem.Text = "Dot Density";
            // 
            // withShapeFileBoundariesToolStripMenuItem1
            // 
            this.withShapeFileBoundariesToolStripMenuItem1.Name = "withShapeFileBoundariesToolStripMenuItem1";
            this.withShapeFileBoundariesToolStripMenuItem1.Size = new System.Drawing.Size(266, 24);
            this.withShapeFileBoundariesToolStripMenuItem1.Text = "With Shape File Boundaries";
            this.withShapeFileBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withShapeFileBoundariesToolStripMenuItem1_Click);
            // 
            // withMapServerBoundariesToolStripMenuItem1
            // 
            this.withMapServerBoundariesToolStripMenuItem1.Name = "withMapServerBoundariesToolStripMenuItem1";
            this.withMapServerBoundariesToolStripMenuItem1.Size = new System.Drawing.Size(266, 24);
            this.withMapServerBoundariesToolStripMenuItem1.Text = "With Map Server Boundaries";
            this.withMapServerBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withMapServerBoundariesToolStripMenuItem1_Click);
            // 
            // withKMLBoundariesToolStripMenuItem1
            // 
            this.withKMLBoundariesToolStripMenuItem1.Name = "withKMLBoundariesToolStripMenuItem1";
            this.withKMLBoundariesToolStripMenuItem1.Size = new System.Drawing.Size(266, 24);
            this.withKMLBoundariesToolStripMenuItem1.Text = "With KML Boundaries";
            this.withKMLBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withKMLBoundariesToolStripMenuItem1_Click);
            // 
            // btnReference
            // 
            this.btnReference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnReference.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromMapServerToolStripMenuItem,
            this.fromShapeFileToolStripMenuItem,
            this.fromKMLToolStripMenuItem});
            this.btnReference.Enabled = false;
            this.btnReference.Image = ((System.Drawing.Image)(resources.GetObject("btnReference.Image")));
            this.btnReference.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReference.Name = "btnReference";
            this.btnReference.Size = new System.Drawing.Size(160, 24);
            this.btnReference.Text = "Add Base Layer";
            this.btnReference.Visible = false;
            // 
            // fromMapServerToolStripMenuItem
            // 
            this.fromMapServerToolStripMenuItem.Name = "fromMapServerToolStripMenuItem";
            this.fromMapServerToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.fromMapServerToolStripMenuItem.Text = "From Map Server";
            this.fromMapServerToolStripMenuItem.Click += new System.EventHandler(this.fromMapServerToolStripMenuItem_Click);
            // 
            // fromShapeFileToolStripMenuItem
            // 
            this.fromShapeFileToolStripMenuItem.Name = "fromShapeFileToolStripMenuItem";
            this.fromShapeFileToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.fromShapeFileToolStripMenuItem.Text = "From Shape File";
            this.fromShapeFileToolStripMenuItem.Click += new System.EventHandler(this.fromShapeFileToolStripMenuItem_Click);
            // 
            // fromKMLToolStripMenuItem
            // 
            this.fromKMLToolStripMenuItem.Name = "fromKMLToolStripMenuItem";
            this.fromKMLToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.fromKMLToolStripMenuItem.Text = "From KML";
            this.fromKMLToolStripMenuItem.Click += new System.EventHandler(this.fromKMLToolStripMenuItem_Click);
            // 
            // btnTimeLapse
            // 
            this.btnTimeLapse.Enabled = false;
            this.btnTimeLapse.Image = ((System.Drawing.Image)(resources.GetObject("btnTimeLapse.Image")));
            this.btnTimeLapse.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnTimeLapse.Name = "btnTimeLapse";
            this.btnTimeLapse.Size = new System.Drawing.Size(151, 24);
            this.btnTimeLapse.Text = "Create Time Lapse";
            this.btnTimeLapse.Visible = false;
            this.btnTimeLapse.Click += new System.EventHandler(this.btnTimeLapse_Click);
            // 
           
            // MapViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1344, 815);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MapViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Map";
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
        private System.Windows.Forms.ToolStripButton btnSaveAsImage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton btnDataLayer;
        private System.Windows.Forms.ToolStripMenuItem caseClusterToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnTimeLapse;
        private System.Windows.Forms.ToolStripDropDownButton btnReference;
        private System.Windows.Forms.ToolStripMenuItem fromMapServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromShapeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem choroplethToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dotDensityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withShapeFileBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withMapServerBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withKMLBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withShapeFileBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem withMapServerBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem withKMLBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fromKMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
      


    }
}