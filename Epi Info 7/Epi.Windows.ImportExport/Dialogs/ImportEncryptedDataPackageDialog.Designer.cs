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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportEncryptedDataPackageDialog));
            this.lblWarningMessage = new System.Windows.Forms.Label();
            this.txtPackageFile = new System.Windows.Forms.TextBox();
            this.lblWarning = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.cmsStatus = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.checkboxBatchImport = new System.Windows.Forms.CheckBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.lblPassword = new System.Windows.Forms.Label();
            this.grpTypeOfImport = new System.Windows.Forms.GroupBox();
            this.rdbAppend = new System.Windows.Forms.RadioButton();
            this.rdbUpdate = new System.Windows.Forms.RadioButton();
            this.rdbUpdateAndAppend = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cmsStatus.SuspendLayout();
            this.groupImportInfo.SuspendLayout();
            this.grpTypeOfImport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblWarningMessage
            // 
            resources.ApplyResources(this.lblWarningMessage, "lblWarningMessage");
            this.lblWarningMessage.Name = "lblWarningMessage";
            // 
            // txtPackageFile
            // 
            resources.ApplyResources(this.txtPackageFile, "txtPackageFile");
            this.txtPackageFile.Name = "txtPackageFile";
            this.txtPackageFile.TextChanged += new System.EventHandler(this.txtPackageFile_TextChanged);
            // 
            // lblWarning
            // 
            resources.ApplyResources(this.lblWarning, "lblWarning");
            this.lblWarning.Name = "lblWarning";
            // 
            // btnBrowse
            // 
            resources.ApplyResources(this.btnBrowse, "btnBrowse");
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtProgress
            // 
            resources.ApplyResources(this.txtProgress, "txtProgress");
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.ReadOnly = true;
            this.txtProgress.TabStop = false;
            // 
            // lblProjectFile
            // 
            resources.ApplyResources(this.lblProjectFile, "lblProjectFile");
            this.lblProjectFile.Name = "lblProjectFile";
            // 
            // lbxStatus
            // 
            resources.ApplyResources(this.lbxStatus, "lbxStatus");
            this.lbxStatus.ContextMenuStrip = this.cmsStatus;
            this.lbxStatus.FormattingEnabled = true;
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.TabStop = false;
            // 
            // cmsStatus
            // 
            this.cmsStatus.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Copy});
            this.cmsStatus.Name = "cmsStatus";
            resources.ApplyResources(this.cmsStatus, "cmsStatus");
            this.cmsStatus.Click += new System.EventHandler(this.cmsStatus_Click);
            // 
            // toolStripMenuItem_Copy
            // 
            this.toolStripMenuItem_Copy.Name = "toolStripMenuItem_Copy";
            resources.ApplyResources(this.toolStripMenuItem_Copy, "toolStripMenuItem_Copy");
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.Name = "lblDescription";
            // 
            // groupImportInfo
            // 
            resources.ApplyResources(this.groupImportInfo, "groupImportInfo");
            this.groupImportInfo.Controls.Add(this.checkboxBatchImport);
            this.groupImportInfo.Controls.Add(this.txtPassword);
            this.groupImportInfo.Controls.Add(this.btnAdvanced);
            this.groupImportInfo.Controls.Add(this.btnBrowse);
            this.groupImportInfo.Controls.Add(this.txtPackageFile);
            this.groupImportInfo.Controls.Add(this.lblPassword);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.TabStop = false;
            // 
            // checkboxBatchImport
            // 
            resources.ApplyResources(this.checkboxBatchImport, "checkboxBatchImport");
            this.checkboxBatchImport.Name = "checkboxBatchImport";
            this.checkboxBatchImport.UseVisualStyleBackColor = true;
            this.checkboxBatchImport.CheckedChanged += new System.EventHandler(this.checkboxBatchImport_CheckedChanged);
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // btnAdvanced
            // 
            resources.ApplyResources(this.btnAdvanced, "btnAdvanced");
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // grpTypeOfImport
            // 
            this.grpTypeOfImport.Controls.Add(this.rdbAppend);
            this.grpTypeOfImport.Controls.Add(this.rdbUpdate);
            this.grpTypeOfImport.Controls.Add(this.rdbUpdateAndAppend);
            resources.ApplyResources(this.grpTypeOfImport, "grpTypeOfImport");
            this.grpTypeOfImport.Name = "grpTypeOfImport";
            this.grpTypeOfImport.TabStop = false;
            // 
            // rdbAppend
            // 
            resources.ApplyResources(this.rdbAppend, "rdbAppend");
            this.rdbAppend.Name = "rdbAppend";
            this.rdbAppend.TabStop = true;
            this.rdbAppend.UseVisualStyleBackColor = true;
            // 
            // rdbUpdate
            // 
            resources.ApplyResources(this.rdbUpdate, "rdbUpdate");
            this.rdbUpdate.Name = "rdbUpdate";
            this.rdbUpdate.TabStop = true;
            this.rdbUpdate.UseVisualStyleBackColor = true;
            // 
            // rdbUpdateAndAppend
            // 
            resources.ApplyResources(this.rdbUpdateAndAppend, "rdbUpdateAndAppend");
            this.rdbUpdateAndAppend.Name = "rdbUpdateAndAppend";
            this.rdbUpdateAndAppend.TabStop = true;
            this.rdbUpdateAndAppend.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Epi.Windows.ImportExport.Properties.Resources.warning;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // ImportEncryptedDataPackageDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.grpTypeOfImport);
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
            this.cmsStatus.ResumeLayout(false);
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
            this.grpTypeOfImport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWarningMessage;
        private System.Windows.Forms.TextBox txtPackageFile;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtProgress;
        private System.Windows.Forms.Label lblProjectFile;
        private System.Windows.Forms.ListBox lbxStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.GroupBox groupImportInfo;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox checkboxBatchImport;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.GroupBox grpTypeOfImport;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton rdbAppend;
        private System.Windows.Forms.RadioButton rdbUpdate;
        private System.Windows.Forms.RadioButton rdbUpdateAndAppend;
        private System.Windows.Forms.ContextMenuStrip cmsStatus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
    }
}