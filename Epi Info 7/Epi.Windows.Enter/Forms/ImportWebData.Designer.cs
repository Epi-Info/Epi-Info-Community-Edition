namespace Epi.Enter.Forms
{
    partial class ImportWebDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportWebDataForm));
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.textProject = new System.Windows.Forms.TextBox();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.textOrganization = new System.Windows.Forms.TextBox();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.textData = new System.Windows.Forms.TextBox();
            this.lblDataGUID = new System.Windows.Forms.Label();
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
            this.rdbFinalMode = new System.Windows.Forms.RadioButton();
            this.rdbDraftMode = new System.Windows.Forms.RadioButton();
            this.rdbAllRecords = new System.Windows.Forms.RadioButton();
            this.rdbSubmittedRecords = new System.Windows.Forms.RadioButton();
            this.grpImportMode = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkIncremental = new System.Windows.Forms.CheckBox();
            this.groupImportInfo.SuspendLayout();
            this.cmsStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpImportMode.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.Name = "lblDescription";
            // 
            // lblProjectFile
            // 
            resources.ApplyResources(this.lblProjectFile, "lblProjectFile");
            this.lblProjectFile.Name = "lblProjectFile";
            // 
            // textProject
            // 
            resources.ApplyResources(this.textProject, "textProject");
            this.textProject.Name = "textProject";
            this.textProject.TextChanged += new System.EventHandler(this.textProject_TextChanged);
            // 
            // groupImportInfo
            // 
            this.groupImportInfo.Controls.Add(this.textOrganization);
            this.groupImportInfo.Controls.Add(this.lblOrganization);
            this.groupImportInfo.Controls.Add(this.textData);
            this.groupImportInfo.Controls.Add(this.lblDataGUID);
            this.groupImportInfo.Controls.Add(this.textProject);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            resources.ApplyResources(this.groupImportInfo, "groupImportInfo");
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.TabStop = false;
            // 
            // textOrganization
            // 
            resources.ApplyResources(this.textOrganization, "textOrganization");
            this.textOrganization.Name = "textOrganization";
            // 
            // lblOrganization
            // 
            resources.ApplyResources(this.lblOrganization, "lblOrganization");
            this.lblOrganization.Name = "lblOrganization";
            // 
            // textData
            // 
            resources.ApplyResources(this.textData, "textData");
            this.textData.Name = "textData";
            // 
            // lblDataGUID
            // 
            resources.ApplyResources(this.lblDataGUID, "lblDataGUID");
            this.lblDataGUID.Name = "lblDataGUID";
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
            // rdbFinalMode
            // 
            resources.ApplyResources(this.rdbFinalMode, "rdbFinalMode");
            this.rdbFinalMode.Name = "rdbFinalMode";
            this.rdbFinalMode.TabStop = true;
            this.rdbFinalMode.UseVisualStyleBackColor = true;
            // 
            // rdbDraftMode
            // 
            resources.ApplyResources(this.rdbDraftMode, "rdbDraftMode");
            this.rdbDraftMode.Name = "rdbDraftMode";
            this.rdbDraftMode.TabStop = true;
            this.rdbDraftMode.UseVisualStyleBackColor = true;
            // 
            // rdbAllRecords
            // 
            resources.ApplyResources(this.rdbAllRecords, "rdbAllRecords");
            this.rdbAllRecords.Name = "rdbAllRecords";
            this.rdbAllRecords.TabStop = true;
            this.rdbAllRecords.UseVisualStyleBackColor = true;
            this.rdbAllRecords.Click += new System.EventHandler(this.rdbAllRecords_Click);
            // 
            // rdbSubmittedRecords
            // 
            resources.ApplyResources(this.rdbSubmittedRecords, "rdbSubmittedRecords");
            this.rdbSubmittedRecords.Name = "rdbSubmittedRecords";
            this.rdbSubmittedRecords.TabStop = true;
            this.rdbSubmittedRecords.UseVisualStyleBackColor = true;
            this.rdbSubmittedRecords.Click += new System.EventHandler(this.rdbSubmittedRecords_Click);
            // 
            // grpImportMode
            // 
            this.grpImportMode.Controls.Add(this.rdbDraftMode);
            this.grpImportMode.Controls.Add(this.rdbFinalMode);
            resources.ApplyResources(this.grpImportMode, "grpImportMode");
            this.grpImportMode.Name = "grpImportMode";
            this.grpImportMode.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkIncremental);
            this.groupBox1.Controls.Add(this.rdbSubmittedRecords);
            this.groupBox1.Controls.Add(this.rdbAllRecords);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // chkIncremental
            // 
            this.chkIncremental.Checked = true;
            this.chkIncremental.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.chkIncremental, "chkIncremental");
            this.chkIncremental.Name = "chkIncremental";
            this.chkIncremental.UseVisualStyleBackColor = true;
            // 
            // ImportWebDataForm
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpImportMode);
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
            this.Name = "ImportWebDataForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportDataForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportDataForm_Load);
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
            this.cmsStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpImportMode.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblProjectFile;
        private System.Windows.Forms.TextBox textProject;
        private System.Windows.Forms.GroupBox groupImportInfo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lbxStatus;
        private System.Windows.Forms.TextBox textProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Label lblWarningMessage;
        private System.Windows.Forms.TextBox textData;
        private System.Windows.Forms.Label lblDataGUID;
        private System.Windows.Forms.TextBox textOrganization;
        private System.Windows.Forms.Label lblOrganization;
        private System.Windows.Forms.RadioButton rdbFinalMode;
        private System.Windows.Forms.RadioButton rdbDraftMode;
        private System.Windows.Forms.RadioButton rdbAllRecords;
        private System.Windows.Forms.RadioButton rdbSubmittedRecords;
        private System.Windows.Forms.GroupBox grpImportMode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ContextMenuStrip cmsStatus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
        private System.Windows.Forms.CheckBox chkIncremental;
    }
}