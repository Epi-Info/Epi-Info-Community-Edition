namespace Epi.Windows.Docking
{
    partial class DockContainer
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

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.autoHideTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// toolTip
			// 
			this.toolTip.Active = false;
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// autoHideTimer
			// 
			this.autoHideTimer.Interval = 25;
			this.autoHideTimer.Tick += new System.EventHandler(this.autoHideTimer_Tick);
			// 
			// DockContainer
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.ResumeLayout(false);

		}
		#endregion



        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.Timer autoHideTimer;
        private System.Windows.Forms.ToolTip toolTip;
    }
}