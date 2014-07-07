namespace Epi.Windows.MakeView.Dialogs
{
    partial class InputGuidDialog
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.btnNo = new System.Windows.Forms.Button();
            this.cmbInput = new System.Windows.Forms.ComboBox();
            this.txtTextInput = new System.Windows.Forms.TextBox();
            this.lbxInput = new System.Windows.Forms.ListBox();
            this.btnYes = new System.Windows.Forms.Button();
            this.txtMaskedTextInput = new System.Windows.Forms.MaskedTextBox();
            this.dtpDateTimeInput = new System.Windows.Forms.DateTimePicker();
            this.gbxMainGroup = new System.Windows.Forms.GroupBox();
            this.gbxMainGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(281, 632);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(362, 632);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(18, 27);
            this.lblPrompt.MaximumSize = new System.Drawing.Size(383, 0);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(111, 13);
            this.lblPrompt.TabIndex = 14;
            this.lblPrompt.Text = "labelText place holder";
            // 
            // btnNo
            // 
            this.btnNo.Location = new System.Drawing.Point(102, 204);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 23);
            this.btnNo.TabIndex = 21;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Visible = false;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // cmbInput
            // 
            this.cmbInput.FormattingEnabled = true;
            this.cmbInput.Location = new System.Drawing.Point(34, 435);
            this.cmbInput.Name = "cmbInput";
            this.cmbInput.Size = new System.Drawing.Size(383, 21);
            this.cmbInput.TabIndex = 18;
            this.cmbInput.Visible = false;
            // 
            // txtTextInput
            // 
            this.txtTextInput.Location = new System.Drawing.Point(21, 55);
            this.txtTextInput.Name = "txtTextInput";
            this.txtTextInput.Size = new System.Drawing.Size(383, 20);
            this.txtTextInput.TabIndex = 13;
            this.txtTextInput.Visible = false;
            this.txtTextInput.TextChanged += new System.EventHandler(this.txtTextInput_TextChanged);
            // 
            // lbxInput
            // 
            this.lbxInput.FormattingEnabled = true;
            this.lbxInput.Location = new System.Drawing.Point(34, 321);
            this.lbxInput.Name = "lbxInput";
            this.lbxInput.Size = new System.Drawing.Size(383, 95);
            this.lbxInput.TabIndex = 17;
            this.lbxInput.Visible = false;
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(21, 204);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 23);
            this.btnYes.TabIndex = 20;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Visible = false;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // txtMaskedTextInput
            // 
            this.txtMaskedTextInput.Location = new System.Drawing.Point(21, 81);
            this.txtMaskedTextInput.Name = "txtMaskedTextInput";
            this.txtMaskedTextInput.Size = new System.Drawing.Size(383, 20);
            this.txtMaskedTextInput.TabIndex = 15;
            this.txtMaskedTextInput.Visible = false;
            // 
            // dtpDateTimeInput
            // 
            this.dtpDateTimeInput.Location = new System.Drawing.Point(21, 107);
            this.dtpDateTimeInput.Name = "dtpDateTimeInput";
            this.dtpDateTimeInput.Size = new System.Drawing.Size(200, 20);
            this.dtpDateTimeInput.TabIndex = 16;
            this.dtpDateTimeInput.Visible = false;
            // 
            // gbxMainGroup
            // 
            this.gbxMainGroup.BackColor = System.Drawing.SystemColors.Control;
            this.gbxMainGroup.Controls.Add(this.lblPrompt);
            this.gbxMainGroup.Controls.Add(this.btnNo);
            this.gbxMainGroup.Controls.Add(this.txtTextInput);
            this.gbxMainGroup.Controls.Add(this.txtMaskedTextInput);
            this.gbxMainGroup.Controls.Add(this.dtpDateTimeInput);
            this.gbxMainGroup.Controls.Add(this.btnYes);
            this.gbxMainGroup.Location = new System.Drawing.Point(13, 13);
            this.gbxMainGroup.Name = "gbxMainGroup";
            this.gbxMainGroup.Size = new System.Drawing.Size(424, 597);
            this.gbxMainGroup.TabIndex = 22;
            this.gbxMainGroup.TabStop = false;
            this.gbxMainGroup.Text = "Input";
            // 
            // InputGuidDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(449, 667);
            this.Controls.Add(this.lbxInput);
            this.Controls.Add(this.cmbInput);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.gbxMainGroup);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputGuidDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Dialog";
            this.gbxMainGroup.ResumeLayout(false);
            this.gbxMainGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.ComboBox cmbInput;
        private System.Windows.Forms.TextBox txtTextInput;
        private System.Windows.Forms.ListBox lbxInput;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.MaskedTextBox txtMaskedTextInput;
        private System.Windows.Forms.DateTimePicker dtpDateTimeInput;
        private System.Windows.Forms.GroupBox gbxMainGroup;
    }
}