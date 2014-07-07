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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportPhoneDataForm));
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupImportInfo = new System.Windows.Forms.GroupBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPhoneDataFile = new System.Windows.Forms.TextBox();
            this.lblProjectFile = new System.Windows.Forms.Label();
            this.cmbImportType = new System.Windows.Forms.ComboBox();
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
            // groupImportInfo
            // 
            this.groupImportInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupImportInfo.Controls.Add(this.txtPassword);
            this.groupImportInfo.Controls.Add(this.lblPassword);
            this.groupImportInfo.Controls.Add(this.btnBrowse);
            this.groupImportInfo.Controls.Add(this.txtPhoneDataFile);
            this.groupImportInfo.Controls.Add(this.lblProjectFile);
            this.groupImportInfo.Controls.Add(this.cmbImportType);
            this.groupImportInfo.Controls.Add(this.lblImportType);
            this.groupImportInfo.Location = new System.Drawing.Point(12, 120);
            this.groupImportInfo.Name = "groupImportInfo";
            this.groupImportInfo.Size = new System.Drawing.Size(505, 149);
            this.groupImportInfo.TabIndex = 6;
            this.groupImportInfo.TabStop = false;
            this.groupImportInfo.Text = "Import information";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(9, 76);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(287, 20);
            this.txtPassword.TabIndex = 11;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(6, 60);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 10;
            this.lblPassword.Text = "Password:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(472, 35);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 23);
            this.btnBrowse.TabIndex = 9;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtPhoneDataFile
            // 
            this.txtPhoneDataFile.Location = new System.Drawing.Point(9, 37);
            this.txtPhoneDataFile.Name = "txtPhoneDataFile";
            this.txtPhoneDataFile.Size = new System.Drawing.Size(457, 20);
            this.txtPhoneDataFile.TabIndex = 8;
            this.txtPhoneDataFile.TextChanged += new System.EventHandler(this.txtPhoneDataFile_TextChanged);
            // 
            // lblProjectFile
            // 
            this.lblProjectFile.Location = new System.Drawing.Point(6, 21);
            this.lblProjectFile.Name = "lblProjectFile";
            this.lblProjectFile.Size = new System.Drawing.Size(460, 13);
            this.lblProjectFile.TabIndex = 7;
            this.lblProjectFile.Text = "Mobile sync file:";
            // 
            // cmbImportType
            // 
            this.cmbImportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImportType.FormattingEnabled = true;
            this.cmbImportType.Items.AddRange(new object[] {
            "Update and append records",
            "Update records only",
            "Append records only"});
            this.cmbImportType.Location = new System.Drawing.Point(9, 115);
            this.cmbImportType.Name = "cmbImportType";
            this.cmbImportType.Size = new System.Drawing.Size(166, 21);
            this.cmbImportType.TabIndex = 4;
            // 
            // lblImportType
            // 
            this.lblImportType.Location = new System.Drawing.Point(6, 99);
            this.lblImportType.Name = "lblImportType";
            this.lblImportType.Size = new System.Drawing.Size(166, 13);
            this.lblImportType.TabIndex = 6;
            this.lblImportType.Text = "Type of import:";
            // 
            // textProgress
            // 
            this.textProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textProgress.Location = new System.Drawing.Point(12, 363);
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
            this.progressBar.Location = new System.Drawing.Point(12, 389);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(505, 23);
            this.progressBar.TabIndex = 12;
            this.progressBar.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(442, 419);
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
            this.btnOK.Location = new System.Drawing.Point(361, 419);
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
            this.lbxStatus.Location = new System.Drawing.Point(12, 275);
            this.lbxStatus.Name = "lbxStatus";
            this.lbxStatus.Size = new System.Drawing.Size(505, 69);
            this.lbxStatus.TabIndex = 9;
            this.lbxStatus.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Epi.Enter.Properties.Resources.warning;
            this.pictureBox1.Location = new System.Drawing.Point(15, 81);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(18, 18);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // lblWarning
            // 
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(36, 81);
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
            this.lblWarningMessage.Text = "Import operations are permanent and cannot be undone. Be sure the form structures" +
    " are the same before proceeding.";
            // 
            // ImportPhoneDataForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(529, 454);
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
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import mobile sync file";
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
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPhoneDataFile;
        private System.Windows.Forms.Label lblProjectFile;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
    }
}