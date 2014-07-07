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
            this.pnlOrgKey.Location = new System.Drawing.Point(3, 42);
            this.pnlOrgKey.MinimumSize = new System.Drawing.Size(533, 0);
            this.pnlOrgKey.Name = "pnlOrgKey";
            this.pnlOrgKey.Size = new System.Drawing.Size(533, 80);
            this.pnlOrgKey.TabIndex = 3;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Enabled = false;
            this.btnSubmit.Location = new System.Drawing.Point(438, 31);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(76, 31);
            this.btnSubmit.TabIndex = 3;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbOrgKey
            // 
            this.tbOrgKey.Location = new System.Drawing.Point(124, 35);
            this.tbOrgKey.Name = "tbOrgKey";
            this.tbOrgKey.Size = new System.Drawing.Size(308, 23);
            this.tbOrgKey.TabIndex = 2;
            this.tbOrgKey.TextChanged += new System.EventHandler(this.tbOrgKey_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Organization Key:";
            // 
            // DialogPromptLabel
            // 
            this.DialogPromptLabel.AutoSize = true;
            this.DialogPromptLabel.Location = new System.Drawing.Point(10, 10);
            this.DialogPromptLabel.Name = "DialogPromptLabel";
            this.DialogPromptLabel.Size = new System.Drawing.Size(504, 15);
            this.DialogPromptLabel.TabIndex = 0;
            this.DialogPromptLabel.Text = "The organization key is required for security purposes before you publish the for" +
    "m to the web. ";
            // 
            // pnlError
            // 
            this.pnlError.AutoSize = true;
            this.pnlError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.pnlError.Controls.Add(this.lblError);
            this.pnlError.Location = new System.Drawing.Point(3, 3);
            this.pnlError.MinimumSize = new System.Drawing.Size(533, 30);
            this.pnlError.Name = "pnlError";
            this.pnlError.Padding = new System.Windows.Forms.Padding(8);
            this.pnlError.Size = new System.Drawing.Size(533, 33);
            this.pnlError.TabIndex = 4;
            this.pnlError.Visible = false;
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.lblError.Location = new System.Drawing.Point(10, 10);
            this.lblError.Margin = new System.Windows.Forms.Padding(0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(318, 15);
            this.lblError.TabIndex = 0;
            this.lblError.Text = "Organization Key is not valid. Please enter a correct key.";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.pnlError);
            this.flowLayoutPanel1.Controls.Add(this.pnlOrgKey);
            this.flowLayoutPanel1.Controls.Add(this.pnlSuccess);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(545, 212);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // pnlSuccess
            // 
            this.pnlSuccess.AutoSize = true;
            this.pnlSuccess.Controls.Add(this.btnClose);
            this.pnlSuccess.Controls.Add(this.pnlSuccessMsg);
            this.pnlSuccess.Location = new System.Drawing.Point(3, 128);
            this.pnlSuccess.Name = "pnlSuccess";
            this.pnlSuccess.Size = new System.Drawing.Size(539, 81);
            this.pnlSuccess.TabIndex = 5;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(227, 47);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(76, 31);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlSuccessMsg
            // 
            this.pnlSuccessMsg.AutoSize = true;
            this.pnlSuccessMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))));
            this.pnlSuccessMsg.Controls.Add(this.lblSuccess);
            this.pnlSuccessMsg.Location = new System.Drawing.Point(3, 2);
            this.pnlSuccessMsg.MinimumSize = new System.Drawing.Size(533, 30);
            this.pnlSuccessMsg.Name = "pnlSuccessMsg";
            this.pnlSuccessMsg.Padding = new System.Windows.Forms.Padding(8);
            this.pnlSuccessMsg.Size = new System.Drawing.Size(533, 33);
            this.pnlSuccessMsg.TabIndex = 5;
            this.pnlSuccessMsg.Visible = false;
            // 
            // lblSuccess
            // 
            this.lblSuccess.AutoSize = true;
            this.lblSuccess.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSuccess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(0)))));
            this.lblSuccess.Location = new System.Drawing.Point(10, 10);
            this.lblSuccess.Margin = new System.Windows.Forms.Padding(0);
            this.lblSuccess.Name = "lblSuccess";
            this.lblSuccess.Size = new System.Drawing.Size(109, 15);
            this.lblSuccess.TabIndex = 0;
            this.lblSuccess.Text = "[Success Message]";
            // 
            // OrgKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(553, 218);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrgKey";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Organization Key";
            this.pnlOrgKey.ResumeLayout(false);
            this.pnlOrgKey.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.pnlError.PerformLayout();
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