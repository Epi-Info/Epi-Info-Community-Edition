namespace Epi.Windows.MakeView.Dialogs
{
    partial class PublishMode
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
            this.UpDateButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.RemoveDataRadioButton = new System.Windows.Forms.RadioButton();
            this.KeepDataRadioButton = new System.Windows.Forms.RadioButton();
            this.FinalRadioButton = new System.Windows.Forms.RadioButton();
            this.DraftRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.OrgKeyLabel = new System.Windows.Forms.Label();
            this.SecurityKey = new System.Windows.Forms.Label();
            this.SecurityKeytextBox = new System.Windows.Forms.TextBox();
            this.OrgKeytextBox = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // UpDateButton
            // 
            this.UpDateButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpDateButton.Location = new System.Drawing.Point(380, 347);
            this.UpDateButton.Name = "UpDateButton";
            this.UpDateButton.Size = new System.Drawing.Size(177, 35);
            this.UpDateButton.TabIndex = 2;
            this.UpDateButton.Text = "Update";
            this.UpDateButton.UseVisualStyleBackColor = true;
            this.UpDateButton.Click += new System.EventHandler(this.UpDateButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelButton.Location = new System.Drawing.Point(585, 347);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(97, 35);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Visible = false;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(110, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(420, 20);
            this.label3.TabIndex = 42;
            this.label3.Text = "(Select to preview and test how your surveys works.)";
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(110, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(420, 20);
            this.label4.TabIndex = 43;
            this.label4.Text = "(Select when you are ready to send the survey out to participants.)";
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(244, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(387, 20);
            this.label5.TabIndex = 44;
            this.label5.Text = "(Select to KEEP any survey responses collected in previous iteration.)";
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(244, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(387, 20);
            this.label6.TabIndex = 45;
            this.label6.Text = "(Select to Remove any survey responses collected in previous iteration.)";
            // 
            // RemoveDataRadioButton
            // 
            this.RemoveDataRadioButton.AutoSize = true;
            this.RemoveDataRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RemoveDataRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveDataRadioButton.Location = new System.Drawing.Point(45, 54);
            this.RemoveDataRadioButton.Name = "RemoveDataRadioButton";
            this.RemoveDataRadioButton.Size = new System.Drawing.Size(193, 19);
            this.RemoveDataRadioButton.TabIndex = 41;
            this.RemoveDataRadioButton.TabStop = true;
            this.RemoveDataRadioButton.Text = "Remove collected survey data";
            this.RemoveDataRadioButton.UseVisualStyleBackColor = true;
            // 
            // KeepDataRadioButton
            // 
            this.KeepDataRadioButton.AutoSize = true;
            this.KeepDataRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeepDataRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.KeepDataRadioButton.Location = new System.Drawing.Point(45, 31);
            this.KeepDataRadioButton.Name = "KeepDataRadioButton";
            this.KeepDataRadioButton.Size = new System.Drawing.Size(175, 19);
            this.KeepDataRadioButton.TabIndex = 40;
            this.KeepDataRadioButton.TabStop = true;
            this.KeepDataRadioButton.Text = "Keep collected survey data";
            this.KeepDataRadioButton.UseVisualStyleBackColor = true;
            // 
            // FinalRadioButton
            // 
            this.FinalRadioButton.AutoSize = true;
            this.FinalRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FinalRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.FinalRadioButton.Location = new System.Drawing.Point(41, 52);
            this.FinalRadioButton.Name = "FinalRadioButton";
            this.FinalRadioButton.Size = new System.Drawing.Size(58, 19);
            this.FinalRadioButton.TabIndex = 39;
            this.FinalRadioButton.TabStop = true;
            this.FinalRadioButton.Text = "FINAL";
            this.FinalRadioButton.UseVisualStyleBackColor = true;
            // 
            // DraftRadioButton
            // 
            this.DraftRadioButton.AutoSize = true;
            this.DraftRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DraftRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DraftRadioButton.Location = new System.Drawing.Point(41, 29);
            this.DraftRadioButton.Name = "DraftRadioButton";
            this.DraftRadioButton.Size = new System.Drawing.Size(63, 19);
            this.DraftRadioButton.TabIndex = 38;
            this.DraftRadioButton.TabStop = true;
            this.DraftRadioButton.Text = "DRAFT";
            this.DraftRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.DraftRadioButton);
            this.groupBox1.Controls.Add(this.FinalRadioButton);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(51, 139);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(631, 91);
            this.groupBox1.TabIndex = 46;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Publish Mode";
            this.groupBox1.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.KeepDataRadioButton);
            this.groupBox2.Controls.Add(this.RemoveDataRadioButton);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(47, 240);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(635, 92);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Survey Data";
            this.groupBox2.Visible = false;
            // 
            // OrgKeyLabel
            // 
            this.OrgKeyLabel.AutoSize = true;
            this.OrgKeyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.OrgKeyLabel.Location = new System.Drawing.Point(48, 49);
            this.OrgKeyLabel.Name = "OrgKeyLabel";
            this.OrgKeyLabel.Size = new System.Drawing.Size(105, 15);
            this.OrgKeyLabel.TabIndex = 48;
            this.OrgKeyLabel.Text = "Organization Key:";
            // 
            // SecurityKey
            // 
            this.SecurityKey.AutoSize = true;
            this.SecurityKey.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.SecurityKey.Location = new System.Drawing.Point(51, 107);
            this.SecurityKey.Name = "SecurityKey";
            this.SecurityKey.Size = new System.Drawing.Size(79, 15);
            this.SecurityKey.TabIndex = 51;
            this.SecurityKey.Text = "Security key:";
            this.SecurityKey.Visible = false;
            // 
            // SecurityKeytextBox
            // 
            this.SecurityKeytextBox.Location = new System.Drawing.Point(163, 113);
            this.SecurityKeytextBox.Name = "SecurityKeytextBox";
            this.SecurityKeytextBox.Size = new System.Drawing.Size(316, 20);
            this.SecurityKeytextBox.TabIndex = 52;
            this.SecurityKeytextBox.Visible = false;
            this.SecurityKeytextBox.TextChanged += new System.EventHandler(this.SecurityKeytextBox_TextChanged);
            // 
            // OrgKeytextBox
            // 
            this.OrgKeytextBox.AutoSize = true;
            this.OrgKeytextBox.Location = new System.Drawing.Point(205, 49);
            this.OrgKeytextBox.Name = "OrgKeytextBox";
            this.OrgKeytextBox.Size = new System.Drawing.Size(35, 13);
            this.OrgKeytextBox.TabIndex = 53;
            this.OrgKeytextBox.Text = "label1";
            // 
            // PublishMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 397);
            this.Controls.Add(this.OrgKeytextBox);
            this.Controls.Add(this.SecurityKeytextBox);
            this.Controls.Add(this.SecurityKey);
            this.Controls.Add(this.OrgKeyLabel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.UpDateButton);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PublishMode";
            this.Text = "Change Publish Mode";
            this.Load += new System.EventHandler(this.PublishMode_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button UpDateButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton RemoveDataRadioButton;
        private System.Windows.Forms.RadioButton KeepDataRadioButton;
        private System.Windows.Forms.RadioButton FinalRadioButton;
        private System.Windows.Forms.RadioButton DraftRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label OrgKeyLabel;
        private System.Windows.Forms.Label SecurityKey;
        private System.Windows.Forms.TextBox SecurityKeytextBox;
        private System.Windows.Forms.Label OrgKeytextBox;
    }
}