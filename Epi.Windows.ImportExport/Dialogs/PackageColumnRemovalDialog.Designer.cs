namespace Epi.Windows.ImportExport.Dialogs
{
    partial class PackageColumnRemovalDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageColumnRemovalDialog));
            this.lblFields = new System.Windows.Forms.Label();
            this.lbxFields = new System.Windows.Forms.ListBox();
            this.lblForm = new System.Windows.Forms.Label();
            this.cmbFormSelector = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFields
            // 
            resources.ApplyResources(this.lblFields, "lblFields");
            this.lblFields.Name = "lblFields";
            // 
            // lbxFields
            // 
            resources.ApplyResources(this.lbxFields, "lbxFields");
            this.lbxFields.FormattingEnabled = true;
            this.lbxFields.Name = "lbxFields";
            this.lbxFields.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbxFields.SelectedIndexChanged += new System.EventHandler(this.lbxFields_SelectedIndexChanged);
            // 
            // lblForm
            // 
            resources.ApplyResources(this.lblForm, "lblForm");
            this.lblForm.Name = "lblForm";
            // 
            // cmbFormSelector
            // 
            resources.ApplyResources(this.cmbFormSelector, "cmbFormSelector");
            this.cmbFormSelector.FormattingEnabled = true;
            this.cmbFormSelector.Name = "cmbFormSelector";
            this.cmbFormSelector.SelectedIndexChanged += new System.EventHandler(this.cmbFormSelector_SelectedIndexChanged);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // PackageColumnRemovalDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblFields);
            this.Controls.Add(this.lbxFields);
            this.Controls.Add(this.lblForm);
            this.Controls.Add(this.cmbFormSelector);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageColumnRemovalDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblFields;
        private System.Windows.Forms.ListBox lbxFields;
        private System.Windows.Forms.Label lblForm;
        private System.Windows.Forms.ComboBox cmbFormSelector;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
    }
}