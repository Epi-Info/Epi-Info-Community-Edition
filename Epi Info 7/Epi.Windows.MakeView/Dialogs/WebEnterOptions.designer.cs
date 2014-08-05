namespace Epi.Windows.MakeView.Dialogs
{
    partial class WebEnterOptions
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlError = new System.Windows.Forms.Panel();
            this.lblError = new System.Windows.Forms.Label();
            this.pnlOrgKey = new System.Windows.Forms.Panel();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BasicRadioButton = new System.Windows.Forms.RadioButton();
            this.WsHTTpRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.YesRadioButton = new System.Windows.Forms.RadioButton();
            this.NoRadioButton = new System.Windows.Forms.RadioButton();
            this.EndPointTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.pnlOrgKey.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.pnlError);
            this.flowLayoutPanel1.Controls.Add(this.pnlOrgKey);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(732, 268);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // pnlError
            // 
            this.pnlError.AutoSize = true;
            this.pnlError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.pnlError.Controls.Add(this.lblError);
            this.pnlError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlError.Location = new System.Drawing.Point(3, 3);
            this.pnlError.MinimumSize = new System.Drawing.Size(622, 35);
            this.pnlError.Name = "pnlError";
            this.pnlError.Padding = new System.Windows.Forms.Padding(9);
            this.pnlError.Size = new System.Drawing.Size(727, 36);
            this.pnlError.TabIndex = 4;
            this.pnlError.Visible = false;
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.lblError.Location = new System.Drawing.Point(12, 12);
            this.lblError.Margin = new System.Windows.Forms.Padding(0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(161, 15);
            this.lblError.TabIndex = 0;
            this.lblError.Text = "[Error Message Description]";
            // 
            // pnlOrgKey
            // 
            this.pnlOrgKey.Controls.Add(this.CancelButton);
            this.pnlOrgKey.Controls.Add(this.OkButton);
            this.pnlOrgKey.Controls.Add(this.groupBox2);
            this.pnlOrgKey.Controls.Add(this.groupBox1);
            this.pnlOrgKey.Controls.Add(this.EndPointTextBox);
            this.pnlOrgKey.Controls.Add(this.label1);
            this.pnlOrgKey.Controls.Add(this.label3);
            this.pnlOrgKey.Controls.Add(this.label2);
            this.pnlOrgKey.Location = new System.Drawing.Point(3, 45);
            this.pnlOrgKey.MinimumSize = new System.Drawing.Size(622, 0);
            this.pnlOrgKey.Name = "pnlOrgKey";
            this.pnlOrgKey.Size = new System.Drawing.Size(727, 216);
            this.pnlOrgKey.TabIndex = 3;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(644, 169);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(80, 32);
            this.CancelButton.TabIndex = 10;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(504, 169);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(121, 32);
            this.OkButton.TabIndex = 9;
            this.OkButton.Text = "Apply Settings";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BasicRadioButton);
            this.groupBox2.Controls.Add(this.WsHTTpRadioButton);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(312, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(153, 58);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Binding Protocol:";
            // 
            // BasicRadioButton
            // 
            this.BasicRadioButton.AutoSize = true;
            this.BasicRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BasicRadioButton.Location = new System.Drawing.Point(9, 28);
            this.BasicRadioButton.Name = "BasicRadioButton";
            this.BasicRadioButton.Size = new System.Drawing.Size(52, 19);
            this.BasicRadioButton.TabIndex = 6;
            this.BasicRadioButton.TabStop = true;
            this.BasicRadioButton.Text = "basic";
            this.BasicRadioButton.UseVisualStyleBackColor = true;
            // 
            // WsHTTpRadioButton
            // 
            this.WsHTTpRadioButton.AutoSize = true;
            this.WsHTTpRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WsHTTpRadioButton.Location = new System.Drawing.Point(67, 28);
            this.WsHTTpRadioButton.Name = "WsHTTpRadioButton";
            this.WsHTTpRadioButton.Size = new System.Drawing.Size(69, 19);
            this.WsHTTpRadioButton.TabIndex = 7;
            this.WsHTTpRadioButton.TabStop = true;
            this.WsHTTpRadioButton.Text = "wsHTTP";
            this.WsHTTpRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.YesRadioButton);
            this.groupBox1.Controls.Add(this.NoRadioButton);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(15, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(291, 58);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connect using Windows Authentication?";
            // 
            // YesRadioButton
            // 
            this.YesRadioButton.AutoSize = true;
            this.YesRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YesRadioButton.Location = new System.Drawing.Point(9, 28);
            this.YesRadioButton.Name = "YesRadioButton";
            this.YesRadioButton.Size = new System.Drawing.Size(174, 19);
            this.YesRadioButton.TabIndex = 6;
            this.YesRadioButton.TabStop = true;
            this.YesRadioButton.Text = "Yes (requires basic protocol)";
            this.YesRadioButton.UseVisualStyleBackColor = true;
            // 
            // NoRadioButton
            // 
            this.NoRadioButton.AutoSize = true;
            this.NoRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoRadioButton.Location = new System.Drawing.Point(233, 28);
            this.NoRadioButton.Name = "NoRadioButton";
            this.NoRadioButton.Size = new System.Drawing.Size(41, 19);
            this.NoRadioButton.TabIndex = 7;
            this.NoRadioButton.TabStop = true;
            this.NoRadioButton.Text = "No";
            this.NoRadioButton.UseVisualStyleBackColor = true;
            // 
            // EndPointTextBox
            // 
            this.EndPointTextBox.Location = new System.Drawing.Point(15, 86);
            this.EndPointTextBox.Multiline = true;
            this.EndPointTextBox.Name = "EndPointTextBox";
            this.EndPointTextBox.Size = new System.Drawing.Size(709, 51);
            this.EndPointTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(518, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "You can also update this information by clicking the ‘Web Survey’ tab under the T" +
    "ools > Options.";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Endpoint Address:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(653, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Please contact your system administrator to provide you with the appropriate sett" +
    "ings to publish surveys to the web.\u2028";
            // 
            // WebEnterOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(732, 268);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "WebEnterOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Web Enter Options";
            this.Load += new System.EventHandler(this.WebSurveyOptions_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.pnlError.PerformLayout();
            this.pnlOrgKey.ResumeLayout(false);
            this.pnlOrgKey.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel pnlError;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Panel pnlOrgKey;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton BasicRadioButton;
        private System.Windows.Forms.RadioButton WsHTTpRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton YesRadioButton;
        private System.Windows.Forms.RadioButton NoRadioButton;
        private System.Windows.Forms.TextBox EndPointTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
    }
}