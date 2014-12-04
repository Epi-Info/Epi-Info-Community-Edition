namespace Epi.Windows.ImportExport.Dialogs
{
    partial class PackageForTransportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageForTransportDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtProjectPath = new System.Windows.Forms.TextBox();
            this.lblProjectPath = new System.Windows.Forms.Label();
            this.btnBrowseProject = new System.Windows.Forms.Button();
            this.txtPackagePath = new System.Windows.Forms.TextBox();
            this.lblPackagePath = new System.Windows.Forms.Label();
            this.btnBrowsePackage = new System.Windows.Forms.Button();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtVerifyPassword = new System.Windows.Forms.TextBox();
            this.lblVerifyPassword = new System.Windows.Forms.Label();
            this.grpDataRemoval = new System.Windows.Forms.GroupBox();
            this.btnKeyFields = new System.Windows.Forms.Button();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btnRowFilter = new System.Windows.Forms.Button();
            this.btnGridDataRemoval = new System.Windows.Forms.Button();
            this.btnFormDataRemoval = new System.Windows.Forms.Button();
            this.lblDataRemoval = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbPackageForm = new System.Windows.Forms.ComboBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.lblPackageName = new System.Windows.Forms.Label();
            this.txtPackageName = new System.Windows.Forms.TextBox();
            this.checkboxIncludeTime = new System.Windows.Forms.CheckBox();
            this.grpDataRemoval.SuspendLayout();
            this.SuspendLayout();
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
            // txtProjectPath
            // 
            resources.ApplyResources(this.txtProjectPath, "txtProjectPath");
            this.txtProjectPath.Name = "txtProjectPath";
            this.txtProjectPath.ReadOnly = true;
            this.txtProjectPath.TextChanged += new System.EventHandler(this.txtProjectPath_TextChanged);
            // 
            // lblProjectPath
            // 
            resources.ApplyResources(this.lblProjectPath, "lblProjectPath");
            this.lblProjectPath.Name = "lblProjectPath";
            // 
            // btnBrowseProject
            // 
            resources.ApplyResources(this.btnBrowseProject, "btnBrowseProject");
            this.btnBrowseProject.Name = "btnBrowseProject";
            this.btnBrowseProject.UseVisualStyleBackColor = true;
            this.btnBrowseProject.Click += new System.EventHandler(this.btnBrowseProject_Click);
            // 
            // txtPackagePath
            // 
            resources.ApplyResources(this.txtPackagePath, "txtPackagePath");
            this.txtPackagePath.Name = "txtPackagePath";
            this.txtPackagePath.ReadOnly = true;
            this.txtPackagePath.TextChanged += new System.EventHandler(this.txtPackagePath_TextChanged);
            // 
            // lblPackagePath
            // 
            resources.ApplyResources(this.lblPackagePath, "lblPackagePath");
            this.lblPackagePath.Name = "lblPackagePath";
            // 
            // btnBrowsePackage
            // 
            resources.ApplyResources(this.btnBrowsePackage, "btnBrowsePackage");
            this.btnBrowsePackage.Name = "btnBrowsePackage";
            this.btnBrowsePackage.UseVisualStyleBackColor = true;
            this.btnBrowsePackage.Click += new System.EventHandler(this.btnBrowsePackage_Click);
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtVerifyPassword
            // 
            resources.ApplyResources(this.txtVerifyPassword, "txtVerifyPassword");
            this.txtVerifyPassword.Name = "txtVerifyPassword";
            this.txtVerifyPassword.UseSystemPasswordChar = true;
            // 
            // lblVerifyPassword
            // 
            resources.ApplyResources(this.lblVerifyPassword, "lblVerifyPassword");
            this.lblVerifyPassword.Name = "lblVerifyPassword";
            // 
            // grpDataRemoval
            // 
            resources.ApplyResources(this.grpDataRemoval, "grpDataRemoval");
            this.grpDataRemoval.Controls.Add(this.btnKeyFields);
            this.grpDataRemoval.Controls.Add(this.btnAdvanced);
            this.grpDataRemoval.Controls.Add(this.btnRowFilter);
            this.grpDataRemoval.Controls.Add(this.btnGridDataRemoval);
            this.grpDataRemoval.Controls.Add(this.btnFormDataRemoval);
            this.grpDataRemoval.Controls.Add(this.lblDataRemoval);
            this.grpDataRemoval.Controls.Add(this.txtVerifyPassword);
            this.grpDataRemoval.Controls.Add(this.lblPassword);
            this.grpDataRemoval.Controls.Add(this.lblVerifyPassword);
            this.grpDataRemoval.Controls.Add(this.txtPassword);
            this.grpDataRemoval.Name = "grpDataRemoval";
            this.grpDataRemoval.TabStop = false;
            // 
            // btnKeyFields
            // 
            resources.ApplyResources(this.btnKeyFields, "btnKeyFields");
            this.btnKeyFields.Name = "btnKeyFields";
            this.btnKeyFields.UseVisualStyleBackColor = true;
            this.btnKeyFields.Click += new System.EventHandler(this.btnKeyFields_Click);
            // 
            // btnAdvanced
            // 
            resources.ApplyResources(this.btnAdvanced, "btnAdvanced");
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btnRowFilter
            // 
            resources.ApplyResources(this.btnRowFilter, "btnRowFilter");
            this.btnRowFilter.Name = "btnRowFilter";
            this.btnRowFilter.UseVisualStyleBackColor = true;
            this.btnRowFilter.Click += new System.EventHandler(this.btnRowFilter_Click);
            // 
            // btnGridDataRemoval
            // 
            resources.ApplyResources(this.btnGridDataRemoval, "btnGridDataRemoval");
            this.btnGridDataRemoval.Name = "btnGridDataRemoval";
            this.btnGridDataRemoval.UseVisualStyleBackColor = true;
            this.btnGridDataRemoval.Click += new System.EventHandler(this.btnGridDataRemoval_Click);
            // 
            // btnFormDataRemoval
            // 
            resources.ApplyResources(this.btnFormDataRemoval, "btnFormDataRemoval");
            this.btnFormDataRemoval.Name = "btnFormDataRemoval";
            this.btnFormDataRemoval.UseVisualStyleBackColor = true;
            this.btnFormDataRemoval.Click += new System.EventHandler(this.btnColumnRemoval_Click);
            // 
            // lblDataRemoval
            // 
            this.lblDataRemoval.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.lblDataRemoval, "lblDataRemoval");
            this.lblDataRemoval.Name = "lblDataRemoval";
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoad
            // 
            resources.ApplyResources(this.btnLoad, "btnLoad");
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbPackageForm
            // 
            this.cmbPackageForm.FormattingEnabled = true;
            resources.ApplyResources(this.cmbPackageForm, "cmbPackageForm");
            this.cmbPackageForm.Name = "cmbPackageForm";
            this.cmbPackageForm.SelectedIndexChanged += new System.EventHandler(this.cmbPackageForm_SelectedIndexChanged);
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            // 
            // txtProgress
            // 
            resources.ApplyResources(this.txtProgress, "txtProgress");
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.ReadOnly = true;
            this.txtProgress.TabStop = false;
            // 
            // lbxStatus
            // 
            resources.ApplyResources(this.lbxStatus, "lbxStatus");
            this.lbxStatus.FormattingEnabled = true;
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.TabStop = false;
            // 
            // lblPackageName
            // 
            resources.ApplyResources(this.lblPackageName, "lblPackageName");
            this.lblPackageName.Name = "lblPackageName";
            // 
            // txtPackageName
            // 
            resources.ApplyResources(this.txtPackageName, "txtPackageName");
            this.txtPackageName.Name = "txtPackageName";
            this.txtPackageName.TextChanged += new System.EventHandler(this.txtPackageName_TextChanged);
            this.txtPackageName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPackageName_KeyDown);
            this.txtPackageName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPackageName_KeyPress);
            // 
            // checkboxIncludeTime
            // 
            resources.ApplyResources(this.checkboxIncludeTime, "checkboxIncludeTime");
            this.checkboxIncludeTime.Name = "checkboxIncludeTime";
            this.checkboxIncludeTime.UseVisualStyleBackColor = true;
            this.checkboxIncludeTime.CheckedChanged += new System.EventHandler(this.checkboxIncludeTime_CheckedChanged);
            // 
            // PackageForTransportDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.checkboxIncludeTime);
            this.Controls.Add(this.txtPackageName);
            this.Controls.Add(this.lblPackageName);
            this.Controls.Add(this.lbxStatus);
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbPackageForm);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpDataRemoval);
            this.Controls.Add(this.btnBrowsePackage);
            this.Controls.Add(this.lblPackagePath);
            this.Controls.Add(this.txtPackagePath);
            this.Controls.Add(this.btnBrowseProject);
            this.Controls.Add(this.lblProjectPath);
            this.Controls.Add(this.txtProjectPath);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageForTransportDialog";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.PackageForTransportDialog_Load);
            this.grpDataRemoval.ResumeLayout(false);
            this.grpDataRemoval.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtProjectPath;
        private System.Windows.Forms.Label lblProjectPath;
        private System.Windows.Forms.Button btnBrowseProject;
        private System.Windows.Forms.TextBox txtPackagePath;
        private System.Windows.Forms.Label lblPackagePath;
        private System.Windows.Forms.Button btnBrowsePackage;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtVerifyPassword;
        private System.Windows.Forms.Label lblVerifyPassword;
        private System.Windows.Forms.GroupBox grpDataRemoval;
        private System.Windows.Forms.Label lblDataRemoval;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbPackageForm;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtProgress;
        private System.Windows.Forms.ListBox lbxStatus;
        private System.Windows.Forms.Button btnFormDataRemoval;
        private System.Windows.Forms.Button btnGridDataRemoval;
        private System.Windows.Forms.Button btnRowFilter;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Label lblPackageName;
        private System.Windows.Forms.TextBox txtPackageName;
        private System.Windows.Forms.CheckBox checkboxIncludeTime;
        private System.Windows.Forms.Button btnKeyFields;
    }
}