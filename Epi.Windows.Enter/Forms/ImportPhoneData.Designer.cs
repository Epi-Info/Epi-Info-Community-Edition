namespace Epi.Enter.Forms
{
    partial class ImportPhoneDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportPhoneDataForm));
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPhoneDataFile = new System.Windows.Forms.TextBox();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.textProgress = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.cmsStatus = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblWarningMessage = new System.Windows.Forms.Label();
            this.grpTypeOfImport = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.viewsList = new System.Windows.Forms.ComboBox();
            this.rdbAppend = new System.Windows.Forms.RadioButton();
            this.rdbUpdate = new System.Windows.Forms.RadioButton();
            this.rdbUpdateAndAppend = new System.Windows.Forms.RadioButton();
            this.groupImportInfo.SuspendLayout();
            this.cmsStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpTypeOfImport.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.Name = "lblDescription";
            // 
            // groupImportInfo
            // 
            resources.ApplyResources(this.groupImportInfo, "groupImportInfo");
            this.groupImportInfo.Controls.Add(this.txtPassword);
            this.groupImportInfo.Controls.Add(this.lblPassword);
            this.groupImportInfo.Controls.Add(this.btnBrowse);
            this.groupImportInfo.Controls.Add(this.txtPhoneDataFile);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.TabStop = false;
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // btnBrowse
            // 
            resources.ApplyResources(this.btnBrowse, "btnBrowse");
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtPhoneDataFile
            // 
            resources.ApplyResources(this.txtPhoneDataFile, "txtPhoneDataFile");
            this.txtPhoneDataFile.Name = "txtPhoneDataFile";
            this.txtPhoneDataFile.TextChanged += new System.EventHandler(this.txtPhoneDataFile_TextChanged);
            // 
            // lblProjectFile
            // 
            resources.ApplyResources(this.lblProjectFile, "lblProjectFile");
            this.lblProjectFile.Name = "lblProjectFile";
            // 
            // textProgress
            // 
            resources.ApplyResources(this.textProgress, "textProgress");
            this.textProgress.Name = "textProgress";
            this.textProgress.ReadOnly = true;
            this.textProgress.TabStop = false;
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
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
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Epi.Enter.Properties.Resources.warning;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // lblWarning
            // 
            resources.ApplyResources(this.lblWarning, "lblWarning");
            this.lblWarning.Name = "lblWarning";
            // 
            // lblWarningMessage
            // 
            resources.ApplyResources(this.lblWarningMessage, "lblWarningMessage");
            this.lblWarningMessage.Name = "lblWarningMessage";
            // 
            // grpTypeOfImport
            // 
            this.grpTypeOfImport.Controls.Add(this.label1);
            this.grpTypeOfImport.Controls.Add(this.viewsList);
            this.grpTypeOfImport.Controls.Add(this.rdbAppend);
            this.grpTypeOfImport.Controls.Add(this.rdbUpdate);
            this.grpTypeOfImport.Controls.Add(this.rdbUpdateAndAppend);
            resources.ApplyResources(this.grpTypeOfImport, "grpTypeOfImport");
            this.grpTypeOfImport.Name = "grpTypeOfImport";
            this.grpTypeOfImport.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // viewsList
            // 
            this.viewsList.FormattingEnabled = true;
            resources.ApplyResources(this.viewsList, "viewsList");
            this.viewsList.Name = "viewsList";
            this.viewsList.SelectedIndexChanged += new System.EventHandler(this.viewsList_SelectedIndexChanged);
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
            this.rdbUpdateAndAppend.CheckedChanged += new System.EventHandler(this.rdbUpdateAndAppend_CheckedChanged);
            // 
            // ImportPhoneDataForm
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.grpTypeOfImport);
            this.Controls.Add(this.lblWarningMessage);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lbxStatus);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.groupImportInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportPhoneDataForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportDataForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportDataForm_Load);
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
            this.cmsStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpTypeOfImport.ResumeLayout(false);
            this.grpTypeOfImport.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.GroupBox groupImportInfo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lbxStatus;
        private System.Windows.Forms.TextBox textProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Label lblWarningMessage;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPhoneDataFile;
        private System.Windows.Forms.Label lblProjectFile;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.GroupBox grpTypeOfImport;
        private System.Windows.Forms.RadioButton rdbAppend;
        private System.Windows.Forms.RadioButton rdbUpdate;
        private System.Windows.Forms.RadioButton rdbUpdateAndAppend;
        private System.Windows.Forms.ContextMenuStrip cmsStatus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
        private System.Windows.Forms.ComboBox viewsList;
        private System.Windows.Forms.Label label1;
    }
}