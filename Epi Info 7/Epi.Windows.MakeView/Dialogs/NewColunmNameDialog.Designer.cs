using System.Windows.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
    partial class NewColumnNameDialog : Form
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewColumnNameDialog));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblNewColumnName = new System.Windows.Forms.Label();
            this.textBoxColumnName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            // 
            // lblNewColumnName
            // 
            this.lblNewColumnName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblNewColumnName, "lblNewColumnName");
            this.lblNewColumnName.Name = "lblNewColumnName";
            // 
            // textBoxColumnName
            // 
            resources.ApplyResources(this.textBoxColumnName, "textBoxColumnName");
            this.textBoxColumnName.Name = "textBoxColumnName";
            this.textBoxColumnName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxColumnName_KeyUp);
            // 
            // NewColumnNameDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.textBoxColumnName);
            this.Controls.Add(this.lblNewColumnName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewColumnNameDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblNewColumnName;
        private System.Windows.Forms.TextBox textBoxColumnName;
    }
}