namespace Epi.Windows.Mapping
{
    partial class MapMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapMainForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveImage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddLayer = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.caseClusterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.withShapeFileBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.withMapServerBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.withKMLBoundariesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dotDensityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withShapeFileBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withMapServerBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.withKMLBoundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            this.toolStripContainer1.ContentPanel.Load += new System.EventHandler(this.toolStripContainer1_ContentPanel_Load);
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.btnSave,
            this.btnSaveImage,
            this.toolStripSeparator1,
            this.btnAddLayer,
            this.btnReference,
            this.btnTimeLapse});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnOpen, "btnOpen");
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnSaveImage, "btnSaveImage");
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // btnAddLayer
            // 
            this.btnAddLayer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.caseClusterToolStripMenuItem,
            this.toolStripMenuItem1,
            this.dotDensityToolStripMenuItem});
            resources.ApplyResources(this.btnAddLayer, "btnAddLayer");
            this.btnAddLayer.Name = "btnAddLayer";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // caseClusterToolStripMenuItem
            // 
            this.caseClusterToolStripMenuItem.Name = "caseClusterToolStripMenuItem";
            resources.ApplyResources(this.caseClusterToolStripMenuItem, "caseClusterToolStripMenuItem");
            this.caseClusterToolStripMenuItem.Click += new System.EventHandler(this.caseClusterToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.withShapeFileBoundariesToolStripMenuItem1,
            this.withMapServerBoundariesToolStripMenuItem1,
            this.withKMLBoundariesToolStripMenuItem1});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // withShapeFileBoundariesToolStripMenuItem1
            // 
            this.withShapeFileBoundariesToolStripMenuItem1.Name = "withShapeFileBoundariesToolStripMenuItem1";
            resources.ApplyResources(this.withShapeFileBoundariesToolStripMenuItem1, "withShapeFileBoundariesToolStripMenuItem1");
            this.withShapeFileBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withShapeFileBoundariesToolStripMenuItem1_Click);
            // 
            // withMapServerBoundariesToolStripMenuItem1
            // 
            this.withMapServerBoundariesToolStripMenuItem1.Name = "withMapServerBoundariesToolStripMenuItem1";
            resources.ApplyResources(this.withMapServerBoundariesToolStripMenuItem1, "withMapServerBoundariesToolStripMenuItem1");
            this.withMapServerBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withMapServerBoundariesToolStripMenuItem1_Click);
            // 
            // withKMLBoundariesToolStripMenuItem1
            // 
            this.withKMLBoundariesToolStripMenuItem1.Name = "withKMLBoundariesToolStripMenuItem1";
            resources.ApplyResources(this.withKMLBoundariesToolStripMenuItem1, "withKMLBoundariesToolStripMenuItem1");
            this.withKMLBoundariesToolStripMenuItem1.Click += new System.EventHandler(this.withKMLBoundariesToolStripMenuItem1_Click);
            // 
            // dotDensityToolStripMenuItem
            // 
            this.dotDensityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.withShapeFileBoundariesToolStripMenuItem,
            this.withMapServerBoundariesToolStripMenuItem,
            this.withKMLBoundariesToolStripMenuItem});
            this.dotDensityToolStripMenuItem.Name = "dotDensityToolStripMenuItem";
            resources.ApplyResources(this.dotDensityToolStripMenuItem, "dotDensityToolStripMenuItem");
            // 
            // withShapeFileBoundariesToolStripMenuItem
            // 
            this.withShapeFileBoundariesToolStripMenuItem.Name = "withShapeFileBoundariesToolStripMenuItem";
            resources.ApplyResources(this.withShapeFileBoundariesToolStripMenuItem, "withShapeFileBoundariesToolStripMenuItem");
            this.withShapeFileBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withShapeFileBoundariesToolStripMenuItem_Click);
            // 
            // withMapServerBoundariesToolStripMenuItem
            // 
            this.withMapServerBoundariesToolStripMenuItem.Name = "withMapServerBoundariesToolStripMenuItem";
            resources.ApplyResources(this.withMapServerBoundariesToolStripMenuItem, "withMapServerBoundariesToolStripMenuItem");
            this.withMapServerBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withMapServerBoundariesToolStripMenuItem_Click);
            // 
            // withKMLBoundariesToolStripMenuItem
            // 
            this.withKMLBoundariesToolStripMenuItem.Name = "withKMLBoundariesToolStripMenuItem";
            resources.ApplyResources(this.withKMLBoundariesToolStripMenuItem, "withKMLBoundariesToolStripMenuItem");
            this.withKMLBoundariesToolStripMenuItem.Click += new System.EventHandler(this.withKMLBoundariesToolStripMenuItem_Click);
            // 
            // btnReference
            // 
            this.btnReference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnReference.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromMapServerToolStripMenuItem,
            this.fromShapeFileToolStripMenuItem,
            this.fromKMLToolStripMenuItem});
            resources.ApplyResources(this.btnReference, "btnReference");
            this.btnReference.Name = "btnReference";
            // 
            // fromMapServerToolStripMenuItem
            // 
            this.fromMapServerToolStripMenuItem.Name = "fromMapServerToolStripMenuItem";
            resources.ApplyResources(this.fromMapServerToolStripMenuItem, "fromMapServerToolStripMenuItem");
            this.fromMapServerToolStripMenuItem.Click += new System.EventHandler(this.fromMapServerToolStripMenuItem_Click);
            // 
            // fromShapeFileToolStripMenuItem
            // 
            this.fromShapeFileToolStripMenuItem.Name = "fromShapeFileToolStripMenuItem";
            resources.ApplyResources(this.fromShapeFileToolStripMenuItem, "fromShapeFileToolStripMenuItem");
            this.fromShapeFileToolStripMenuItem.Click += new System.EventHandler(this.fromShapeFileToolStripMenuItem_Click);
            // 
            // fromKMLToolStripMenuItem
            // 
            this.fromKMLToolStripMenuItem.Name = "fromKMLToolStripMenuItem";
            resources.ApplyResources(this.fromKMLToolStripMenuItem, "fromKMLToolStripMenuItem");
            this.fromKMLToolStripMenuItem.Click += new System.EventHandler(this.fromKMLToolStripMenuItem_Click);
            // 
            // btnTimeLapse
            // 
            resources.ApplyResources(this.btnTimeLapse, "btnTimeLapse");
            this.btnTimeLapse.Name = "btnTimeLapse";
            this.btnTimeLapse.Click += new System.EventHandler(this.btnTimeLapse_Click);
            // 
            // MapMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "MapMainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
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
        private System.Windows.Forms.ToolStripButton btnSaveImage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton btnAddLayer;
        private System.Windows.Forms.ToolStripMenuItem caseClusterToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnTimeLapse;
        private System.Windows.Forms.ToolStripDropDownButton btnReference;
        private System.Windows.Forms.ToolStripMenuItem fromMapServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromShapeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromKMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem dotDensityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withShapeFileBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withMapServerBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withKMLBoundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withShapeFileBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem withMapServerBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem withKMLBoundariesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

