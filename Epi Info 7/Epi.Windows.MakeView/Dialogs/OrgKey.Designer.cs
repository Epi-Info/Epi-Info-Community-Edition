namespace Epi.Windows.MakeView.Dialogs
{
    partial class OrgKey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrgKey));
            this.pnlOrgKey = new System.Windows.Forms.Panel();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.tbOrgKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.DialogPromptLabel = new System.Windows.Forms.Label();
            this.pnlError = new System.Windows.Forms.Panel();
            this.lblError = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlSuccess = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlSuccessMsg = new System.Windows.Forms.Panel();
            this.lblSuccess = new System.Windows.Forms.Label();
            this.pnlOrgKey.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlSuccess.SuspendLayout();
            this.pnlSuccessMsg.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlOrgKey
            // 
            this.pnlOrgKey.Controls.Add(this.btnSubmit);
            this.pnlOrgKey.Controls.Add(this.tbOrgKey);
            this.pnlOrgKey.Controls.Add(this.label3);
            this.pnlOrgKey.Controls.Add(this.DialogPromptLabel);
            resources.ApplyResources(this.pnlOrgKey, "pnlOrgKey");
            this.pnlOrgKey.Name = "pnlOrgKey";
            // 
            // btnSubmit
            // 
            resources.ApplyResources(this.btnSubmit, "btnSubmit");
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbOrgKey
            // 
            resources.ApplyResources(this.tbOrgKey, "tbOrgKey");
            this.tbOrgKey.Name = "tbOrgKey";
            this.tbOrgKey.TextChanged += new System.EventHandler(this.tbOrgKey_TextChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // DialogPromptLabel
            // 
            resources.ApplyResources(this.DialogPromptLabel, "DialogPromptLabel");
            this.DialogPromptLabel.Name = "DialogPromptLabel";
            // 
            // pnlError
            // 
            resources.ApplyResources(this.pnlError, "pnlError");
            this.pnlError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.pnlError.Controls.Add(this.lblError);
            this.pnlError.Name = "pnlError";
            // 
            // lblError
            // 
            resources.ApplyResources(this.lblError, "lblError");
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.lblError.Name = "lblError";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.pnlError);
            this.flowLayoutPanel1.Controls.Add(this.pnlOrgKey);
            this.flowLayoutPanel1.Controls.Add(this.pnlSuccess);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // pnlSuccess
            // 
            resources.ApplyResources(this.pnlSuccess, "pnlSuccess");
            this.pnlSuccess.Controls.Add(this.btnClose);
            this.pnlSuccess.Controls.Add(this.pnlSuccessMsg);
            this.pnlSuccess.Name = "pnlSuccess";
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlSuccessMsg
            // 
            resources.ApplyResources(this.pnlSuccessMsg, "pnlSuccessMsg");
            this.pnlSuccessMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))));
            this.pnlSuccessMsg.Controls.Add(this.lblSuccess);
            this.pnlSuccessMsg.Name = "pnlSuccessMsg";
            // 
            // lblSuccess
            // 
            resources.ApplyResources(this.lblSuccess, "lblSuccess");
            this.lblSuccess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(0)))));
            this.lblSuccess.Name = "lblSuccess";
            // 
            // OrgKey
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrgKey";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.pnlOrgKey.ResumeLayout(false);
            this.pnlOrgKey.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pnlSuccess.ResumeLayout(false);
            this.pnlSuccess.PerformLayout();
            this.pnlSuccessMsg.ResumeLayout(false);
            this.pnlSuccessMsg.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlOrgKey;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.TextBox tbOrgKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label DialogPromptLabel;
        private System.Windows.Forms.Panel pnlError;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel pnlSuccess;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlSuccessMsg;
        private System.Windows.Forms.Label lblSuccess;


    }
}