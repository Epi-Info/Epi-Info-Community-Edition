namespace Epi.Data.SqlServer.Forms
{
    partial class NonExistingConnectionStringDialog
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
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.groupBoxAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = false;
            this.lblInstructions.Location = new System.Drawing.Point(10, 9);
            this.lblInstructions.Size = new System.Drawing.Size(294, 27);
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = false;
            this.lblServerName.Location = new System.Drawing.Point(10, 36);
            this.lblServerName.Size = new System.Drawing.Size(294, 13);
            // 
            // groupBoxAuthentication
            // 
            this.groupBoxAuthentication.Size = new System.Drawing.Size(302, 171);
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = false;
            this.lblUserName.Location = new System.Drawing.Point(28, 69);
            this.lblUserName.Size = new System.Drawing.Size(258, 13);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = false;
            this.lblPassword.Location = new System.Drawing.Point(28, 118);
            this.lblPassword.Size = new System.Drawing.Size(258, 13);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(28, 134);
            this.txtPassword.Size = new System.Drawing.Size(258, 20);
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(28, 85);
            this.txtUserName.Size = new System.Drawing.Size(258, 20);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(122, 299);
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(218, 299);
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.AutoSize = false;
            this.lblDatabaseName.Location = new System.Drawing.Point(10, 78);
            this.lblDatabaseName.Size = new System.Drawing.Size(294, 13);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(26, 299);
            this.btnTest.Size = new System.Drawing.Size(90, 23);
            // 
            // cmbDatabaseName
            // 
            this.cmbDatabaseName.Visible = false;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(10, 96);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(294, 20);
            this.txtDatabaseName.TabIndex = 2;
            this.txtDatabaseName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDatabaseName_KeyPress);
            // 
            // NonExistingConnectionStringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(317, 334);
            this.Controls.Add(this.txtDatabaseName);
            this.Name = "NonExistingConnectionStringDialog";
            this.Load += new System.EventHandler(this.NonExistingConnectionStringDialog_Load);
            this.Controls.SetChildIndex(this.btnTest, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.cmbServerName, 0);
            this.Controls.SetChildIndex(this.cmbDatabaseName, 0);
            this.Controls.SetChildIndex(this.txtDatabaseName, 0);
            this.Controls.SetChildIndex(this.lblInstructions, 0);
            this.Controls.SetChildIndex(this.lblServerName, 0);
            this.Controls.SetChildIndex(this.groupBoxAuthentication, 0);
            this.Controls.SetChildIndex(this.lblDatabaseName, 0);
            this.groupBoxAuthentication.ResumeLayout(false);
            this.groupBoxAuthentication.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDatabaseName;
    }
}
