namespace Epi.Windows.ImportExport.Dialogs
{
    partial class PackageAdvancedOptionsDialog
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
            this.lblInitVector = new System.Windows.Forms.Label();
            this.txtInitVector = new System.Windows.Forms.TextBox();
            this.lblMain = new System.Windows.Forms.Label();
            this.txtSalt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudIterations = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpSecurity = new System.Windows.Forms.GroupBox();
            this.checkboxCustomEncryption = new System.Windows.Forms.CheckBox();
            this.checkboxIncludeGrids = new System.Windows.Forms.CheckBox();
            this.checkboxIncludeCodeTables = new System.Windows.Forms.CheckBox();
            this.cmbFormData = new System.Windows.Forms.ComboBox();
            this.lblFormDataToInclude = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterations)).BeginInit();
            this.grpSecurity.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInitVector
            // 
            this.lblInitVector.Location = new System.Drawing.Point(6, 115);
            this.lblInitVector.Name = "lblInitVector";
            this.lblInitVector.Size = new System.Drawing.Size(335, 13);
            this.lblInitVector.TabIndex = 0;
            this.lblInitVector.Text = "Custom Initialization Vector:";
            // 
            // txtInitVector
            // 
            this.txtInitVector.Enabled = false;
            this.txtInitVector.Location = new System.Drawing.Point(6, 131);
            this.txtInitVector.MaxLength = 16;
            this.txtInitVector.Name = "txtInitVector";
            this.txtInitVector.Size = new System.Drawing.Size(335, 20);
            this.txtInitVector.TabIndex = 1;
            // 
            // lblMain
            // 
            this.lblMain.Location = new System.Drawing.Point(6, 53);
            this.lblMain.Name = "lblMain";
            this.lblMain.Size = new System.Drawing.Size(335, 63);
            this.lblMain.TabIndex = 2;
            this.lblMain.Text = "These options can be used to change the encryption parameters for the data packag" +
    "e. These settings should only be modified by experienced users.";
            // 
            // txtSalt
            // 
            this.txtSalt.Enabled = false;
            this.txtSalt.Location = new System.Drawing.Point(6, 170);
            this.txtSalt.MaxLength = 32;
            this.txtSalt.Name = "txtSalt";
            this.txtSalt.Size = new System.Drawing.Size(335, 20);
            this.txtSalt.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(335, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Custom Salt Value:";
            // 
            // nudIterations
            // 
            this.nudIterations.Enabled = false;
            this.nudIterations.Location = new System.Drawing.Point(9, 209);
            this.nudIterations.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudIterations.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudIterations.Name = "nudIterations";
            this.nudIterations.Size = new System.Drawing.Size(79, 20);
            this.nudIterations.TabIndex = 5;
            this.nudIterations.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 193);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(335, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Iterations:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(208, 355);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(290, 355);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpSecurity
            // 
            this.grpSecurity.Controls.Add(this.checkboxCustomEncryption);
            this.grpSecurity.Controls.Add(this.lblInitVector);
            this.grpSecurity.Controls.Add(this.txtInitVector);
            this.grpSecurity.Controls.Add(this.label2);
            this.grpSecurity.Controls.Add(this.lblMain);
            this.grpSecurity.Controls.Add(this.label1);
            this.grpSecurity.Controls.Add(this.nudIterations);
            this.grpSecurity.Controls.Add(this.txtSalt);
            this.grpSecurity.Location = new System.Drawing.Point(12, 114);
            this.grpSecurity.Name = "grpSecurity";
            this.grpSecurity.Size = new System.Drawing.Size(351, 235);
            this.grpSecurity.TabIndex = 9;
            this.grpSecurity.TabStop = false;
            this.grpSecurity.Text = "Encryption";
            // 
            // checkboxCustomEncryption
            // 
            this.checkboxCustomEncryption.AutoSize = true;
            this.checkboxCustomEncryption.Location = new System.Drawing.Point(6, 23);
            this.checkboxCustomEncryption.Name = "checkboxCustomEncryption";
            this.checkboxCustomEncryption.Size = new System.Drawing.Size(148, 17);
            this.checkboxCustomEncryption.TabIndex = 10;
            this.checkboxCustomEncryption.Text = "Enable custom encryption";
            this.checkboxCustomEncryption.UseVisualStyleBackColor = true;
            this.checkboxCustomEncryption.CheckedChanged += new System.EventHandler(this.checkboxCustomEncryption_CheckedChanged);
            // 
            // checkboxIncludeGrids
            // 
            this.checkboxIncludeGrids.AutoSize = true;
            this.checkboxIncludeGrids.Checked = true;
            this.checkboxIncludeGrids.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxIncludeGrids.Location = new System.Drawing.Point(12, 58);
            this.checkboxIncludeGrids.Name = "checkboxIncludeGrids";
            this.checkboxIncludeGrids.Size = new System.Drawing.Size(161, 17);
            this.checkboxIncludeGrids.TabIndex = 10;
            this.checkboxIncludeGrids.Text = "Include grid data in package";
            this.checkboxIncludeGrids.UseVisualStyleBackColor = true;
            // 
            // checkboxIncludeCodeTables
            // 
            this.checkboxIncludeCodeTables.AutoSize = true;
            this.checkboxIncludeCodeTables.Location = new System.Drawing.Point(12, 81);
            this.checkboxIncludeCodeTables.Name = "checkboxIncludeCodeTables";
            this.checkboxIncludeCodeTables.Size = new System.Drawing.Size(175, 17);
            this.checkboxIncludeCodeTables.TabIndex = 11;
            this.checkboxIncludeCodeTables.Text = "Include code tables in package";
            this.checkboxIncludeCodeTables.UseVisualStyleBackColor = true;
            // 
            // cmbFormData
            // 
            this.cmbFormData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormData.FormattingEnabled = true;
            this.cmbFormData.Items.AddRange(new object[] {
            "Selected form and all descendant forms",
            "Selected form and direct descendants",
            "Selected form only (no descendants)"});
            this.cmbFormData.Location = new System.Drawing.Point(12, 29);
            this.cmbFormData.Name = "cmbFormData";
            this.cmbFormData.Size = new System.Drawing.Size(248, 21);
            this.cmbFormData.TabIndex = 12;
            // 
            // lblFormDataToInclude
            // 
            this.lblFormDataToInclude.Location = new System.Drawing.Point(12, 13);
            this.lblFormDataToInclude.Name = "lblFormDataToInclude";
            this.lblFormDataToInclude.Size = new System.Drawing.Size(248, 13);
            this.lblFormDataToInclude.TabIndex = 13;
            this.lblFormDataToInclude.Text = "Form data to include:";
            // 
            // PackageAdvancedOptionsDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(377, 390);
            this.Controls.Add(this.lblFormDataToInclude);
            this.Controls.Add(this.cmbFormData);
            this.Controls.Add(this.checkboxIncludeCodeTables);
            this.Controls.Add(this.checkboxIncludeGrids);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpSecurity);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageAdvancedOptionsDialog";
            this.ShowIcon = false;
            this.Text = "Advanced Options";
            ((System.ComponentModel.ISupportInitialize)(this.nudIterations)).EndInit();
            this.grpSecurity.ResumeLayout(false);
            this.grpSecurity.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInitVector;
        private System.Windows.Forms.TextBox txtInitVector;
        private System.Windows.Forms.Label lblMain;
        private System.Windows.Forms.TextBox txtSalt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudIterations;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpSecurity;
        private System.Windows.Forms.CheckBox checkboxCustomEncryption;
        private System.Windows.Forms.CheckBox checkboxIncludeGrids;
        private System.Windows.Forms.CheckBox checkboxIncludeCodeTables;
        private System.Windows.Forms.ComboBox cmbFormData;
        private System.Windows.Forms.Label lblFormDataToInclude;
    }
}