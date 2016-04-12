namespace Updater
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.CreateLocalHashButton = new System.Windows.Forms.Button();
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.ExecuteDownloadButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 15);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(671, 28);
            this.progressBar1.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(16, 66);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            this.lblStatus.TabIndex = 1;
            // 
            // CreateLocalHashButton
            // 
            this.CreateLocalHashButton.Location = new System.Drawing.Point(12, 150);
            this.CreateLocalHashButton.Name = "CreateLocalHashButton";
            this.CreateLocalHashButton.Size = new System.Drawing.Size(187, 23);
            this.CreateLocalHashButton.TabIndex = 2;
            this.CreateLocalHashButton.Text = "CreateLocalHashButton";
            this.CreateLocalHashButton.UseVisualStyleBackColor = true;
            this.CreateLocalHashButton.Click += new System.EventHandler(this.CreateLocalHashButton_Click);
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputTextBox.HideSelection = false;
            this.OutputTextBox.Location = new System.Drawing.Point(12, 192);
            this.OutputTextBox.Multiline = true;
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.OutputTextBox.Size = new System.Drawing.Size(695, 159);
            this.OutputTextBox.TabIndex = 3;
            this.OutputTextBox.WordWrap = false;
            // 
            // ExecuteDownloadButton
            // 
            this.ExecuteDownloadButton.Location = new System.Drawing.Point(331, 145);
            this.ExecuteDownloadButton.Name = "ExecuteDownloadButton";
            this.ExecuteDownloadButton.Size = new System.Drawing.Size(155, 32);
            this.ExecuteDownloadButton.TabIndex = 4;
            this.ExecuteDownloadButton.Text = "Execute Download";
            this.ExecuteDownloadButton.UseVisualStyleBackColor = true;
            this.ExecuteDownloadButton.Click += new System.EventHandler(this.ExecuteDownloadButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 363);
            this.Controls.Add(this.ExecuteDownloadButton);
            this.Controls.Add(this.OutputTextBox);
            this.Controls.Add(this.CreateLocalHashButton);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Epi Info Updater";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button CreateLocalHashButton;
        private System.Windows.Forms.TextBox OutputTextBox;
        private System.Windows.Forms.Button ExecuteDownloadButton;
    }
}

