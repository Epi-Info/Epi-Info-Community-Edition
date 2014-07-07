namespace Epi.Windows.Controls
{
    partial class SettingsPanel
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

		#region Component Designer generated code

	
		
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.cmbYesAs = new System.Windows.Forms.ComboBox();
            this.cmbNoAs = new System.Windows.Forms.ComboBox();
            this.cmbMissingAs = new System.Windows.Forms.ComboBox();
            this.cbxShowPrompt = new System.Windows.Forms.CheckBox();
            this.cbxGraphics = new System.Windows.Forms.CheckBox();
            this.cbxHyperlinks = new System.Windows.Forms.CheckBox();
            this.cbxSelectCriteria = new System.Windows.Forms.CheckBox();
            this.cbxPercents = new System.Windows.Forms.CheckBox();
            this.cbxTablesOutput = new System.Windows.Forms.CheckBox();
            this.gbxStatistics = new System.Windows.Forms.GroupBox();
            this.rdbAdvanced = new System.Windows.Forms.RadioButton();
            this.rdbMinimal = new System.Windows.Forms.RadioButton();
            this.rdbIntermediate = new System.Windows.Forms.RadioButton();
            this.rdbNone = new System.Windows.Forms.RadioButton();
            this.cbxIncludeMissing = new System.Windows.Forms.CheckBox();
            this.gbxProcessRecords = new System.Windows.Forms.GroupBox();
            this.rdbDeleted = new System.Windows.Forms.RadioButton();
            this.rdbBoth = new System.Windows.Forms.RadioButton();
            this.rdbNormal = new System.Windows.Forms.RadioButton();
            this.lblMissingAs = new System.Windows.Forms.Label();
            this.lblYesAs = new System.Windows.Forms.Label();
            this.lblNoAs = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numericUpDownPrecision = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.gbxStatistics.SuspendLayout();
            this.gbxProcessRecords.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPrecision)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbYesAs
            // 
            this.cmbYesAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbYesAs.Location = new System.Drawing.Point(9, 32);
            this.cmbYesAs.Name = "cmbYesAs";
            this.cmbYesAs.Size = new System.Drawing.Size(121, 21);
            this.cmbYesAs.TabIndex = 25;
            this.cmbYesAs.SelectedIndexChanged += new System.EventHandler(this.cmbYesAs_SelectedIndexChanged);
            this.cmbYesAs.TextChanged += new System.EventHandler(this.cmbYesAs_TextChanged);
            // 
            // cmbNoAs
            // 
            this.cmbNoAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNoAs.Location = new System.Drawing.Point(145, 32);
            this.cmbNoAs.Name = "cmbNoAs";
            this.cmbNoAs.Size = new System.Drawing.Size(121, 21);
            this.cmbNoAs.TabIndex = 26;
            this.cmbNoAs.SelectedIndexChanged += new System.EventHandler(this.cmbNoAs_SelectedIndexChanged);
            this.cmbNoAs.TextChanged += new System.EventHandler(this.cmbNoAs_TextChanged);
            // 
            // cmbMissingAs
            // 
            this.cmbMissingAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbMissingAs.Location = new System.Drawing.Point(281, 32);
            this.cmbMissingAs.Name = "cmbMissingAs";
            this.cmbMissingAs.Size = new System.Drawing.Size(121, 21);
            this.cmbMissingAs.TabIndex = 27;
            this.cmbMissingAs.SelectedIndexChanged += new System.EventHandler(this.cmbMissingAs_SelectedIndexChanged);
            this.cmbMissingAs.TextChanged += new System.EventHandler(this.cmbMissingAs_TextChanged);
            // 
            // cbxShowPrompt
            // 
            this.cbxShowPrompt.Enabled = true;
            this.cbxShowPrompt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxShowPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxShowPrompt.Location = new System.Drawing.Point(6, 19);
            this.cbxShowPrompt.Name = "cbxShowPrompt";
            this.cbxShowPrompt.Size = new System.Drawing.Size(166, 24);
            this.cbxShowPrompt.TabIndex = 28;
            this.cbxShowPrompt.Text = "Show Complete Prompt";
            this.cbxShowPrompt.CheckedChanged += new System.EventHandler(this.cbxShowPrompt_CheckedChanged);
            // 
            // cbxGraphics
            // 
            this.cbxGraphics.Enabled = false;
            this.cbxGraphics.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxGraphics.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxGraphics.Location = new System.Drawing.Point(6, 43);
            this.cbxGraphics.Name = "cbxGraphics";
            this.cbxGraphics.Size = new System.Drawing.Size(166, 24);
            this.cbxGraphics.TabIndex = 29;
            this.cbxGraphics.Text = "Show Graphics";
            this.cbxGraphics.CheckedChanged += new System.EventHandler(this.cbxGraphics_CheckedChanged);
            // 
            // cbxHyperlinks
            // 
            this.cbxHyperlinks.Enabled = false;
            this.cbxHyperlinks.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxHyperlinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxHyperlinks.Location = new System.Drawing.Point(6, 67);
            this.cbxHyperlinks.Name = "cbxHyperlinks";
            this.cbxHyperlinks.Size = new System.Drawing.Size(166, 24);
            this.cbxHyperlinks.TabIndex = 30;
            this.cbxHyperlinks.Text = "Show Hyperlinks";
            this.cbxHyperlinks.CheckedChanged += new System.EventHandler(this.cbxHyperlinks_CheckedChanged);
            // 
            // cbxSelectCriteria
            // 
            this.cbxSelectCriteria.Enabled = false;
            this.cbxSelectCriteria.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxSelectCriteria.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxSelectCriteria.Location = new System.Drawing.Point(217, 19);
            this.cbxSelectCriteria.Name = "cbxSelectCriteria";
            this.cbxSelectCriteria.Size = new System.Drawing.Size(166, 24);
            this.cbxSelectCriteria.TabIndex = 31;
            this.cbxSelectCriteria.Text = "Show Selection Criteria";
            this.cbxSelectCriteria.CheckedChanged += new System.EventHandler(this.cbxSelectCriteria_CheckedChanged);
            // 
            // cbxPercents
            // 
            this.cbxPercents.Enabled = true;
            this.cbxPercents.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxPercents.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxPercents.Location = new System.Drawing.Point(217, 43);
            this.cbxPercents.Name = "cbxPercents";
            this.cbxPercents.Size = new System.Drawing.Size(166, 24);
            this.cbxPercents.TabIndex = 32;
            this.cbxPercents.Text = "Show Percents";
            this.cbxPercents.CheckedChanged += new System.EventHandler(this.cbxPercents_CheckedChanged);
            // 
            // cbxTablesOutput
            // 
            this.cbxTablesOutput.Enabled = false;
            this.cbxTablesOutput.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxTablesOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxTablesOutput.Location = new System.Drawing.Point(217, 67);
            this.cbxTablesOutput.Name = "cbxTablesOutput";
            this.cbxTablesOutput.Size = new System.Drawing.Size(166, 24);
            this.cbxTablesOutput.TabIndex = 33;
            this.cbxTablesOutput.Text = "Show Tables in Output";
            this.cbxTablesOutput.CheckedChanged += new System.EventHandler(this.cbxTablesOutput_CheckedChanged);
            // 
            // gbxStatistics
            // 
            this.gbxStatistics.Controls.Add(this.rdbAdvanced);
            this.gbxStatistics.Controls.Add(this.rdbMinimal);
            this.gbxStatistics.Controls.Add(this.rdbIntermediate);
            this.gbxStatistics.Controls.Add(this.rdbNone);
            this.gbxStatistics.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxStatistics.Location = new System.Drawing.Point(8, 184);
            this.gbxStatistics.Name = "gbxStatistics";
            this.gbxStatistics.Size = new System.Drawing.Size(287, 64);
            this.gbxStatistics.TabIndex = 34;
            this.gbxStatistics.TabStop = false;
            this.gbxStatistics.Text = "Statistics";
            // 
            // rdbAdvanced
            // 
            this.rdbAdvanced.Checked = true;
            this.rdbAdvanced.Enabled = false;
            this.rdbAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbAdvanced.Location = new System.Drawing.Point(145, 40);
            this.rdbAdvanced.Name = "rdbAdvanced";
            this.rdbAdvanced.Size = new System.Drawing.Size(127, 16);
            this.rdbAdvanced.TabIndex = 3;
            this.rdbAdvanced.TabStop = true;
            this.rdbAdvanced.Tag = "4";
            this.rdbAdvanced.Text = "Advanced";
            this.rdbAdvanced.Click += new System.EventHandler(this.StatisticsRadioButtonClick);
            // 
            // rdbMinimal
            // 
            this.rdbMinimal.Enabled = false;
            this.rdbMinimal.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbMinimal.Location = new System.Drawing.Point(145, 19);
            this.rdbMinimal.Name = "rdbMinimal";
            this.rdbMinimal.Size = new System.Drawing.Size(127, 16);
            this.rdbMinimal.TabIndex = 2;
            this.rdbMinimal.Tag = "2";
            this.rdbMinimal.Text = "Minimal";
            this.rdbMinimal.Click += new System.EventHandler(this.StatisticsRadioButtonClick);
            // 
            // rdbIntermediate
            // 
            this.rdbIntermediate.Enabled = false;
            this.rdbIntermediate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbIntermediate.Location = new System.Drawing.Point(12, 40);
            this.rdbIntermediate.Name = "rdbIntermediate";
            this.rdbIntermediate.Size = new System.Drawing.Size(127, 16);
            this.rdbIntermediate.TabIndex = 1;
            this.rdbIntermediate.Tag = "3";
            this.rdbIntermediate.Text = "Intermediate";
            this.rdbIntermediate.Click += new System.EventHandler(this.StatisticsRadioButtonClick);
            // 
            // rdbNone
            // 
            this.rdbNone.Enabled = false;
            this.rdbNone.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbNone.Location = new System.Drawing.Point(12, 19);
            this.rdbNone.Name = "rdbNone";
            this.rdbNone.Size = new System.Drawing.Size(127, 16);
            this.rdbNone.TabIndex = 0;
            this.rdbNone.Tag = "1";
            this.rdbNone.Text = "None";
            this.rdbNone.Click += new System.EventHandler(this.StatisticsRadioButtonClick);
            // 
            // cbxIncludeMissing
            // 
            this.cbxIncludeMissing.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxIncludeMissing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxIncludeMissing.Location = new System.Drawing.Point(8, 254);
            this.cbxIncludeMissing.Name = "cbxIncludeMissing";
            this.cbxIncludeMissing.Size = new System.Drawing.Size(287, 28);
            this.cbxIncludeMissing.TabIndex = 35;
            this.cbxIncludeMissing.Text = "Include Missing Values";
            this.cbxIncludeMissing.CheckedChanged += new System.EventHandler(this.cbxIncludeMissing_CheckedChanged);
            // 
            // gbxProcessRecords
            // 
            this.gbxProcessRecords.Controls.Add(this.rdbDeleted);
            this.gbxProcessRecords.Controls.Add(this.rdbBoth);
            this.gbxProcessRecords.Controls.Add(this.rdbNormal);
            this.gbxProcessRecords.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxProcessRecords.Location = new System.Drawing.Point(8, 284);
            this.gbxProcessRecords.Name = "gbxProcessRecords";
            this.gbxProcessRecords.Size = new System.Drawing.Size(234, 91);
            this.gbxProcessRecords.TabIndex = 36;
            this.gbxProcessRecords.TabStop = false;
            this.gbxProcessRecords.Text = "Process Records";
            // 
            // rdbDeleted
            // 
            this.rdbDeleted.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbDeleted.Location = new System.Drawing.Point(8, 41);
            this.rdbDeleted.Name = "rdbDeleted";
            this.rdbDeleted.Size = new System.Drawing.Size(187, 16);
            this.rdbDeleted.TabIndex = 2;
            this.rdbDeleted.Tag = "2";
            this.rdbDeleted.Text = "Deleted Records Only";
            this.rdbDeleted.Click += new System.EventHandler(this.ProcessRecordsRadioButtonClick);
            // 
            // rdbBoth
            // 
            this.rdbBoth.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbBoth.Location = new System.Drawing.Point(8, 61);
            this.rdbBoth.Name = "rdbBoth";
            this.rdbBoth.Size = new System.Drawing.Size(104, 16);
            this.rdbBoth.TabIndex = 1;
            this.rdbBoth.Tag = "3";
            this.rdbBoth.Text = "Both";
            this.rdbBoth.Click += new System.EventHandler(this.ProcessRecordsRadioButtonClick);
            // 
            // rdbNormal
            // 
            this.rdbNormal.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdbNormal.Location = new System.Drawing.Point(8, 19);
            this.rdbNormal.Name = "rdbNormal";
            this.rdbNormal.Size = new System.Drawing.Size(216, 16);
            this.rdbNormal.TabIndex = 0;
            this.rdbNormal.Tag = "1";
            this.rdbNormal.Text = "Undeleted Records only (Normal)";
            this.rdbNormal.Click += new System.EventHandler(this.ProcessRecordsRadioButtonClick);
            // 
            // lblMissingAs
            // 
            this.lblMissingAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMissingAs.Location = new System.Drawing.Point(281, 16);
            this.lblMissingAs.Name = "lblMissingAs";
            this.lblMissingAs.Size = new System.Drawing.Size(121, 16);
            this.lblMissingAs.TabIndex = 24;
            this.lblMissingAs.Text = "MISSING As:";
            // 
            // lblYesAs
            // 
            this.lblYesAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblYesAs.Location = new System.Drawing.Point(9, 16);
            this.lblYesAs.Name = "lblYesAs";
            this.lblYesAs.Size = new System.Drawing.Size(121, 16);
            this.lblYesAs.TabIndex = 22;
            this.lblYesAs.Text = "YES As:";
            // 
            // lblNoAs
            // 
            this.lblNoAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoAs.Location = new System.Drawing.Point(145, 16);
            this.lblNoAs.Name = "lblNoAs";
            this.lblNoAs.Size = new System.Drawing.Size(119, 16);
            this.lblNoAs.TabIndex = 23;
            this.lblNoAs.Text = "NO As:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblYesAs);
            this.groupBox1.Controls.Add(this.cmbYesAs);
            this.groupBox1.Controls.Add(this.lblNoAs);
            this.groupBox1.Controls.Add(this.cmbNoAs);
            this.groupBox1.Controls.Add(this.lblMissingAs);
            this.groupBox1.Controls.Add(this.cmbMissingAs);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(419, 65);
            this.groupBox1.TabIndex = 37;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Representation of Boolean Values";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbxShowPrompt);
            this.groupBox2.Controls.Add(this.cbxTablesOutput);
            this.groupBox2.Controls.Add(this.cbxPercents);
            this.groupBox2.Controls.Add(this.cbxGraphics);
            this.groupBox2.Controls.Add(this.cbxSelectCriteria);
            this.groupBox2.Controls.Add(this.cbxHyperlinks);
            this.groupBox2.Location = new System.Drawing.Point(8, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(419, 97);
            this.groupBox2.TabIndex = 38;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "HTML Output Options";
            // 
            // numericUpDownPrecision
            // 
            this.numericUpDownPrecision.Enabled = false;
            this.numericUpDownPrecision.Location = new System.Drawing.Point(301, 210);
            this.numericUpDownPrecision.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownPrecision.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPrecision.Name = "numericUpDownPrecision";
            this.numericUpDownPrecision.Size = new System.Drawing.Size(77, 20);
            this.numericUpDownPrecision.TabIndex = 40;
            this.numericUpDownPrecision.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(301, 192);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 41;
            this.label1.Text = "Precision:";
            // 
            // SettingsPanel
            // 
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownPrecision);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbxStatistics);
            this.Controls.Add(this.cbxIncludeMissing);
            this.Controls.Add(this.gbxProcessRecords);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(431, 381);
            this.Load += new System.EventHandler(this.SettingsPanel_Load);
            this.gbxStatistics.ResumeLayout(false);
            this.gbxProcessRecords.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPrecision)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion



		private System.Windows.Forms.ComboBox cmbYesAs;
		private System.Windows.Forms.ComboBox cmbNoAs;
		private System.Windows.Forms.ComboBox cmbMissingAs;
		private System.Windows.Forms.CheckBox cbxShowPrompt;
		private System.Windows.Forms.CheckBox cbxGraphics;
		private System.Windows.Forms.CheckBox cbxHyperlinks;
		private System.Windows.Forms.CheckBox cbxSelectCriteria;
		private System.Windows.Forms.CheckBox cbxPercents;
		private System.Windows.Forms.CheckBox cbxTablesOutput;
		private System.Windows.Forms.GroupBox gbxStatistics;
		private System.Windows.Forms.RadioButton rdbAdvanced;
		private System.Windows.Forms.RadioButton rdbMinimal;
		private System.Windows.Forms.RadioButton rdbIntermediate;
		private System.Windows.Forms.RadioButton rdbNone;
		private System.Windows.Forms.CheckBox cbxIncludeMissing;
		private System.Windows.Forms.GroupBox gbxProcessRecords;
		private System.Windows.Forms.RadioButton rdbDeleted;
		private System.Windows.Forms.RadioButton rdbBoth;
		private System.Windows.Forms.RadioButton rdbNormal;
		private System.Windows.Forms.Label lblMissingAs;
		private System.Windows.Forms.Label lblYesAs;
        private System.Windows.Forms.Label lblNoAs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown numericUpDownPrecision;
        private System.Windows.Forms.Label label1;
    }
}