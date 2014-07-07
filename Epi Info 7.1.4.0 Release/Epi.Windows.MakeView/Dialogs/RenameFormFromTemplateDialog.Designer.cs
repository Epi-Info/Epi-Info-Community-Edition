namespace Epi.Windows.MakeView.Dialogs
{
    partial class RenameFormFromTemplatePageDialog : System.Windows.Forms.Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameFormFromTemplatePageDialog));
            this.textBoxFormName = new System.Windows.Forms.TextBox();
            this.labelFormName = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxFormName
            // 
            resources.ApplyResources(this.textBoxFormName, "textBoxFormName");
            this.textBoxFormName.Name = "textBoxFormName";
            this.textBoxFormName.TextChanged += new System.EventHandler(this.textBoxFormName_TextChanged);
            // 
            // labelFormName
            // 
            resources.ApplyResources(this.labelFormName, "labelFormName");
            this.labelFormName.Name = "labelFormName";
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // RenameFormFromTemplatePageDialog
            // 
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelFormName);
            this.Controls.Add(this.textBoxFormName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameFormFromTemplatePageDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.RenamePage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private System.Windows.Forms.TextBox textBoxFormName;
        private System.Windows.Forms.Label labelFormName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button btnCancel;
    }
}