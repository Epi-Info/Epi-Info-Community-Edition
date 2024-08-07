namespace Epi.Data.SQLite.Forms
{
    partial class ExcelExistingFileDialog
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.chkFirstRowContainsHeaders = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCancel.Location = new System.Drawing.Point(381, 138);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 31;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(285, 138);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 30;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnBrowse.Location = new System.Drawing.Point(381, 54);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(90, 23);
            this.btnBrowse.TabIndex = 29;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileName.Location = new System.Drawing.Point(12, 57);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Enabled = false;
            this.txtFileName.Size = new System.Drawing.Size(363, 20);
            this.txtFileName.TabIndex = 28;
            this.txtFileName.TextChanged += new System.EventHandler(this.txtFileName_TextChanged);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblFileName.Location = new System.Drawing.Point(9, 41);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(51, 13);
            this.lblFileName.TabIndex = 27;
            this.lblFileName.Text = "Location:";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblHeader.Location = new System.Drawing.Point(9, 14);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(349, 13);
            this.lblHeader.TabIndex = 26;
            this.lblHeader.Text = "Please enter the filename and path to the existing Excel workbook below";
            // 
            // chkFirstRowContainsHeaders
            // 
            this.chkFirstRowContainsHeaders.AutoSize = true;
            this.chkFirstRowContainsHeaders.Checked = true;
            this.chkFirstRowContainsHeaders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFirstRowContainsHeaders.Location = new System.Drawing.Point(13, 93);
            this.chkFirstRowContainsHeaders.Name = "chkFirstRowContainsHeaders";
            this.chkFirstRowContainsHeaders.Size = new System.Drawing.Size(198, 17);
            this.chkFirstRowContainsHeaders.TabIndex = 32;
            this.chkFirstRowContainsHeaders.Text = "First row contains header information";
            this.chkFirstRowContainsHeaders.UseVisualStyleBackColor = true;
            // 
            // ExcelExistingFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(483, 173);
            this.Controls.Add(this.chkFirstRowContainsHeaders);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExcelExistingFileDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Existing File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnCancel;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnOK;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Button btnBrowse;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.TextBox txtFileName;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblFileName;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        protected System.Windows.Forms.Label lblHeader;
        /// <summary>
        /// Control exposed to drived class for UI inheritance
        /// </summary>
        private System.Windows.Forms.CheckBox chkFirstRowContainsHeaders;



    }
}