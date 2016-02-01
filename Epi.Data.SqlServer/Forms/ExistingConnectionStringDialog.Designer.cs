namespace Epi.Data.SqlServer.Forms
{
    partial class ExistingConnectionStringDialog
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
            this.groupBoxAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInstructions
            // 
            this.lblInstructions.TabIndex = 4;
            // 
            // lblServerName
            // 
            this.lblServerName.TabIndex = 2;
            // 
            // groupBoxAuthentication
            // 
            this.groupBoxAuthentication.Size = new System.Drawing.Size(302, 162);
            this.groupBoxAuthentication.TabIndex = 5;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = false;
            this.lblUserName.Location = new System.Drawing.Point(33, 69);
            this.lblUserName.Size = new System.Drawing.Size(263, 13);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = false;
            this.lblPassword.Location = new System.Drawing.Point(33, 108);
            this.lblPassword.Size = new System.Drawing.Size(263, 13);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(33, 124);
            this.txtPassword.Size = new System.Drawing.Size(263, 20);
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(33, 85);
            this.txtUserName.Size = new System.Drawing.Size(263, 20);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(122, 290);
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(218, 290);
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            // 
            // cmbServerName
            // 
            this.cmbServerName.TabIndex = 0;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(26, 290);
            this.btnTest.Size = new System.Drawing.Size(90, 23);
            // 
            // cmbDatabaseName
            // 
            this.cmbDatabaseName.TabIndex = 1;
            // 
            // ExistingConnectionStringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(317, 325);
            this.Name = "ExistingConnectionStringDialog";
            this.Load += new System.EventHandler(this.ExistingConnectionStringDialog_Load);
            this.groupBoxAuthentication.ResumeLayout(false);
            this.groupBoxAuthentication.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
