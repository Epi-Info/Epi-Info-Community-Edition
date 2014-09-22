namespace Epi.Windows.MakeView.Forms
{
    partial class WaitPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitPanel));
            this.labelStatus = new System.Windows.Forms.Label();
            this.pictureBoxHourglass = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHourglass)).BeginInit();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.BackColor = System.Drawing.SystemColors.Window;
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(100, 23);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "labelStatus";
            // 
            // pictureBoxHourglass
            // 
            this.pictureBoxHourglass.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBoxHourglass.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxHourglass.Image")));
            this.pictureBoxHourglass.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxHourglass.InitialImage")));
            this.pictureBoxHourglass.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxHourglass.Name = "pictureBoxHourglass";
            this.pictureBoxHourglass.Size = new System.Drawing.Size(50, 80);
            this.pictureBoxHourglass.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxHourglass.TabIndex = 0;
            this.pictureBoxHourglass.TabStop = false;
            // 
            // WaitPanel
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Size = new System.Drawing.Size(200, 300);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHourglass)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.PictureBox pictureBoxHourglass;


    }
}
