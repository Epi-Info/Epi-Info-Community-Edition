namespace Epi.Enter.Dialogs
{
    partial class IOCoderDialog
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
            this.lblInd1 = new System.Windows.Forms.Label();
            this.lblOcc1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInd2 = new System.Windows.Forms.Label();
            this.lblOcc2 = new System.Windows.Forms.Label();
            this.lblOccCode = new System.Windows.Forms.Label();
            this.lblIndCode = new System.Windows.Forms.Label();
            this.lblOccTitle = new System.Windows.Forms.Label();
            this.lblIndTitle = new System.Windows.Forms.Label();
            this.lstIndustry = new System.Windows.Forms.ListBox();
            this.lstOccupation = new System.Windows.Forms.ListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblPossibilitiesInd = new System.Windows.Forms.Label();
            this.lblPossibilitiesOcc = new System.Windows.Forms.Label();
            this.lblInd3 = new System.Windows.Forms.Label();
            this.lblOcc3 = new System.Windows.Forms.Label();
            this.lblScheme1 = new System.Windows.Forms.Label();
            this.lblScheme = new System.Windows.Forms.Label();
            this.txtIndustry = new Epi.Windows.Enter.Controls.CueTextBox();
            this.txtOccupation = new Epi.Windows.Enter.Controls.CueTextBox();
            this.SuspendLayout();
            // 
            // lblInd1
            // 
            this.lblInd1.AutoSize = true;
            this.lblInd1.Location = new System.Drawing.Point(373, 9);
            this.lblInd1.Name = "lblInd1";
            this.lblInd1.Size = new System.Drawing.Size(47, 13);
            this.lblInd1.TabIndex = 2;
            this.lblInd1.Text = "Industry:";
            // 
            // lblOcc1
            // 
            this.lblOcc1.AutoSize = true;
            this.lblOcc1.Location = new System.Drawing.Point(4, 9);
            this.lblOcc1.Name = "lblOcc1";
            this.lblOcc1.Size = new System.Drawing.Size(65, 13);
            this.lblOcc1.TabIndex = 3;
            this.lblOcc1.Text = "Occupation:";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(663, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblInd2
            // 
            this.lblInd2.AutoSize = true;
            this.lblInd2.Location = new System.Drawing.Point(373, 35);
            this.lblInd2.Name = "lblInd2";
            this.lblInd2.Size = new System.Drawing.Size(75, 13);
            this.lblInd2.TabIndex = 5;
            this.lblInd2.Text = "Industry Code:";
            // 
            // lblOcc2
            // 
            this.lblOcc2.AutoSize = true;
            this.lblOcc2.Location = new System.Drawing.Point(4, 35);
            this.lblOcc2.Name = "lblOcc2";
            this.lblOcc2.Size = new System.Drawing.Size(93, 13);
            this.lblOcc2.TabIndex = 6;
            this.lblOcc2.Text = "Occupation Code:";
            // 
            // lblOccCode
            // 
            this.lblOccCode.AutoSize = true;
            this.lblOccCode.Location = new System.Drawing.Point(102, 35);
            this.lblOccCode.Name = "lblOccCode";
            this.lblOccCode.Size = new System.Drawing.Size(0, 13);
            this.lblOccCode.TabIndex = 9;
            // 
            // lblIndCode
            // 
            this.lblIndCode.AutoSize = true;
            this.lblIndCode.Location = new System.Drawing.Point(472, 35);
            this.lblIndCode.Name = "lblIndCode";
            this.lblIndCode.Size = new System.Drawing.Size(0, 13);
            this.lblIndCode.TabIndex = 8;
            // 
            // lblOccTitle
            // 
            this.lblOccTitle.AutoEllipsis = true;
            this.lblOccTitle.AutoSize = true;
            this.lblOccTitle.Location = new System.Drawing.Point(102, 60);
            this.lblOccTitle.MaximumSize = new System.Drawing.Size(267, 0);
            this.lblOccTitle.Name = "lblOccTitle";
            this.lblOccTitle.Size = new System.Drawing.Size(0, 13);
            this.lblOccTitle.TabIndex = 12;
            // 
            // lblIndTitle
            // 
            this.lblIndTitle.AutoEllipsis = true;
            this.lblIndTitle.AutoSize = true;
            this.lblIndTitle.Location = new System.Drawing.Point(472, 60);
            this.lblIndTitle.MaximumSize = new System.Drawing.Size(267, 0);
            this.lblIndTitle.Name = "lblIndTitle";
            this.lblIndTitle.Size = new System.Drawing.Size(0, 13);
            this.lblIndTitle.TabIndex = 11;
            // 
            // lstIndustry
            // 
            this.lstIndustry.FormattingEnabled = true;
            this.lstIndustry.Location = new System.Drawing.Point(376, 111);
            this.lstIndustry.Name = "lstIndustry";
            this.lstIndustry.Size = new System.Drawing.Size(360, 134);
            this.lstIndustry.TabIndex = 3;
            this.lstIndustry.SelectedIndexChanged += new System.EventHandler(this.lstIndustry_SelectedIndexChanged);
            // 
            // lstOccupation
            // 
            this.lstOccupation.FormattingEnabled = true;
            this.lstOccupation.Location = new System.Drawing.Point(7, 111);
            this.lstOccupation.Name = "lstOccupation";
            this.lstOccupation.Size = new System.Drawing.Size(360, 134);
            this.lstOccupation.TabIndex = 2;
            this.lstOccupation.SelectedIndexChanged += new System.EventHandler(this.lstOccupation_SelectedIndexChanged);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(582, 252);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblPossibilitiesInd
            // 
            this.lblPossibilitiesInd.AutoSize = true;
            this.lblPossibilitiesInd.Location = new System.Drawing.Point(373, 90);
            this.lblPossibilitiesInd.Name = "lblPossibilitiesInd";
            this.lblPossibilitiesInd.Size = new System.Drawing.Size(63, 13);
            this.lblPossibilitiesInd.TabIndex = 17;
            this.lblPossibilitiesInd.Text = "Possibilities:";
            // 
            // lblPossibilitiesOcc
            // 
            this.lblPossibilitiesOcc.AutoSize = true;
            this.lblPossibilitiesOcc.Location = new System.Drawing.Point(4, 90);
            this.lblPossibilitiesOcc.Name = "lblPossibilitiesOcc";
            this.lblPossibilitiesOcc.Size = new System.Drawing.Size(63, 13);
            this.lblPossibilitiesOcc.TabIndex = 18;
            this.lblPossibilitiesOcc.Text = "Possibilities:";
            // 
            // lblInd3
            // 
            this.lblInd3.AutoSize = true;
            this.lblInd3.Location = new System.Drawing.Point(373, 60);
            this.lblInd3.Name = "lblInd3";
            this.lblInd3.Size = new System.Drawing.Size(70, 13);
            this.lblInd3.TabIndex = 19;
            this.lblInd3.Text = "Industry Title:";
            // 
            // lblOcc3
            // 
            this.lblOcc3.AutoSize = true;
            this.lblOcc3.Location = new System.Drawing.Point(4, 60);
            this.lblOcc3.Name = "lblOcc3";
            this.lblOcc3.Size = new System.Drawing.Size(88, 13);
            this.lblOcc3.TabIndex = 20;
            this.lblOcc3.Text = "Occupation Title:";
            // 
            // lblScheme1
            // 
            this.lblScheme1.AutoSize = true;
            this.lblScheme1.Location = new System.Drawing.Point(5, 256);
            this.lblScheme1.Name = "lblScheme1";
            this.lblScheme1.Size = new System.Drawing.Size(49, 13);
            this.lblScheme1.TabIndex = 21;
            this.lblScheme1.Text = "Scheme:";
            this.lblScheme1.Visible = false;
            // 
            // lblScheme
            // 
            this.lblScheme.AutoSize = true;
            this.lblScheme.Location = new System.Drawing.Point(74, 256);
            this.lblScheme.Name = "lblScheme";
            this.lblScheme.Size = new System.Drawing.Size(0, 13);
            this.lblScheme.TabIndex = 22;
            this.lblScheme.Visible = false;
            // 
            // txtIndustry
            // 
            this.txtIndustry.Cue = "high school, doctor\'s office, auto sales, etc.";
            this.txtIndustry.Location = new System.Drawing.Point(469, 6);
            this.txtIndustry.MaxLength = 250;
            this.txtIndustry.Name = "txtIndustry";
            this.txtIndustry.Size = new System.Drawing.Size(267, 20);
            this.txtIndustry.TabIndex = 1;
            this.txtIndustry.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // txtOccupation
            // 
            this.txtOccupation.Cue = "receptionist, electrical engineer, farm hand, etc.";
            this.txtOccupation.Location = new System.Drawing.Point(100, 6);
            this.txtOccupation.MaxLength = 250;
            this.txtOccupation.Name = "txtOccupation";
            this.txtOccupation.Size = new System.Drawing.Size(267, 20);
            this.txtOccupation.TabIndex = 0;
            this.txtOccupation.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // IOCoderDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(744, 281);
            this.Controls.Add(this.txtOccupation);
            this.Controls.Add(this.txtIndustry);
            this.Controls.Add(this.lblScheme);
            this.Controls.Add(this.lblScheme1);
            this.Controls.Add(this.lblOcc3);
            this.Controls.Add(this.lblInd3);
            this.Controls.Add(this.lblPossibilitiesOcc);
            this.Controls.Add(this.lblPossibilitiesInd);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstOccupation);
            this.Controls.Add(this.lstIndustry);
            this.Controls.Add(this.lblOccTitle);
            this.Controls.Add(this.lblIndTitle);
            this.Controls.Add(this.lblOccCode);
            this.Controls.Add(this.lblIndCode);
            this.Controls.Add(this.lblOcc2);
            this.Controls.Add(this.lblInd2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblOcc1);
            this.Controls.Add(this.lblInd1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IOCoderDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Get Industry and Occupation Results";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblInd1;
        private System.Windows.Forms.Label lblOcc1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInd2;
        private System.Windows.Forms.Label lblOcc2;
        private System.Windows.Forms.Label lblOccCode;
        private System.Windows.Forms.Label lblIndCode;
        private System.Windows.Forms.Label lblOccTitle;
        private System.Windows.Forms.Label lblIndTitle;
        private System.Windows.Forms.ListBox lstIndustry;
        private System.Windows.Forms.ListBox lstOccupation;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblPossibilitiesInd;
        private System.Windows.Forms.Label lblPossibilitiesOcc;
        private System.Windows.Forms.Label lblInd3;
        private System.Windows.Forms.Label lblOcc3;
        private System.Windows.Forms.Label lblScheme1;
        private System.Windows.Forms.Label lblScheme;
        private Windows.Enter.Controls.CueTextBox txtIndustry;
        private Windows.Enter.Controls.CueTextBox txtOccupation;
    }
}

