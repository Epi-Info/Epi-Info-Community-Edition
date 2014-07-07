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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportWebDataForm));
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.textProject = new System.Windows.Forms.TextBox();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.textOrganization = new System.Windows.Forms.TextBox();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.textData = new System.Windows.Forms.TextBox();
            this.cmbImportType = new System.Windows.Forms.ComboBox();
            this.lblDataGUID = new System.Windows.Forms.Label();
            this.lblImportType = new System.Windows.Forms.Label();
            this.textProgress = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lbxStatus = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblWarningMessage = new System.Windows.Forms.Label();
            this.groupImportInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(505, 59);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // lblProjectFile
            // 
            this.lblProjectFile.Location = new System.Drawing.Point(6, 24);
            this.lblProjectFile.Name = "lblProjectFile";
            this.lblProjectFile.Size = new System.Drawing.Size(460, 13);
            this.lblProjectFile.TabIndex = 1;
            this.lblProjectFile.Text = "Web Survey Key:";
            // 
            // textProject
            // 
            this.textProject.Location = new System.Drawing.Point(9, 40);
            this.textProject.Name = "textProject";
            this.textProject.Size = new System.Drawing.Size(487, 20);
            this.textProject.TabIndex = 1;
            this.textProject.TextChanged += new System.EventHandler(this.textProject_TextChanged);
            // 
            // groupImportInfo
            // 
            this.groupImportInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupImportInfo.Controls.Add(this.textOrganization);
            this.groupImportInfo.Controls.Add(this.lblOrganization);
            this.groupImportInfo.Controls.Add(this.textData);
            this.groupImportInfo.Controls.Add(this.cmbImportType);
            this.groupImportInfo.Controls.Add(this.lblDataGUID);
            this.groupImportInfo.Controls.Add(this.lblImportType);
            this.groupImportInfo.Controls.Add(this.textProject);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Location = new System.Drawing.Point(12, 122);
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.Size = new System.Drawing.Size(505, 190);
            this.groupImportInfo.TabIndex = 6;
            this.groupImportInfo.TabStop = false;
            this.groupImportInfo.Text = "Import information";
            // 
            // textOrganization
            // 
            this.textOrganization.Location = new System.Drawing.Point(9, 79);
            this.textOrganization.Name = "textOrganization";
            this.textOrganization.Size = new System.Drawing.Size(487, 20);
            this.textOrganization.TabIndex = 2;
            // 
            // lblOrganization
            // 
            this.lblOrganization.Location = new System.Drawing.Point(6, 63);
            this.lblOrganization.Name = "lblOrganization";
            this.lblOrganization.Size = new System.Drawing.Size(460, 13);
            this.lblOrganization.TabIndex = 18;
            this.lblOrganization.Text = "Organization Key:";
            // 
            // textData
            // 
            this.textData.Location = new System.Drawing.Point(9, 118);
            this.textData.Name = "textData";
            this.textData.PasswordChar = '*';
            this.textData.Size = new System.Drawing.Size(487, 20);
            this.textData.TabIndex = 3;
            // 
            // cmbImportType
            // 
            this.cmbImportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImportType.FormattingEnabled = true;
            this.cmbImportType.Items.AddRange(new object[] {
            "Completed records in draft mode",
            "All records in draft mode",
            "Completed records in final mode",
            "All records in final mode"});
            this.cmbImportType.Location = new System.Drawing.Point(9, 157);
            this.cmbImportType.Name = "cmbImportType";
            this.cmbImportType.Size = new System.Drawing.Size(166, 21);
            this.cmbImportType.TabIndex = 4;
            // 
            // lblDataGUID
            // 
            this.lblDataGUID.Location = new System.Drawing.Point(6, 102);
            this.lblDataGUID.Name = "lblDataGUID";
            this.lblDataGUID.Size = new System.Drawing.Size(460, 13);
            this.lblDataGUID.TabIndex = 16;
            this.lblDataGUID.Text = "Security Token:";
            // 
            // lblImportType
            // 
            this.lblImportType.Location = new System.Drawing.Point(6, 141);
            this.lblImportType.Name = "lblImportType";
            this.lblImportType.Size = new System.Drawing.Size(166, 13);
            this.lblImportType.TabIndex = 6;
            this.lblImportType.Text = "Type of import:";
            // 
            // textProgress
            // 
            this.textProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textProgress.Location = new System.Drawing.Point(12, 445);
            this.textProgress.Name = "textProgress";
            this.textProgress.ReadOnly = true;
            this.textProgress.Size = new System.Drawing.Size(505, 20);
            this.textProgress.TabIndex = 13;
            this.textProgress.TabStop = false;
            this.textProgress.Text = "Ready";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 471);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(505, 23);
            this.progressBar.TabIndex = 12;
            this.progressBar.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(442, 501);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(361, 501);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "Import";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lbxStatus
            // 
            this.lbxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxStatus.FormattingEnabled = true;
            this.lbxStatus.HorizontalScrollbar = true;
            this.lbxStatus.Location = new System.Drawing.Point(12, 318);
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.Size = new System.Drawing.Size(505, 121);
            this.lbxStatus.TabIndex = 9;
            this.lbxStatus.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Epi.Enter.Properties.Resources.warning;
            this.pictureBox1.Location = new System.Drawing.Point(15, 87);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(18, 18);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // lblWarning
            // 
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(36, 87);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(71, 18);
            this.lblWarning.TabIndex = 14;
            this.lblWarning.Text = "Warning:";
            // 
            // lblWarningMessage
            // 
            this.lblWarningMessage.Location = new System.Drawing.Point(103, 75);
            this.lblWarningMessage.Name = "lblWarningMessage";
            this.lblWarningMessage.Size = new System.Drawing.Size(414, 44);
            this.lblWarningMessage.TabIndex = 15;
            this.lblWarningMessage.Text = resources.GetString("lblWarningMessage.Text");
            // 
            // ImportWebDataForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(529, 536);
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
            this.ShowIcon = false;
            this.Text = "Import web form data";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportDataForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportDataForm_Load);
            this.groupImportInfo.ResumeLayout(false);
            this.groupImportInfo.PerformLayout();
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
        private System.Windows.Forms.ComboBox cmbImportType;
        private System.Windows.Forms.Label lblImportType;
        private System.Windows.Forms.TextBox textData;
        private System.Windows.Forms.Label lblDataGUID;
        private System.Windows.Forms.TextBox textOrganization;
        private System.Windows.Forms.Label lblOrganization;
    }
}