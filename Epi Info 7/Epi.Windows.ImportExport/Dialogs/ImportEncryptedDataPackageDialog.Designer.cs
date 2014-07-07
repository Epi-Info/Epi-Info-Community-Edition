namespace Epi.Windows.ImportExport.Dialogs
{
    partial class ImportEncryptedDataPackageDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportEncryptedDataPackageDialog));
            this.cmbImportType = new System.Windows.Forms.ComboBox();
            this.lblWarningMessage = new System.Windows.Forms.Label();
            this.txtPackageFile = new System.Windows.Forms.TextBox();
            this.lblWarning = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.lblImportType = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.checkboxBatchImport = new System.Windows.Forms.CheckBox();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.groupImportInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbImportType
            // 
            this.cmbImportType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbImportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImportType.FormattingEnabled = true;
            this.cmbImportType.Items.AddRange(new object[] {
            "Update and append records",
            "Update records only",
            "Append records only"});
            this.cmbImportType.Location = new System.Drawing.Point(9, 150);
            this.cmbImportType.Name = "cmbImportType";
            this.cmbImportType.Size = new System.Drawing.Size(166, 21);
            this.cmbImportType.TabIndex = 4;
            // 
            // lblWarningMessage
            // 
            this.lblWarningMessage.Location = new System.Drawing.Point(103, 84);
            this.lblWarningMessage.Name = "lblWarningMessage";
            this.lblWarningMessage.Size = new System.Drawing.Size(414, 44);
            this.lblWarningMessage.TabIndex = 25;
            this.lblWarningMessage.Text = "Import operations are permanent and cannot be undone. Be sure the form structures" +
    " are the same (including all form names and form table names) before proceeding." +
    "";
            // 
            // txtPackageFile
            // 
            this.txtPackageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPackageFile.Location = new System.Drawing.Point(9, 71);
            this.txtPackageFile.Name = "txtPackageFile";
            this.txtPackageFile.Size = new System.Drawing.Size(406, 20);
            this.txtPackageFile.TabIndex = 1;
            this.txtPackageFile.TextChanged += new System.EventHandler(this.txtPackageFile_TextChanged);
            // 
            // lblWarning
            // 
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(36, 89);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(71, 18);
            this.lblWarning.TabIndex = 24;
            this.lblWarning.Text = "Warning:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBrowse.Location = new System.Drawing.Point(421, 68);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtProgress
            // 
            this.txtProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProgress.Location = new System.Drawing.Point(12, 488);
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.ReadOnly = true;
            this.txtProgress.Size = new System.Drawing.Size(505, 20);
            this.txtProgress.TabIndex = 23;
            this.txtProgress.TabStop = false;
            this.txtProgress.Text = "Ready";
            // 
            // lblProjectFile
            // 
            this.lblProjectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProjectFile.Location = new System.Drawing.Point(6, 55);
            this.lblProjectFile.Name = "lblProjectFile";
            this.lblProjectFile.Size = new System.Drawing.Size(460, 13);
            this.lblProjectFile.TabIndex = 1;
            this.lblProjectFile.Text = "Encrypted data package(s) to import:";
            // 
            // lbxStatus
            // 
            this.lbxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxStatus.FormattingEnabled = true;
            this.lbxStatus.HorizontalScrollbar = true;
            this.lbxStatus.Location = new System.Drawing.Point(12, 322);
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.Size = new System.Drawing.Size(505, 160);
            this.lbxStatus.TabIndex = 21;
            this.lbxStatus.TabStop = false;
            // 
            // lblImportType
            // 
            this.lblImportType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblImportType.Location = new System.Drawing.Point(9, 134);
            this.lblImportType.Name = "lblImportType";
            this.lblImportType.Size = new System.Drawing.Size(166, 13);
            this.lblImportType.TabIndex = 6;
            this.lblImportType.Text = "Type of import:";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 514);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(505, 23);
            this.progressBar.TabIndex = 22;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(361, 544);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 17;
            this.btnOK.Text = "Import";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(442, 544);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 11);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(505, 59);
            this.lblDescription.TabIndex = 16;
            this.lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // groupImportInfo
            // 
            this.groupImportInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupImportInfo.Controls.Add(this.checkboxBatchImport);
            this.groupImportInfo.Controls.Add(this.btnAdvanced);
            this.groupImportInfo.Controls.Add(this.txtPassword);
            this.groupImportInfo.Controls.Add(this.cmbImportType);
            this.groupImportInfo.Controls.Add(this.lblImportType);
            this.groupImportInfo.Controls.Add(this.btnBrowse);
            this.groupImportInfo.Controls.Add(this.txtPackageFile);
            this.groupImportInfo.Controls.Add(this.lblPassword);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Location = new System.Drawing.Point(15, 131);
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.Size = new System.Drawing.Size(502, 184);
            this.groupImportInfo.TabIndex = 18;
            this.groupImportInfo.TabStop = false;
            this.groupImportInfo.Text = "Import information";
            // 
            // checkboxBatchImport
            // 
            this.checkboxBatchImport.AutoSize = true;
            this.checkboxBatchImport.Location = new System.Drawing.Point(7, 28);
            this.checkboxBatchImport.Name = "checkboxBatchImport";
            this.checkboxBatchImport.Size = new System.Drawing.Size(95, 17);
            this.checkboxBatchImport.TabIndex = 9;
            this.checkboxBatchImport.Text = "Is batch import";
            this.checkboxBatchImport.UseVisualStyleBackColor = true;
            this.checkboxBatchImport.CheckedChanged += new System.EventHandler(this.checkboxBatchImport_CheckedChanged);
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdvanced.Location = new System.Drawing.Point(358, 148);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(138, 23);
            this.btnAdvanced.TabIndex = 8;
            this.btnAdvanced.Text = "Advanced Options";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Visible = false;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPassword.Location = new System.Drawing.Point(9, 111);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(487, 20);
            this.txtPassword.TabIndex = 7;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // lblPassword
            // 
            this.lblPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPassword.Location = new System.Drawing.Point(9, 94);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(487, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password:";
            // 
            // ImportEncryptedDataPackageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(529, 577);
            this.Controls.Add(this.lblWarningMessage);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.lbxStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.groupImportInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ImportEncryptedDataPackageDialog";
            this.ShowIcon = false;
            this.Text = "Import Encrypted Data Package";
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbImportType;
        private System.Windows.Forms.Label lblWarningMessage;
        private System.Windows.Forms.TextBox txtPackageFile;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtProgress;
        private System.Windows.Forms.Label lblProjectFile;
        private System.Windows.Forms.ListBox lbxStatus;
        private System.Windows.Forms.Label lblImportType;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.GroupBox groupImportInfo;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.CheckBox checkboxBatchImport;
    }
}