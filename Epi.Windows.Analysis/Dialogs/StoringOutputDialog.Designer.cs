using System;
using System.Drawing;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Partial class Storing output dialog
    /// </summary>
    partial class StoringOutputDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StoringOutputDialog));
            this.txtOutputPrefix = new System.Windows.Forms.TextBox();
            this.lblOutputPrefix = new System.Windows.Forms.Label();
            this.lblOutputSequence = new System.Windows.Forms.Label();
            this.txtOutputSequence = new System.Windows.Forms.TextBox();
            this.lblResultsFolder = new System.Windows.Forms.Label();
            this.txtResultsFolder = new System.Windows.Forms.TextBox();
            this.lblArchiveFolder = new System.Windows.Forms.Label();
            this.txtArchiveFolder = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnResultsFolder = new System.Windows.Forms.Button();
            this.btnArchiveFolder = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblFlagSize = new System.Windows.Forms.Label();
            this.txtFlagSize = new System.Windows.Forms.TextBox();
            this.lblFlagNumber = new System.Windows.Forms.Label();
            this.txtFlagNumber = new System.Windows.Forms.TextBox();
            this.lblFlagDays = new System.Windows.Forms.Label();
            this.txtFlagAge = new System.Windows.Forms.TextBox();
            this.btnArchiveResults = new System.Windows.Forms.Button();
            this.btnDeleteResults = new System.Windows.Forms.Button();
            this.btnDeleteArchive = new System.Windows.Forms.Button();
            this.btnViewResults = new System.Windows.Forms.Button();
            this.btnViewArchive = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnApply = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseImageList
            // 
            this.baseImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("baseImageList.ImageStream")));
            this.baseImageList.Images.SetKeyName(0, "");
            this.baseImageList.Images.SetKeyName(1, "");
            this.baseImageList.Images.SetKeyName(2, "");
            this.baseImageList.Images.SetKeyName(3, "");
            this.baseImageList.Images.SetKeyName(4, "");
            this.baseImageList.Images.SetKeyName(5, "");
            this.baseImageList.Images.SetKeyName(6, "");
            this.baseImageList.Images.SetKeyName(7, "");
            this.baseImageList.Images.SetKeyName(8, "");
            this.baseImageList.Images.SetKeyName(9, "");
            this.baseImageList.Images.SetKeyName(10, "");
            this.baseImageList.Images.SetKeyName(11, "");
            this.baseImageList.Images.SetKeyName(12, "");
            this.baseImageList.Images.SetKeyName(13, "");
            this.baseImageList.Images.SetKeyName(14, "");
            this.baseImageList.Images.SetKeyName(15, "");
            this.baseImageList.Images.SetKeyName(16, "");
            this.baseImageList.Images.SetKeyName(17, "");
            this.baseImageList.Images.SetKeyName(18, "");
            this.baseImageList.Images.SetKeyName(19, "");
            this.baseImageList.Images.SetKeyName(20, "");
            this.baseImageList.Images.SetKeyName(21, "");
            this.baseImageList.Images.SetKeyName(22, "");
            this.baseImageList.Images.SetKeyName(23, "");
            this.baseImageList.Images.SetKeyName(24, "");
            this.baseImageList.Images.SetKeyName(25, "");
            this.baseImageList.Images.SetKeyName(26, "");
            this.baseImageList.Images.SetKeyName(27, "");
            this.baseImageList.Images.SetKeyName(28, "");
            this.baseImageList.Images.SetKeyName(29, "");
            this.baseImageList.Images.SetKeyName(30, "");
            this.baseImageList.Images.SetKeyName(31, "");
            this.baseImageList.Images.SetKeyName(32, "");
            this.baseImageList.Images.SetKeyName(33, "");
            this.baseImageList.Images.SetKeyName(34, "");
            this.baseImageList.Images.SetKeyName(35, "");
            this.baseImageList.Images.SetKeyName(36, "");
            this.baseImageList.Images.SetKeyName(37, "");
            this.baseImageList.Images.SetKeyName(38, "");
            this.baseImageList.Images.SetKeyName(39, "");
            this.baseImageList.Images.SetKeyName(40, "");
            this.baseImageList.Images.SetKeyName(41, "");
            this.baseImageList.Images.SetKeyName(42, "");
            this.baseImageList.Images.SetKeyName(43, "");
            this.baseImageList.Images.SetKeyName(44, "");
            this.baseImageList.Images.SetKeyName(45, "");
            this.baseImageList.Images.SetKeyName(46, "");
            this.baseImageList.Images.SetKeyName(47, "");
            this.baseImageList.Images.SetKeyName(48, "");
            this.baseImageList.Images.SetKeyName(49, "");
            this.baseImageList.Images.SetKeyName(50, "");
            this.baseImageList.Images.SetKeyName(51, "");
            this.baseImageList.Images.SetKeyName(52, "");
            this.baseImageList.Images.SetKeyName(53, "");
            this.baseImageList.Images.SetKeyName(54, "");
            this.baseImageList.Images.SetKeyName(55, "");
            this.baseImageList.Images.SetKeyName(56, "");
            this.baseImageList.Images.SetKeyName(57, "");
            this.baseImageList.Images.SetKeyName(58, "");
            this.baseImageList.Images.SetKeyName(59, "");
            this.baseImageList.Images.SetKeyName(60, "");
            this.baseImageList.Images.SetKeyName(61, "");
            this.baseImageList.Images.SetKeyName(62, "");
            this.baseImageList.Images.SetKeyName(63, "");
            this.baseImageList.Images.SetKeyName(64, "");
            this.baseImageList.Images.SetKeyName(65, "");
            this.baseImageList.Images.SetKeyName(66, "");
            this.baseImageList.Images.SetKeyName(67, "");
            this.baseImageList.Images.SetKeyName(68, "");
            this.baseImageList.Images.SetKeyName(69, "");
            this.baseImageList.Images.SetKeyName(70, "");
            this.baseImageList.Images.SetKeyName(71, "");
            this.baseImageList.Images.SetKeyName(72, "");
            this.baseImageList.Images.SetKeyName(73, "");
            this.baseImageList.Images.SetKeyName(74, "");
            this.baseImageList.Images.SetKeyName(75, "");
            this.baseImageList.Images.SetKeyName(76, "");
            this.baseImageList.Images.SetKeyName(77, "");
            this.baseImageList.Images.SetKeyName(78, "");
            this.baseImageList.Images.SetKeyName(79, "");
            // 
            // txtOutputPrefix
            // 
            this.txtOutputPrefix.Location = new System.Drawing.Point(15, 25);
            this.txtOutputPrefix.Name = "txtOutputPrefix";
            this.txtOutputPrefix.Size = new System.Drawing.Size(186, 20);
            this.txtOutputPrefix.TabIndex = 0;
            // 
            // lblOutputPrefix
            // 
            this.lblOutputPrefix.Location = new System.Drawing.Point(15, 9);
            this.lblOutputPrefix.Name = "lblOutputPrefix";
            this.lblOutputPrefix.Size = new System.Drawing.Size(186, 13);
            this.lblOutputPrefix.TabIndex = 1;
            this.lblOutputPrefix.Text = "Output File Prefix:";
            // 
            // lblOutputSequence
            // 
            this.lblOutputSequence.Location = new System.Drawing.Point(15, 51);
            this.lblOutputSequence.Name = "lblOutputSequence";
            this.lblOutputSequence.Size = new System.Drawing.Size(186, 13);
            this.lblOutputSequence.TabIndex = 3;
            this.lblOutputSequence.Text = "Output File Sequence:";
            // 
            // txtOutputSequence
            // 
            this.txtOutputSequence.Location = new System.Drawing.Point(15, 67);
            this.txtOutputSequence.Name = "txtOutputSequence";
            this.txtOutputSequence.Size = new System.Drawing.Size(186, 20);
            this.txtOutputSequence.TabIndex = 2;
            // 
            // lblResultsFolder
            // 
            this.lblResultsFolder.Location = new System.Drawing.Point(12, 96);
            this.lblResultsFolder.Name = "lblResultsFolder";
            this.lblResultsFolder.Size = new System.Drawing.Size(77, 13);
            this.lblResultsFolder.TabIndex = 5;
            this.lblResultsFolder.Text = "Results Folder:";
            // 
            // txtResultsFolder
            // 
            this.txtResultsFolder.Location = new System.Drawing.Point(15, 112);
            this.txtResultsFolder.Name = "txtResultsFolder";
            this.txtResultsFolder.Size = new System.Drawing.Size(334, 20);
            this.txtResultsFolder.TabIndex = 4;
            // 
            // lblArchiveFolder
            // 
            this.lblArchiveFolder.Location = new System.Drawing.Point(12, 174);
            this.lblArchiveFolder.Name = "lblArchiveFolder";
            this.lblArchiveFolder.Size = new System.Drawing.Size(78, 13);
            this.lblArchiveFolder.TabIndex = 7;
            this.lblArchiveFolder.Text = "Archive Folder:";
            // 
            // txtArchiveFolder
            // 
            this.txtArchiveFolder.Location = new System.Drawing.Point(15, 190);
            this.txtArchiveFolder.Name = "txtArchiveFolder";
            this.txtArchiveFolder.Size = new System.Drawing.Size(334, 20);
            this.txtArchiveFolder.TabIndex = 6;
            // 
            // btnResultsFolder
            // 
            this.btnResultsFolder.Location = new System.Drawing.Point(355, 110);
            this.btnResultsFolder.Name = "btnResultsFolder";
            this.btnResultsFolder.Size = new System.Drawing.Size(90, 23);
            this.btnResultsFolder.TabIndex = 8;
            this.btnResultsFolder.Text = "Browse";
            this.btnResultsFolder.UseVisualStyleBackColor = true;
            this.btnResultsFolder.Click += new System.EventHandler(this.btnResultsFolder_Click);
            // 
            // btnArchiveFolder
            // 
            this.btnArchiveFolder.Location = new System.Drawing.Point(355, 188);
            this.btnArchiveFolder.Name = "btnArchiveFolder";
            this.btnArchiveFolder.Size = new System.Drawing.Size(90, 23);
            this.btnArchiveFolder.TabIndex = 9;
            this.btnArchiveFolder.Text = "Browse";
            this.btnArchiveFolder.UseVisualStyleBackColor = true;
            this.btnArchiveFolder.Click += new System.EventHandler(this.btnArchiveFolder_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(163, 393);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(90, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(355, 393);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblFlagSize);
            this.groupBox1.Controls.Add(this.txtFlagSize);
            this.groupBox1.Controls.Add(this.lblFlagNumber);
            this.groupBox1.Controls.Add(this.txtFlagNumber);
            this.groupBox1.Controls.Add(this.lblFlagDays);
            this.groupBox1.Controls.Add(this.txtFlagAge);
            this.groupBox1.Location = new System.Drawing.Point(12, 264);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(433, 123);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Flag Output Files Exceeding These Limits";
            // 
            // lblFlagSize
            // 
            this.lblFlagSize.Location = new System.Drawing.Point(6, 85);
            this.lblFlagSize.Name = "lblFlagSize";
            this.lblFlagSize.Size = new System.Drawing.Size(72, 13);
            this.lblFlagSize.TabIndex = 7;
            this.lblFlagSize.Text = "File Size (KB):";
            // 
            // txtFlagSize
            // 
            this.txtFlagSize.Location = new System.Drawing.Point(131, 82);
            this.txtFlagSize.Name = "txtFlagSize";
            this.txtFlagSize.Size = new System.Drawing.Size(135, 20);
            this.txtFlagSize.TabIndex = 6;
            // 
            // lblFlagNumber
            // 
            this.lblFlagNumber.Location = new System.Drawing.Point(6, 59);
            this.lblFlagNumber.Name = "lblFlagNumber";
            this.lblFlagNumber.Size = new System.Drawing.Size(97, 13);
            this.lblFlagNumber.TabIndex = 5;
            this.lblFlagNumber.Text = "Number of Results:";
            // 
            // txtFlagNumber
            // 
            this.txtFlagNumber.Location = new System.Drawing.Point(131, 56);
            this.txtFlagNumber.Name = "txtFlagNumber";
            this.txtFlagNumber.Size = new System.Drawing.Size(135, 20);
            this.txtFlagNumber.TabIndex = 4;
            // 
            // lblFlagDays
            // 
            this.lblFlagDays.Location = new System.Drawing.Point(6, 33);
            this.lblFlagDays.Name = "lblFlagDays";
            this.lblFlagDays.Size = new System.Drawing.Size(68, 13);
            this.lblFlagDays.TabIndex = 3;
            this.lblFlagDays.Text = "Age In Days:";
            // 
            // txtFlagAge
            // 
            this.txtFlagAge.Location = new System.Drawing.Point(131, 30);
            this.txtFlagAge.Name = "txtFlagAge";
            this.txtFlagAge.Size = new System.Drawing.Size(135, 20);
            this.txtFlagAge.TabIndex = 2;
            // 
            // btnArchiveResults
            // 
            this.btnArchiveResults.Location = new System.Drawing.Point(111, 138);
            this.btnArchiveResults.Name = "btnArchiveResults";
            this.btnArchiveResults.Size = new System.Drawing.Size(90, 23);
            this.btnArchiveResults.TabIndex = 14;
            this.btnArchiveResults.Text = "Archive...";
            this.btnArchiveResults.UseVisualStyleBackColor = true;
            this.btnArchiveResults.Click += new System.EventHandler(this.btnArchiveResults_Click);
            // 
            // btnDeleteResults
            // 
            this.btnDeleteResults.Location = new System.Drawing.Point(207, 138);
            this.btnDeleteResults.Name = "btnDeleteResults";
            this.btnDeleteResults.Size = new System.Drawing.Size(90, 23);
            this.btnDeleteResults.TabIndex = 16;
            this.btnDeleteResults.Text = "Delete...";
            this.btnDeleteResults.UseVisualStyleBackColor = true;
            this.btnDeleteResults.Click += new System.EventHandler(this.btnDeleteResults_Click);
            // 
            // btnDeleteArchive
            // 
            this.btnDeleteArchive.Location = new System.Drawing.Point(111, 216);
            this.btnDeleteArchive.Name = "btnDeleteArchive";
            this.btnDeleteArchive.Size = new System.Drawing.Size(90, 23);
            this.btnDeleteArchive.TabIndex = 17;
            this.btnDeleteArchive.Text = "Delete...";
            this.btnDeleteArchive.UseVisualStyleBackColor = true;
            this.btnDeleteArchive.Click += new System.EventHandler(this.btnDeleteArchive_Click);
            // 
            // btnViewResults
            // 
            this.btnViewResults.Location = new System.Drawing.Point(15, 138);
            this.btnViewResults.Name = "btnViewResults";
            this.btnViewResults.Size = new System.Drawing.Size(90, 23);
            this.btnViewResults.TabIndex = 18;
            this.btnViewResults.Text = "View...";
            this.btnViewResults.UseVisualStyleBackColor = true;
            this.btnViewResults.Click += new System.EventHandler(this.btnViewResults_Click);
            // 
            // btnViewArchive
            // 
            this.btnViewArchive.Location = new System.Drawing.Point(15, 216);
            this.btnViewArchive.Name = "btnViewArchive";
            this.btnViewArchive.Size = new System.Drawing.Size(90, 23);
            this.btnViewArchive.TabIndex = 19;
            this.btnViewArchive.Text = "View...";
            this.btnViewArchive.UseVisualStyleBackColor = true;
            this.btnViewArchive.Click += new System.EventHandler(this.btnViewArchive_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(259, 393);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(90, 23);
            this.btnApply.TabIndex = 20;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // StoringOutputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(455, 425);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnViewArchive);
            this.Controls.Add(this.btnViewResults);
            this.Controls.Add(this.btnDeleteArchive);
            this.Controls.Add(this.btnDeleteResults);
            this.Controls.Add(this.btnArchiveResults);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnArchiveFolder);
            this.Controls.Add(this.btnResultsFolder);
            this.Controls.Add(this.lblArchiveFolder);
            this.Controls.Add(this.txtArchiveFolder);
            this.Controls.Add(this.lblResultsFolder);
            this.Controls.Add(this.txtResultsFolder);
            this.Controls.Add(this.lblOutputSequence);
            this.Controls.Add(this.txtOutputSequence);
            this.Controls.Add(this.lblOutputPrefix);
            this.Controls.Add(this.txtOutputPrefix);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StoringOutputDialog";
            this.ShowIcon = false;
            this.Text = "Storing Output";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtOutputPrefix;
        private System.Windows.Forms.Label lblOutputPrefix;
        private System.Windows.Forms.Label lblOutputSequence;
        private System.Windows.Forms.TextBox txtOutputSequence;
        private System.Windows.Forms.Label lblResultsFolder;
        private System.Windows.Forms.TextBox txtResultsFolder;
        private System.Windows.Forms.Label lblArchiveFolder;
        private System.Windows.Forms.TextBox txtArchiveFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnResultsFolder;
        private System.Windows.Forms.Button btnArchiveFolder;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblFlagSize;
        private System.Windows.Forms.TextBox txtFlagSize;
        private System.Windows.Forms.Label lblFlagNumber;
        private System.Windows.Forms.TextBox txtFlagNumber;
        private System.Windows.Forms.Label lblFlagDays;
        private System.Windows.Forms.TextBox txtFlagAge;
        private System.Windows.Forms.Button btnArchiveResults;
        private System.Windows.Forms.Button btnDeleteResults;
        private System.Windows.Forms.Button btnDeleteArchive;
        private System.Windows.Forms.Button btnViewResults;
        private System.Windows.Forms.Button btnViewArchive;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnApply;
    }
}