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
            this.SelectedDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this.DownloadFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SearchForFolderDialogButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.AvailableVersionSetGroupBox = new System.Windows.Forms.GroupBox();
            this.AvailableVersionSetListBox = new System.Windows.Forms.ListBox();
            this.VersionDetailTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.AvailableVersionSetGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(16, 15);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(859, 28);
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
            this.CreateLocalHashButton.Location = new System.Drawing.Point(14, 322);
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
            this.OutputTextBox.Location = new System.Drawing.Point(12, 371);
            this.OutputTextBox.Multiline = true;
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.OutputTextBox.Size = new System.Drawing.Size(863, 188);
            this.OutputTextBox.TabIndex = 3;
            this.OutputTextBox.WordWrap = false;
            // 
            // ExecuteDownloadButton
            // 
            this.ExecuteDownloadButton.Location = new System.Drawing.Point(327, 322);
            this.ExecuteDownloadButton.Name = "ExecuteDownloadButton";
            this.ExecuteDownloadButton.Size = new System.Drawing.Size(155, 32);
            this.ExecuteDownloadButton.TabIndex = 4;
            this.ExecuteDownloadButton.Text = "Execute Download";
            this.ExecuteDownloadButton.UseVisualStyleBackColor = true;
            this.ExecuteDownloadButton.Click += new System.EventHandler(this.ExecuteDownloadButton_Click);
            // 
            // SelectedDownloadFolderTextBox
            // 
            this.SelectedDownloadFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedDownloadFolderTextBox.Location = new System.Drawing.Point(6, 31);
            this.SelectedDownloadFolderTextBox.Name = "SelectedDownloadFolderTextBox";
            this.SelectedDownloadFolderTextBox.Size = new System.Drawing.Size(847, 22);
            this.SelectedDownloadFolderTextBox.TabIndex = 5;
            // 
            // SearchForFolderDialogButton
            // 
            this.SearchForFolderDialogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchForFolderDialogButton.Location = new System.Drawing.Point(633, 72);
            this.SearchForFolderDialogButton.Name = "SearchForFolderDialogButton";
            this.SearchForFolderDialogButton.Size = new System.Drawing.Size(220, 33);
            this.SearchForFolderDialogButton.TabIndex = 6;
            this.SearchForFolderDialogButton.Text = "Select Download Folder...";
            this.SearchForFolderDialogButton.UseVisualStyleBackColor = true;
            this.SearchForFolderDialogButton.Click += new System.EventHandler(this.SearchForFolderDialogButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.SearchForFolderDialogButton);
            this.groupBox1.Controls.Add(this.SelectedDownloadFolderTextBox);
            this.groupBox1.Location = new System.Drawing.Point(14, 205);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(859, 111);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Download Destination Folder";
            // 
            // AvailableVersionSetGroupBox
            // 
            this.AvailableVersionSetGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AvailableVersionSetGroupBox.Controls.Add(this.VersionDetailTextBox);
            this.AvailableVersionSetGroupBox.Controls.Add(this.AvailableVersionSetListBox);
            this.AvailableVersionSetGroupBox.Location = new System.Drawing.Point(19, 50);
            this.AvailableVersionSetGroupBox.Name = "AvailableVersionSetGroupBox";
            this.AvailableVersionSetGroupBox.Size = new System.Drawing.Size(856, 149);
            this.AvailableVersionSetGroupBox.TabIndex = 8;
            this.AvailableVersionSetGroupBox.TabStop = false;
            this.AvailableVersionSetGroupBox.Text = "Availabe Versions";
            // 
            // AvailableVersionSetListBox
            // 
            this.AvailableVersionSetListBox.FormattingEnabled = true;
            this.AvailableVersionSetListBox.ItemHeight = 16;
            this.AvailableVersionSetListBox.Location = new System.Drawing.Point(6, 21);
            this.AvailableVersionSetListBox.Name = "AvailableVersionSetListBox";
            this.AvailableVersionSetListBox.Size = new System.Drawing.Size(254, 116);
            this.AvailableVersionSetListBox.TabIndex = 0;
            // 
            // VersionDetailTextBox
            // 
            this.VersionDetailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VersionDetailTextBox.Location = new System.Drawing.Point(266, 21);
            this.VersionDetailTextBox.Multiline = true;
            this.VersionDetailTextBox.Name = "VersionDetailTextBox";
            this.VersionDetailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.VersionDetailTextBox.Size = new System.Drawing.Size(565, 122);
            this.VersionDetailTextBox.TabIndex = 1;
            this.VersionDetailTextBox.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 571);
            this.Controls.Add(this.AvailableVersionSetGroupBox);
            this.Controls.Add(this.groupBox1);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.AvailableVersionSetGroupBox.ResumeLayout(false);
            this.AvailableVersionSetGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button CreateLocalHashButton;
        private System.Windows.Forms.TextBox OutputTextBox;
        private System.Windows.Forms.Button ExecuteDownloadButton;
        private System.Windows.Forms.TextBox SelectedDownloadFolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog DownloadFolderBrowserDialog;
        private System.Windows.Forms.Button SearchForFolderDialogButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox AvailableVersionSetGroupBox;
        private System.Windows.Forms.TextBox VersionDetailTextBox;
        private System.Windows.Forms.ListBox AvailableVersionSetListBox;
    }
}

