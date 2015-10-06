namespace Epi.Windows.ImportExport.Dialogs
{
    partial class PackageRecordSelectionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageRecordSelectionDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbFieldName = new System.Windows.Forms.ComboBox();
            this.lblField1 = new System.Windows.Forms.Label();
            this.cmbFieldOperator = new System.Windows.Forms.ComboBox();
            this.txtFieldValue = new System.Windows.Forms.TextBox();
            this.lblField1Op = new System.Windows.Forms.Label();
            this.lblField1Val = new System.Windows.Forms.Label();
            this.lbxRecordFilters = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.cmbFieldValue = new System.Windows.Forms.ComboBox();
            this.dtpFieldValue = new System.Windows.Forms.DateTimePicker();
            this.btnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
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
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbFieldName
            // 
            resources.ApplyResources(this.cmbFieldName, "cmbFieldName");
            this.cmbFieldName.FormattingEnabled = true;
            this.cmbFieldName.Name = "cmbFieldName";
            this.cmbFieldName.SelectedIndexChanged += new System.EventHandler(this.cmbField1Name_SelectedIndexChanged);
            // 
            // lblField1
            // 
            resources.ApplyResources(this.lblField1, "lblField1");
            this.lblField1.Name = "lblField1";
            // 
            // cmbFieldOperator
            // 
            resources.ApplyResources(this.cmbFieldOperator, "cmbFieldOperator");
            this.cmbFieldOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFieldOperator.FormattingEnabled = true;
            this.cmbFieldOperator.Name = "cmbFieldOperator";
            this.cmbFieldOperator.SelectedIndexChanged += new System.EventHandler(this.cmbField1Operator_SelectedIndexChanged);
            // 
            // txtFieldValue
            // 
            resources.ApplyResources(this.txtFieldValue, "txtFieldValue");
            this.txtFieldValue.Name = "txtFieldValue";
            // 
            // lblField1Op
            // 
            resources.ApplyResources(this.lblField1Op, "lblField1Op");
            this.lblField1Op.Name = "lblField1Op";
            // 
            // lblField1Val
            // 
            resources.ApplyResources(this.lblField1Val, "lblField1Val");
            this.lblField1Val.Name = "lblField1Val";
            // 
            // lbxRecordFilters
            // 
            resources.ApplyResources(this.lbxRecordFilters, "lbxRecordFilters");
            this.lbxRecordFilters.FormattingEnabled = true;
            this.lbxRecordFilters.Name = "lbxRecordFilters";
            this.lbxRecordFilters.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbxRecordFilters_KeyUp);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnAdd
            // 
            resources.ApplyResources(this.btnAdd, "btnAdd");
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // cmbFieldValue
            // 
            this.cmbFieldValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFieldValue.FormattingEnabled = true;
            resources.ApplyResources(this.cmbFieldValue, "cmbFieldValue");
            this.cmbFieldValue.Name = "cmbFieldValue";
            // 
            // dtpFieldValue
            // 
            resources.ApplyResources(this.dtpFieldValue, "dtpFieldValue");
            this.dtpFieldValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFieldValue.MaxDate = new System.DateTime(2050, 12, 31, 0, 0, 0, 0);
            this.dtpFieldValue.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dtpFieldValue.Name = "dtpFieldValue";
            // 
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // PackageRecordSelectionDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.dtpFieldValue);
            this.Controls.Add(this.cmbFieldValue);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbxRecordFilters);
            this.Controls.Add(this.lblField1Val);
            this.Controls.Add(this.lblField1Op);
            this.Controls.Add(this.txtFieldValue);
            this.Controls.Add(this.cmbFieldOperator);
            this.Controls.Add(this.lblField1);
            this.Controls.Add(this.cmbFieldName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageRecordSelectionDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbFieldName;
        private System.Windows.Forms.Label lblField1;
        private System.Windows.Forms.ComboBox cmbFieldOperator;
        private System.Windows.Forms.TextBox txtFieldValue;
        private System.Windows.Forms.Label lblField1Op;
        private System.Windows.Forms.Label lblField1Val;
        private System.Windows.Forms.ListBox lbxRecordFilters;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ComboBox cmbFieldValue;
        private System.Windows.Forms.DateTimePicker dtpFieldValue;
        private System.Windows.Forms.Button btnClear;
    }
}