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
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(152, 261);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(233, 261);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(71, 261);
            // 
            // cmbDatabaseName
            // 
            this.cmbDatabaseName.Visible = false;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(10, 96);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(293, 20);
            this.txtDatabaseName.TabIndex = 2;
            this.txtDatabaseName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDatabaseName_KeyPress);
            // 
            // NonExistingConnectionStringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(317, 296);
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
