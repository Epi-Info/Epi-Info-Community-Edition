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
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(213, 272);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 25;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(375, 272);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 26;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(441, 41);
            this.label1.TabIndex = 27;
            this.label1.Text = "Use this dialog to select specific records to be included in the data package. If" +
    " left blank, all records in the selected form will be packaged.";
            // 
            // cmbFieldName
            // 
            this.cmbFieldName.FormattingEnabled = true;
            this.cmbFieldName.Location = new System.Drawing.Point(12, 78);
            this.cmbFieldName.Name = "cmbFieldName";
            this.cmbFieldName.Size = new System.Drawing.Size(121, 21);
            this.cmbFieldName.TabIndex = 28;
            this.cmbFieldName.SelectedIndexChanged += new System.EventHandler(this.cmbField1Name_SelectedIndexChanged);
            // 
            // lblField1
            // 
            this.lblField1.Location = new System.Drawing.Point(13, 59);
            this.lblField1.Name = "lblField1";
            this.lblField1.Size = new System.Drawing.Size(120, 16);
            this.lblField1.TabIndex = 29;
            this.lblField1.Text = "Field:";
            // 
            // cmbFieldOperator
            // 
            this.cmbFieldOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFieldOperator.FormattingEnabled = true;
            this.cmbFieldOperator.Location = new System.Drawing.Point(139, 78);
            this.cmbFieldOperator.Name = "cmbFieldOperator";
            this.cmbFieldOperator.Size = new System.Drawing.Size(160, 21);
            this.cmbFieldOperator.TabIndex = 30;
            this.cmbFieldOperator.SelectedIndexChanged += new System.EventHandler(this.cmbField1Operator_SelectedIndexChanged);
            // 
            // txtFieldValue
            // 
            this.txtFieldValue.Location = new System.Drawing.Point(305, 78);
            this.txtFieldValue.Name = "txtFieldValue";
            this.txtFieldValue.Size = new System.Drawing.Size(145, 20);
            this.txtFieldValue.TabIndex = 31;
            // 
            // lblField1Op
            // 
            this.lblField1Op.Location = new System.Drawing.Point(139, 59);
            this.lblField1Op.Name = "lblField1Op";
            this.lblField1Op.Size = new System.Drawing.Size(120, 16);
            this.lblField1Op.TabIndex = 32;
            this.lblField1Op.Text = "Operator:";
            // 
            // lblField1Val
            // 
            this.lblField1Val.Location = new System.Drawing.Point(302, 59);
            this.lblField1Val.Name = "lblField1Val";
            this.lblField1Val.Size = new System.Drawing.Size(120, 16);
            this.lblField1Val.TabIndex = 33;
            this.lblField1Val.Text = "Value:";
            // 
            // lbxRecordFilters
            // 
            this.lbxRecordFilters.FormattingEnabled = true;
            this.lbxRecordFilters.Location = new System.Drawing.Point(12, 165);
            this.lbxRecordFilters.Name = "lbxRecordFilters";
            this.lbxRecordFilters.Size = new System.Drawing.Size(437, 95);
            this.lbxRecordFilters.TabIndex = 34;
            this.lbxRecordFilters.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbxRecordFilters_KeyUp);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(436, 16);
            this.label2.TabIndex = 35;
            this.label2.Text = "Records that meet all the following conditions will be included in the package:";
            // 
            // btnAdd
            // 
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(12, 114);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(143, 23);
            this.btnAdd.TabIndex = 36;
            this.btnAdd.Text = "Add Record Filter";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // cmbFieldValue
            // 
            this.cmbFieldValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFieldValue.FormattingEnabled = true;
            this.cmbFieldValue.Location = new System.Drawing.Point(306, 78);
            this.cmbFieldValue.Name = "cmbFieldValue";
            this.cmbFieldValue.Size = new System.Drawing.Size(144, 21);
            this.cmbFieldValue.TabIndex = 37;
            this.cmbFieldValue.Visible = false;
            // 
            // dtpFieldValue
            // 
            this.dtpFieldValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFieldValue.Location = new System.Drawing.Point(306, 78);
            this.dtpFieldValue.MaxDate = new System.DateTime(2050, 12, 31, 0, 0, 0, 0);
            this.dtpFieldValue.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dtpFieldValue.Name = "dtpFieldValue";
            this.dtpFieldValue.Size = new System.Drawing.Size(145, 20);
            this.dtpFieldValue.TabIndex = 38;
            this.dtpFieldValue.Visible = false;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(294, 272);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 39;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // PackageRecordSelectionDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(462, 307);
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
            this.Text = "Select Records for Data Package";
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