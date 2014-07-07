namespace EpiDashboard.Dialogs
{
    partial class ConditionalAssignDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConditionalAssignDialog));
            this.cbxFieldType = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.txtDestinationField = new System.Windows.Forms.TextBox();
            this.lblDestination = new System.Windows.Forms.Label();
            this.txtAssignCondition = new System.Windows.Forms.TextBox();
            this.btnIfCondition = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAssignValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtElseValue = new System.Windows.Forms.TextBox();
            this.checkboxUseElse = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cmbAssignValue = new System.Windows.Forms.ComboBox();
            this.cmbElseValue = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbxFieldType
            // 
            this.cbxFieldType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFieldType.FormattingEnabled = true;
            resources.ApplyResources(this.cbxFieldType, "cbxFieldType");
            this.cbxFieldType.Name = "cbxFieldType";
            this.cbxFieldType.SelectedIndexChanged += new System.EventHandler(this.cbxFieldType_SelectedIndexChanged);
            // 
            // lblType
            // 
            resources.ApplyResources(this.lblType, "lblType");
            this.lblType.Name = "lblType";
            // 
            // txtDestinationField
            // 
            resources.ApplyResources(this.txtDestinationField, "txtDestinationField");
            this.txtDestinationField.Name = "txtDestinationField";
            // 
            // lblDestination
            // 
            resources.ApplyResources(this.lblDestination, "lblDestination");
            this.lblDestination.Name = "lblDestination";
            // 
            // txtAssignCondition
            // 
            resources.ApplyResources(this.txtAssignCondition, "txtAssignCondition");
            this.txtAssignCondition.Name = "txtAssignCondition";
            this.txtAssignCondition.ReadOnly = true;
            // 
            // btnIfCondition
            // 
            resources.ApplyResources(this.btnIfCondition, "btnIfCondition");
            this.btnIfCondition.Name = "btnIfCondition";
            this.btnIfCondition.UseVisualStyleBackColor = true;
            this.btnIfCondition.Click += new System.EventHandler(this.btnIfCondition_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtAssignValue
            // 
            resources.ApplyResources(this.txtAssignValue, "txtAssignValue");
            this.txtAssignValue.Name = "txtAssignValue";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // txtElseValue
            // 
            resources.ApplyResources(this.txtElseValue, "txtElseValue");
            this.txtElseValue.Name = "txtElseValue";
            // 
            // checkboxUseElse
            // 
            resources.ApplyResources(this.checkboxUseElse, "checkboxUseElse");
            this.checkboxUseElse.Name = "checkboxUseElse";
            this.checkboxUseElse.UseVisualStyleBackColor = true;
            this.checkboxUseElse.CheckedChanged += new System.EventHandler(this.checkboxUseElse_CheckedChanged);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cmbAssignValue
            // 
            this.cmbAssignValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAssignValue.FormattingEnabled = true;
            resources.ApplyResources(this.cmbAssignValue, "cmbAssignValue");
            this.cmbAssignValue.Name = "cmbAssignValue";
            // 
            // cmbElseValue
            // 
            this.cmbElseValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbElseValue, "cmbElseValue");
            this.cmbElseValue.FormattingEnabled = true;
            this.cmbElseValue.Name = "cmbElseValue";
            // 
            // ConditionalAssignDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cmbElseValue);
            this.Controls.Add(this.cmbAssignValue);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.checkboxUseElse);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtElseValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAssignValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnIfCondition);
            this.Controls.Add(this.txtAssignCondition);
            this.Controls.Add(this.cbxFieldType);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.txtDestinationField);
            this.Controls.Add(this.lblDestination);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConditionalAssignDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxFieldType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TextBox txtDestinationField;
        private System.Windows.Forms.Label lblDestination;
        private System.Windows.Forms.TextBox txtAssignCondition;
        private System.Windows.Forms.Button btnIfCondition;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAssignValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtElseValue;
        private System.Windows.Forms.CheckBox checkboxUseElse;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ComboBox cmbAssignValue;
        private System.Windows.Forms.ComboBox cmbElseValue;
    }
}