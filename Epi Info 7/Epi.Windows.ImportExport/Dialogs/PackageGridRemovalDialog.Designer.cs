namespace Epi.Windows.ImportExport.Dialogs
{
    partial class PackageGridRemovalDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageGridRemovalDialog));
            this.lblFields = new System.Windows.Forms.Label();
            this.lbxGridColumns = new System.Windows.Forms.ListBox();
            this.lblForm = new System.Windows.Forms.Label();
            this.cmbGridSelector = new System.Windows.Forms.ComboBox();
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
            // lbxGridColumns
            // 
            this.lbxGridColumns.FormattingEnabled = true;
            resources.ApplyResources(this.lbxGridColumns, "lbxGridColumns");
            this.lbxGridColumns.Name = "lbxGridColumns";
            this.lbxGridColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbxGridColumns.SelectedIndexChanged += new System.EventHandler(this.lbxFields_SelectedIndexChanged);
            // 
            // lblForm
            // 
            resources.ApplyResources(this.lblForm, "lblForm");
            this.lblForm.Name = "lblForm";
            // 
            // cmbGridSelector
            // 
            this.cmbGridSelector.FormattingEnabled = true;
            resources.ApplyResources(this.cmbGridSelector, "cmbGridSelector");
            this.cmbGridSelector.Name = "cmbGridSelector";
            this.cmbGridSelector.SelectedIndexChanged += new System.EventHandler(this.cmbFormSelector_SelectedIndexChanged);
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
            // PackageGridRemovalDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblFields);
            this.Controls.Add(this.lbxGridColumns);
            this.Controls.Add(this.lblForm);
            this.Controls.Add(this.cmbGridSelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageGridRemovalDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblFields;
        private System.Windows.Forms.ListBox lbxGridColumns;
        private System.Windows.Forms.Label lblForm;
        private System.Windows.Forms.ComboBox cmbGridSelector;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
    }
}