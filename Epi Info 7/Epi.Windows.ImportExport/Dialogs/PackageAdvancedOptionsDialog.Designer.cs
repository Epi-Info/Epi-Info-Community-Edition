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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageAdvancedOptionsDialog));
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
            resources.ApplyResources(this.lblInitVector, "lblInitVector");
            this.lblInitVector.Name = "lblInitVector";
            // 
            // txtInitVector
            // 
            resources.ApplyResources(this.txtInitVector, "txtInitVector");
            this.txtInitVector.Name = "txtInitVector";
            // 
            // lblMain
            // 
            resources.ApplyResources(this.lblMain, "lblMain");
            this.lblMain.Name = "lblMain";
            // 
            // txtSalt
            // 
            resources.ApplyResources(this.txtSalt, "txtSalt");
            this.txtSalt.Name = "txtSalt";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // nudIterations
            // 
            resources.ApplyResources(this.nudIterations, "nudIterations");
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
            this.nudIterations.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
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
            resources.ApplyResources(this.grpSecurity, "grpSecurity");
            this.grpSecurity.Name = "grpSecurity";
            this.grpSecurity.TabStop = false;
            // 
            // checkboxCustomEncryption
            // 
            resources.ApplyResources(this.checkboxCustomEncryption, "checkboxCustomEncryption");
            this.checkboxCustomEncryption.Name = "checkboxCustomEncryption";
            this.checkboxCustomEncryption.UseVisualStyleBackColor = true;
            this.checkboxCustomEncryption.CheckedChanged += new System.EventHandler(this.checkboxCustomEncryption_CheckedChanged);
            // 
            // checkboxIncludeGrids
            // 
            resources.ApplyResources(this.checkboxIncludeGrids, "checkboxIncludeGrids");
            this.checkboxIncludeGrids.Checked = true;
            this.checkboxIncludeGrids.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxIncludeGrids.Name = "checkboxIncludeGrids";
            this.checkboxIncludeGrids.UseVisualStyleBackColor = true;
            // 
            // checkboxIncludeCodeTables
            // 
            resources.ApplyResources(this.checkboxIncludeCodeTables, "checkboxIncludeCodeTables");
            this.checkboxIncludeCodeTables.Name = "checkboxIncludeCodeTables";
            this.checkboxIncludeCodeTables.UseVisualStyleBackColor = true;
            // 
            // cmbFormData
            // 
            this.cmbFormData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormData.FormattingEnabled = true;
            this.cmbFormData.Items.AddRange(new object[] {
            resources.GetString("cmbFormData.Items"),
            resources.GetString("cmbFormData.Items1"),
            resources.GetString("cmbFormData.Items2")});
            resources.ApplyResources(this.cmbFormData, "cmbFormData");
            this.cmbFormData.Name = "cmbFormData";
            // 
            // lblFormDataToInclude
            // 
            resources.ApplyResources(this.lblFormDataToInclude, "lblFormDataToInclude");
            this.lblFormDataToInclude.Name = "lblFormDataToInclude";
            // 
            // PackageAdvancedOptionsDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
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