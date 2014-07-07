namespace Epi.Windows.Analysis.Controls
{
    partial class ProjectBasedPgmControl
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnFindProject = new System.Windows.Forms.Button();
			this.txtProject = new System.Windows.Forms.TextBox();
			this.lblProject = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtComment = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.lbxPgms = new System.Windows.Forms.ListBox();
			this.btnTextFile = new System.Windows.Forms.Button();
			this.btnHelp = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.lblDateModified = new System.Windows.Forms.Label();
			this.lblDateCreated = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtPgmName = new System.Windows.Forms.TextBox();
			this.txtDateUpdated = new System.Windows.Forms.TextBox();
			this.txtDateCreated = new System.Windows.Forms.TextBox();
			this.txtAuthor = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnFindProject
			// 
			this.btnFindProject.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnFindProject.Location = new System.Drawing.Point(400, 40);
			this.btnFindProject.Name = "btnFindProject";
			this.btnFindProject.Size = new System.Drawing.Size(24, 16);
			this.btnFindProject.TabIndex = 27;
			this.btnFindProject.Text = "...";
			// 
			// txtProject
			// 
			this.txtProject.Location = new System.Drawing.Point(8, 38);
			this.txtProject.Name = "txtProject";
			this.txtProject.Size = new System.Drawing.Size(384, 20);
			this.txtProject.TabIndex = 26;
			this.txtProject.Text = "";
			// 
			// lblProject
			// 
			this.lblProject.Location = new System.Drawing.Point(8, 16);
			this.lblProject.Name = "lblProject";
			this.lblProject.Size = new System.Drawing.Size(408, 16);
			this.lblProject.TabIndex = 25;
			this.lblProject.Text = "&Project";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(152, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(264, 23);
			this.label3.TabIndex = 33;
			this.label3.Text = "Comments";
			// 
			// txtComment
			// 
			this.txtComment.Location = new System.Drawing.Point(152, 96);
			this.txtComment.Multiline = true;
			this.txtComment.Name = "txtComment";
			this.txtComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtComment.Size = new System.Drawing.Size(264, 136);
			this.txtComment.TabIndex = 32;
			this.txtComment.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 23);
			this.label1.TabIndex = 31;
			this.label1.Text = "Programs";
			// 
			// lbxPgms
			// 
			this.lbxPgms.Location = new System.Drawing.Point(8, 97);
			this.lbxPgms.Name = "lbxPgms";
			this.lbxPgms.Size = new System.Drawing.Size(136, 134);
			this.lbxPgms.TabIndex = 30;
			// 
			// btnTextFile
			// 
			this.btnTextFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnTextFile.Location = new System.Drawing.Point(256, 360);
			this.btnTextFile.Name = "btnTextFile";
			this.btnTextFile.TabIndex = 50;
			this.btnTextFile.Text = "&Text File";
			// 
			// btnHelp
			// 
			this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnHelp.Location = new System.Drawing.Point(336, 360);
			this.btnHelp.Name = "btnHelp"; btnHelp.Enabled = false;
			this.btnHelp.TabIndex = 49;
			this.btnHelp.Text = "&Help";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(88, 360);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 47;
			this.btnCancel.Text = "Cancel";
			// 
			// btnOK
			// 
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Location = new System.Drawing.Point(8, 360);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 46;
			this.btnOK.Text = "OK";
			// 
			// btnDelete
			// 
			this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnDelete.Location = new System.Drawing.Point(176, 360);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.TabIndex = 48;
			this.btnDelete.Text = "&Delete";
			// 
			// lblDateModified
			// 
			this.lblDateModified.Location = new System.Drawing.Point(208, 304);
			this.lblDateModified.Name = "lblDateModified";
			this.lblDateModified.Size = new System.Drawing.Size(184, 23);
			this.lblDateModified.TabIndex = 45;
			this.lblDateModified.Text = "Date Modified";
			// 
			// lblDateCreated
			// 
			this.lblDateCreated.Location = new System.Drawing.Point(8, 304);
			this.lblDateCreated.Name = "lblDateCreated";
			this.lblDateCreated.Size = new System.Drawing.Size(184, 23);
			this.lblDateCreated.TabIndex = 44;
			this.lblDateCreated.Text = "Date Created";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(208, 256);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(184, 23);
			this.label4.TabIndex = 43;
			this.label4.Text = "Author";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 256);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 24);
			this.label2.TabIndex = 42;
			this.label2.Text = "Program Name";
			// 
			// txtPgmName
			// 
			this.txtPgmName.Location = new System.Drawing.Point(8, 280);
			this.txtPgmName.Name = "txtPgmName";
			this.txtPgmName.Size = new System.Drawing.Size(184, 20);
			this.txtPgmName.TabIndex = 41;
			this.txtPgmName.Text = "";
			// 
			// txtDateUpdated
			// 
			this.txtDateUpdated.Location = new System.Drawing.Point(208, 328);
			this.txtDateUpdated.Name = "txtDateUpdated";
			this.txtDateUpdated.ReadOnly = true;
			this.txtDateUpdated.Size = new System.Drawing.Size(184, 20);
			this.txtDateUpdated.TabIndex = 39;
			this.txtDateUpdated.TabStop = false;
			this.txtDateUpdated.Text = "";
			// 
			// txtDateCreated
			// 
			this.txtDateCreated.Location = new System.Drawing.Point(8, 328);
			this.txtDateCreated.Name = "txtDateCreated";
			this.txtDateCreated.ReadOnly = true;
			this.txtDateCreated.Size = new System.Drawing.Size(184, 20);
			this.txtDateCreated.TabIndex = 38;
			this.txtDateCreated.TabStop = false;
			this.txtDateCreated.Text = "";
			// 
			// txtAuthor
			// 
			this.txtAuthor.Location = new System.Drawing.Point(208, 280);
			this.txtAuthor.Name = "txtAuthor";
			this.txtAuthor.Size = new System.Drawing.Size(184, 20);
			this.txtAuthor.TabIndex = 40;
			this.txtAuthor.Text = "";
			// 
			// ProjectBasedPgm
			// 
			this.Controls.Add(this.btnTextFile);
			this.Controls.Add(this.btnHelp);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnDelete);
			this.Controls.Add(this.lblDateModified);
			this.Controls.Add(this.lblDateCreated);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtPgmName);
			this.Controls.Add(this.txtDateUpdated);
			this.Controls.Add(this.txtDateCreated);
			this.Controls.Add(this.txtAuthor);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtComment);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lbxPgms);
			this.Controls.Add(this.btnFindProject);
			this.Controls.Add(this.txtProject);
			this.Controls.Add(this.lblProject);
			this.Name = "ProjectBasedPgm";
			this.Size = new System.Drawing.Size(432, 400);
			this.ResumeLayout(false);

		}
		#endregion



		private System.Windows.Forms.Button btnFindProject;
		private System.Windows.Forms.TextBox txtProject;
		private System.Windows.Forms.Label lblProject;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtComment;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox lbxPgms;
		private System.Windows.Forms.Button btnTextFile;
		private System.Windows.Forms.Button btnHelp;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Label lblDateModified;
		private System.Windows.Forms.Label lblDateCreated;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtPgmName;
		private System.Windows.Forms.TextBox txtDateUpdated;
		private System.Windows.Forms.TextBox txtDateCreated;
		private System.Windows.Forms.TextBox txtAuthor;
    }
}