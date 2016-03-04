namespace Epi.Enter.Forms
{
    partial class ImportWebEnterDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportWebEnterDataForm));
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.textProject = new System.Windows.Forms.TextBox();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.chkIncremental = new System.Windows.Forms.CheckBox();
            this.textOrganization = new System.Windows.Forms.TextBox();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.textData = new System.Windows.Forms.TextBox();
            this.lblDataGUID = new System.Windows.Forms.Label();
            this.grpImportMode = new System.Windows.Forms.GroupBox();
            this.rdbDraftMode = new System.Windows.Forms.RadioButton();
            this.rdbFinalMode = new System.Windows.Forms.RadioButton();
            this.textProgress = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblWarningMessage = new System.Windows.Forms.Label();
            this.groupImportInfo.SuspendLayout();
            this.grpImportMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.groupImportInfo.Controls.Add(this.chkIncremental);
            this.groupImportInfo.Controls.Add(this.textOrganization);
            this.groupImportInfo.Controls.Add(this.lblOrganization);
            this.groupImportInfo.Controls.Add(this.textData);
            this.groupImportInfo.Controls.Add(this.lblDataGUID);
            this.groupImportInfo.Controls.Add(this.textProject);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Controls.Add(this.grpImportMode);
            resources.ApplyResources(this.groupImportInfo, "groupImportInfo");
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.TabStop = false;
            // 
            // chkIncremental
            // 
            resources.ApplyResources(this.chkIncremental, "chkIncremental");
            this.chkIncremental.Name = "chkIncremental";
            this.chkIncremental.UseVisualStyleBackColor = true;
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
            // grpImportMode
            // 
            this.grpImportMode.Controls.Add(this.rdbDraftMode);
            this.grpImportMode.Controls.Add(this.rdbFinalMode);
            resources.ApplyResources(this.grpImportMode, "grpImportMode");
            this.grpImportMode.Name = "grpImportMode";
            this.grpImportMode.TabStop = false;
            // 
            // rdbDraftMode
            // 
            resources.ApplyResources(this.rdbDraftMode, "rdbDraftMode");
            this.rdbDraftMode.Name = "rdbDraftMode";
            this.rdbDraftMode.TabStop = true;
            this.rdbDraftMode.UseVisualStyleBackColor = true;
            // 
            // rdbFinalMode
            // 
            resources.ApplyResources(this.rdbFinalMode, "rdbFinalMode");
            this.rdbFinalMode.Name = "rdbFinalMode";
            this.rdbFinalMode.TabStop = true;
            this.rdbFinalMode.UseVisualStyleBackColor = true;
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
            this.lbxStatus.FormattingEnabled = true;
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.TabStop = false;
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
            // ImportWebEnterDataForm
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
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
            this.Name = "ImportWebEnterDataForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportDataForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportDataForm_Load);
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
            this.grpImportMode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.GroupBox grpImportMode;
        private System.Windows.Forms.RadioButton rdbFinalMode;
        private System.Windows.Forms.CheckBox chkIncremental;
        public System.Windows.Forms.RadioButton rdbDraftMode;
    }
}