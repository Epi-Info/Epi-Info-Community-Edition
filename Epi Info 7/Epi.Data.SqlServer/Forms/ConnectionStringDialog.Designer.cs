namespace Epi.Data.SqlServer.Forms
{
    partial class ConnectionStringDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionStringDialog));
            this.lblInstructions = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.groupBoxAuthentication = new System.Windows.Forms.GroupBox();
            this.rdbSqlAuthentication = new System.Windows.Forms.RadioButton();
            this.rdbWindowsAuthentication = new System.Windows.Forms.RadioButton();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblDatabaseName = new System.Windows.Forms.Label();
            this.cmbServerName = new System.Windows.Forms.ComboBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.cmbDatabaseName = new System.Windows.Forms.ComboBox();
            this.groupBoxAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInstructions
            // 
            resources.ApplyResources(this.lblInstructions, "lblInstructions");
            this.lblInstructions.Name = "lblInstructions";
            // 
            // lblServerName
            // 
            resources.ApplyResources(this.lblServerName, "lblServerName");
            this.lblServerName.Name = "lblServerName";
            // 
            // groupBoxAuthentication
            // 
            this.groupBoxAuthentication.Controls.Add(this.rdbSqlAuthentication);
            this.groupBoxAuthentication.Controls.Add(this.rdbWindowsAuthentication);
            this.groupBoxAuthentication.Controls.Add(this.lblPassword);
            this.groupBoxAuthentication.Controls.Add(this.txtPassword);
            this.groupBoxAuthentication.Controls.Add(this.txtUserName);
            this.groupBoxAuthentication.Controls.Add(this.lblUserName);
            resources.ApplyResources(this.groupBoxAuthentication, "groupBoxAuthentication");
            this.groupBoxAuthentication.Name = "groupBoxAuthentication";
            this.groupBoxAuthentication.TabStop = false;
            // 
            // rdbSqlAuthentication
            // 
            resources.ApplyResources(this.rdbSqlAuthentication, "rdbSqlAuthentication");
            this.rdbSqlAuthentication.Name = "rdbSqlAuthentication";
            this.rdbSqlAuthentication.UseVisualStyleBackColor = true;
            this.rdbSqlAuthentication.CheckedChanged += new System.EventHandler(this.rdbAuthentication_CheckedChanged);
            // 
            // rdbWindowsAuthentication
            // 
            resources.ApplyResources(this.rdbWindowsAuthentication, "rdbWindowsAuthentication");
            this.rdbWindowsAuthentication.Checked = true;
            this.rdbWindowsAuthentication.Name = "rdbWindowsAuthentication";
            this.rdbWindowsAuthentication.TabStop = true;
            this.rdbWindowsAuthentication.UseVisualStyleBackColor = true;
            this.rdbWindowsAuthentication.CheckedChanged += new System.EventHandler(this.rdbAuthentication_CheckedChanged);
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUserName
            // 
            resources.ApplyResources(this.txtUserName, "txtUserName");
            this.txtUserName.Name = "txtUserName";
            // 
            // lblUserName
            // 
            resources.ApplyResources(this.lblUserName, "lblUserName");
            this.lblUserName.Name = "lblUserName";
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
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblDatabaseName
            // 
            resources.ApplyResources(this.lblDatabaseName, "lblDatabaseName");
            this.lblDatabaseName.Name = "lblDatabaseName";
            // 
            // cmbServerName
            // 
            this.cmbServerName.FormattingEnabled = true;
            resources.ApplyResources(this.cmbServerName, "cmbServerName");
            this.cmbServerName.Name = "cmbServerName";
            this.cmbServerName.SelectedIndexChanged += new System.EventHandler(this.cmbServerName_SelectedIndexChanged);
            // 
            // btnTest
            // 
            resources.ApplyResources(this.btnTest, "btnTest");
            this.btnTest.Name = "btnTest";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // cmbDatabaseName
            // 
            this.cmbDatabaseName.FormattingEnabled = true;
            resources.ApplyResources(this.cmbDatabaseName, "cmbDatabaseName");
            this.cmbDatabaseName.Name = "cmbDatabaseName";
            // 
            // ConnectionStringDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cmbDatabaseName);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.cmbServerName);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBoxAuthentication);
            this.Controls.Add(this.lblServerName);
            this.Controls.Add(this.lblInstructions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionStringDialog";
            this.groupBoxAuthentication.ResumeLayout(false);
            this.groupBoxAuthentication.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblInstructions;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblServerName;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.GroupBox groupBoxAuthentication;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblUserName;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblPassword;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.TextBox txtPassword;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.TextBox txtUserName;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnOK;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnCancel;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.RadioButton rdbSqlAuthentication;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.RadioButton rdbWindowsAuthentication;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblDatabaseName;

        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.ComboBox cmbServerName;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnTest;
        
        /// <summary>
        /// Control exposed to derived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.ComboBox cmbDatabaseName;

    }
}