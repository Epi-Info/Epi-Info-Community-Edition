namespace Epi.Windows.Menu.Dialogs
{
    partial class UpdateDetailDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCurrentVersion = new System.Windows.Forms.TextBox();
            this.txtYourVersion = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtChanges = new System.Windows.Forms.TextBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "A newer version of Epi Info is available for download.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Your Version:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Current Version:";
            // 
            // txtCurrentVersion
            // 
            this.txtCurrentVersion.Location = new System.Drawing.Point(100, 45);
            this.txtCurrentVersion.Name = "txtCurrentVersion";
            this.txtCurrentVersion.ReadOnly = true;
            this.txtCurrentVersion.Size = new System.Drawing.Size(100, 20);
            this.txtCurrentVersion.TabIndex = 3;
            // 
            // txtYourVersion
            // 
            this.txtYourVersion.Location = new System.Drawing.Point(100, 71);
            this.txtYourVersion.Name = "txtYourVersion";
            this.txtYourVersion.ReadOnly = true;
            this.txtYourVersion.Size = new System.Drawing.Size(100, 20);
            this.txtYourVersion.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Changes in current version:";
            // 
            // txtChanges
            // 
            this.txtChanges.Location = new System.Drawing.Point(15, 116);
            this.txtChanges.Multiline = true;
            this.txtChanges.Name = "txtChanges";
            this.txtChanges.ReadOnly = true;
            this.txtChanges.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtChanges.Size = new System.Drawing.Size(318, 134);
            this.txtChanges.TabIndex = 6;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(177, 256);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 7;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(258, 256);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // UpdateDetailDialog
            // 
            this.AcceptButton = this.btnDownload;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(345, 294);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.txtChanges);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtYourVersion);
            this.Controls.Add(this.txtCurrentVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDetailDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Version Available";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCurrentVersion;
        private System.Windows.Forms.TextBox txtYourVersion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtChanges;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel;
    }
}