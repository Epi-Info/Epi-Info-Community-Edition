namespace Epi.Windows.MakeView.Dialogs
{
    partial class GetSurveyLink
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
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnDataKeyCopy = new System.Windows.Forms.Button();
            this.btnKeyCopy = new System.Windows.Forms.Button();
            this.lblSecurityToken = new System.Windows.Forms.Label();
            this.lblSurveyKey = new System.Windows.Forms.Label();
            this.txtDataKey = new System.Windows.Forms.TextBox();
            this.txtSurveyKey = new System.Windows.Forms.TextBox();
            this.btnCopyAllURLs = new System.Windows.Forms.Button();
            this.btnURLCopy = new System.Windows.Forms.Button();
            this.btnGo = new System.Windows.Forms.Button();
            this.lblURL = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(12, 59);
            this.txtURL.Name = "txtURL";
            this.txtURL.ReadOnly = true;
            this.txtURL.Size = new System.Drawing.Size(667, 20);
            this.txtURL.TabIndex = 37;
            // 
            // btnDataKeyCopy
            // 
            this.btnDataKeyCopy.Enabled = false;
            this.btnDataKeyCopy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDataKeyCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnDataKeyCopy.Location = new System.Drawing.Point(686, 143);
            this.btnDataKeyCopy.Name = "btnDataKeyCopy";
            this.btnDataKeyCopy.Size = new System.Drawing.Size(58, 23);
            this.btnDataKeyCopy.TabIndex = 47;
            this.btnDataKeyCopy.Text = "Copy";
            this.btnDataKeyCopy.UseVisualStyleBackColor = true;
            // 
            // btnKeyCopy
            // 
            this.btnKeyCopy.Enabled = false;
            this.btnKeyCopy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnKeyCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnKeyCopy.Location = new System.Drawing.Point(686, 99);
            this.btnKeyCopy.Name = "btnKeyCopy";
            this.btnKeyCopy.Size = new System.Drawing.Size(58, 23);
            this.btnKeyCopy.TabIndex = 46;
            this.btnKeyCopy.Text = "Copy";
            this.btnKeyCopy.UseVisualStyleBackColor = true;
            // 
            // lblSecurityToken
            // 
            this.lblSecurityToken.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSecurityToken.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblSecurityToken.Location = new System.Drawing.Point(195, 126);
            this.lblSecurityToken.Name = "lblSecurityToken";
            this.lblSecurityToken.Size = new System.Drawing.Size(525, 16);
            this.lblSecurityToken.TabIndex = 44;
            this.lblSecurityToken.Text = "-- Be sure to copy and save this Security Token in order to download the data peo" +
    "ple submit. ";
            // 
            // lblSurveyKey
            // 
            this.lblSurveyKey.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSurveyKey.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblSurveyKey.Location = new System.Drawing.Point(174, 82);
            this.lblSurveyKey.Name = "lblSurveyKey";
            this.lblSurveyKey.Size = new System.Drawing.Size(552, 16);
            this.lblSurveyKey.TabIndex = 45;
            this.lblSurveyKey.Text = "-- Copy and save this Survey Key in order to access the survey after the people s" +
    "ubmit their responses.";
            this.lblSurveyKey.Click += new System.EventHandler(this.lblSurveyKey_Click);
            // 
            // txtDataKey
            // 
            this.txtDataKey.Location = new System.Drawing.Point(12, 145);
            this.txtDataKey.Name = "txtDataKey";
            this.txtDataKey.ReadOnly = true;
            this.txtDataKey.Size = new System.Drawing.Size(667, 20);
            this.txtDataKey.TabIndex = 42;
            // 
            // txtSurveyKey
            // 
            this.txtSurveyKey.Location = new System.Drawing.Point(12, 101);
            this.txtSurveyKey.Name = "txtSurveyKey";
            this.txtSurveyKey.ReadOnly = true;
            this.txtSurveyKey.Size = new System.Drawing.Size(667, 20);
            this.txtSurveyKey.TabIndex = 43;
            // 
            // btnCopyAllURLs
            // 
            this.btnCopyAllURLs.Enabled = false;
            this.btnCopyAllURLs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCopyAllURLs.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCopyAllURLs.Location = new System.Drawing.Point(755, 98);
            this.btnCopyAllURLs.Name = "btnCopyAllURLs";
            this.btnCopyAllURLs.Size = new System.Drawing.Size(80, 68);
            this.btnCopyAllURLs.TabIndex = 40;
            this.btnCopyAllURLs.Text = "Copy All to Clipboard";
            this.btnCopyAllURLs.UseVisualStyleBackColor = true;
            // 
            // btnURLCopy
            // 
            this.btnURLCopy.Enabled = false;
            this.btnURLCopy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnURLCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnURLCopy.Location = new System.Drawing.Point(686, 56);
            this.btnURLCopy.Name = "btnURLCopy";
            this.btnURLCopy.Size = new System.Drawing.Size(58, 23);
            this.btnURLCopy.TabIndex = 41;
            this.btnURLCopy.Text = "Copy";
            this.btnURLCopy.UseVisualStyleBackColor = true;
            // 
            // btnGo
            // 
            this.btnGo.Enabled = false;
            this.btnGo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnGo.Location = new System.Drawing.Point(754, 56);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(58, 23);
            this.btnGo.TabIndex = 39;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            // 
            // lblURL
            // 
            this.lblURL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblURL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblURL.Location = new System.Drawing.Point(103, 40);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(561, 16);
            this.lblURL.TabIndex = 38;
            this.lblURL.Text = "Send this link to people you want to complete the survey.)";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(103, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(140, 23);
            this.label5.TabIndex = 49;
            this.label5.Text = "DRAFT";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(12, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 23);
            this.label4.TabIndex = 48;
            this.label4.Text = "Publish Mode:";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 16);
            this.label1.TabIndex = 50;
            this.label1.Text = "1. Survey Link:  ";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(12, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 16);
            this.label2.TabIndex = 51;
            this.label2.Text = "2. Survey Key:  ";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(12, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 16);
            this.label3.TabIndex = 52;
            this.label3.Text = "3. Security Token:  ";
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Maroon;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(99, 82);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 16);
            this.label7.TabIndex = 53;
            this.label7.Text = "IMPORTANT";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Maroon;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(119, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 16);
            this.label6.TabIndex = 54;
            this.label6.Text = "IMPORTANT";
            // 
            // GetSurveyLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 197);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.btnDataKeyCopy);
            this.Controls.Add(this.btnKeyCopy);
            this.Controls.Add(this.lblSecurityToken);
            this.Controls.Add(this.lblSurveyKey);
            this.Controls.Add(this.txtDataKey);
            this.Controls.Add(this.txtSurveyKey);
            this.Controls.Add(this.btnCopyAllURLs);
            this.Controls.Add(this.btnURLCopy);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.lblURL);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetSurveyLink";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Get Survey Link and Keys";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnDataKeyCopy;
        private System.Windows.Forms.Button btnKeyCopy;
        private System.Windows.Forms.Label lblSecurityToken;
        private System.Windows.Forms.Label lblSurveyKey;
        private System.Windows.Forms.TextBox txtDataKey;
        private System.Windows.Forms.TextBox txtSurveyKey;
        private System.Windows.Forms.Button btnCopyAllURLs;
        private System.Windows.Forms.Button btnURLCopy;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
    }
}