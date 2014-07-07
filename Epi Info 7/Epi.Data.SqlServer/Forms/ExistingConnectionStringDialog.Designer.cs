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
            this.groupBoxAuthentication.TabIndex = 5;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(152, 261);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(233, 261);
            // 
            // cmbServerName
            // 
            this.cmbServerName.TabIndex = 0;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(71, 261);
            // 
            // cmbDatabaseName
            // 
            this.cmbDatabaseName.TabIndex = 1;
            // 
            // ExistingConnectionStringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(317, 296);
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
