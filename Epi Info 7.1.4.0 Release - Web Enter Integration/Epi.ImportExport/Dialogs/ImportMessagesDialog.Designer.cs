namespace Epi.ImportExport.Dialogs
{
    /// <summary>
    /// Partial class for Epi.Data.SqlServer form BrowseForServers
    /// </summary>
    partial class ImportMessagesDialog
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
            this.listBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.labelElapsedTime = new System.Windows.Forms.Label();
            this.txtElapsedTime = new System.Windows.Forms.TextBox();
            this.lblLogFilePath = new System.Windows.Forms.Label();
            this.txtLogFilePath = new System.Windows.Forms.TextBox();
            this.btnOpenLogFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox
            // 
            this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox.FormattingEnabled = true;
            this.listBox.HorizontalScrollbar = true;
            this.listBox.Location = new System.Drawing.Point(12, 39);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(554, 251);
            this.listBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(557, 27);
            this.label1.TabIndex = 1;
            this.label1.Text = "The following import-related messages were logged during this session of Form Des" +
    "igner:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(491, 387);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // labelElapsedTime
            // 
            this.labelElapsedTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelElapsedTime.Location = new System.Drawing.Point(12, 340);
            this.labelElapsedTime.Name = "labelElapsedTime";
            this.labelElapsedTime.Size = new System.Drawing.Size(149, 14);
            this.labelElapsedTime.TabIndex = 8;
            this.labelElapsedTime.Text = "Elapsed time:";
            // 
            // txtElapsedTime
            // 
            this.txtElapsedTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtElapsedTime.Location = new System.Drawing.Point(15, 357);
            this.txtElapsedTime.Name = "txtElapsedTime";
            this.txtElapsedTime.ReadOnly = true;
            this.txtElapsedTime.Size = new System.Drawing.Size(146, 20);
            this.txtElapsedTime.TabIndex = 9;
            // 
            // lblLogFilePath
            // 
            this.lblLogFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLogFilePath.Location = new System.Drawing.Point(12, 301);
            this.lblLogFilePath.Name = "lblLogFilePath";
            this.lblLogFilePath.Size = new System.Drawing.Size(353, 13);
            this.lblLogFilePath.TabIndex = 10;
            this.lblLogFilePath.Text = "Full path to import log file:";
            // 
            // txtLogFilePath
            // 
            this.txtLogFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtLogFilePath.Location = new System.Drawing.Point(15, 317);
            this.txtLogFilePath.Name = "txtLogFilePath";
            this.txtLogFilePath.ReadOnly = true;
            this.txtLogFilePath.Size = new System.Drawing.Size(350, 20);
            this.txtLogFilePath.TabIndex = 11;
            // 
            // btnOpenLogFile
            // 
            this.btnOpenLogFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpenLogFile.Location = new System.Drawing.Point(371, 315);
            this.btnOpenLogFile.Name = "btnOpenLogFile";
            this.btnOpenLogFile.Size = new System.Drawing.Size(75, 23);
            this.btnOpenLogFile.TabIndex = 12;
            this.btnOpenLogFile.Text = "Open Log";
            this.btnOpenLogFile.UseVisualStyleBackColor = true;
            this.btnOpenLogFile.Click += new System.EventHandler(this.btnOpenLogFile_Click);
            // 
            // ImportMessagesDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 418);
            this.Controls.Add(this.btnOpenLogFile);
            this.Controls.Add(this.txtLogFilePath);
            this.Controls.Add(this.lblLogFilePath);
            this.Controls.Add(this.txtElapsedTime);
            this.Controls.Add(this.labelElapsedTime);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportMessagesDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Messages";
            this.Shown += new System.EventHandler(this.ImportMessagesDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Label label1;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label labelElapsedTime;
        private System.Windows.Forms.TextBox txtElapsedTime;
        private System.Windows.Forms.Label lblLogFilePath;
        private System.Windows.Forms.TextBox txtLogFilePath;
        private System.Windows.Forms.Button btnOpenLogFile;
    }
}