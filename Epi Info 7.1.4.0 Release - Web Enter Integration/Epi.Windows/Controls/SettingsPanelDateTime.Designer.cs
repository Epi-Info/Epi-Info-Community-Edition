namespace Epi.Windows.Controls
{
    partial class SettingsPanelDateTime
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
            this.cmbDateFormat = new System.Windows.Forms.ComboBox();
            this.cmbTimeFormat = new System.Windows.Forms.ComboBox();
            this.cmbDateTimeFormat = new System.Windows.Forms.ComboBox();
            this.labelDateFormat = new System.Windows.Forms.Label();
            this.labelTimeFormat = new System.Windows.Forms.Label();
            this.labelDataTimeFormat = new System.Windows.Forms.Label();
            this.groupBoxDefaultDateTimeFormats = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // cmbDateFormat
            // 
            this.cmbDateFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDateFormat.FormattingEnabled = true;
            this.cmbDateFormat.Location = new System.Drawing.Point(24, 54);
            this.cmbDateFormat.Name = "cmbDateFormat";
            this.cmbDateFormat.Size = new System.Drawing.Size(376, 21);
            this.cmbDateFormat.TabIndex = 25;
            this.cmbDateFormat.SelectedIndexChanged += new System.EventHandler(this.cmbDateFormat_SelectedIndexChanged);
            this.cmbDateFormat.TextChanged += new System.EventHandler(this.cmbDateFormat_TextChanged);
            // 
            // cmbTimeFormat
            // 
            this.cmbTimeFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTimeFormat.Location = new System.Drawing.Point(24, 100);
            this.cmbTimeFormat.Name = "cmbTimeFormat";
            this.cmbTimeFormat.Size = new System.Drawing.Size(376, 21);
            this.cmbTimeFormat.TabIndex = 26;
            this.cmbTimeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeFormat_SelectedIndexChanged);
            this.cmbTimeFormat.TextChanged += new System.EventHandler(this.cmbTimeFormat_TextChanged);
            // 
            // cmbDateTimeFormat
            // 
            this.cmbDateTimeFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDateTimeFormat.Location = new System.Drawing.Point(24, 146);
            this.cmbDateTimeFormat.Name = "cmbDateTimeFormat";
            this.cmbDateTimeFormat.Size = new System.Drawing.Size(376, 21);
            this.cmbDateTimeFormat.TabIndex = 27;
            this.cmbDateTimeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbDateTimeFormat_SelectedIndexChanged);
            this.cmbDateTimeFormat.TextChanged += new System.EventHandler(this.cmbDateTimeFormat_TextChanged);
            // 
            // labelDateFormat
            // 
            this.labelDateFormat.AutoSize = true;
            this.labelDateFormat.Location = new System.Drawing.Point(21, 36);
            this.labelDateFormat.Name = "labelDateFormat";
            this.labelDateFormat.Size = new System.Drawing.Size(65, 13);
            this.labelDateFormat.TabIndex = 28;
            this.labelDateFormat.Text = "Date Format";
            // 
            // labelTimeFormat
            // 
            this.labelTimeFormat.AutoSize = true;
            this.labelTimeFormat.Location = new System.Drawing.Point(21, 82);
            this.labelTimeFormat.Name = "labelTimeFormat";
            this.labelTimeFormat.Size = new System.Drawing.Size(65, 13);
            this.labelTimeFormat.TabIndex = 29;
            this.labelTimeFormat.Text = "Time Format";
            // 
            // labelDataTimeFormat
            // 
            this.labelDataTimeFormat.AutoSize = true;
            this.labelDataTimeFormat.Location = new System.Drawing.Point(21, 128);
            this.labelDataTimeFormat.Name = "labelDataTimeFormat";
            this.labelDataTimeFormat.Size = new System.Drawing.Size(93, 13);
            this.labelDataTimeFormat.TabIndex = 30;
            this.labelDataTimeFormat.Text = "Date\\Time Format";
            // 
            // groupBoxDefaultDateTimeFormats
            // 
            this.groupBoxDefaultDateTimeFormats.Location = new System.Drawing.Point(3, 3);
            this.groupBoxDefaultDateTimeFormats.Name = "groupBoxDefaultDateTimeFormats";
            this.groupBoxDefaultDateTimeFormats.Size = new System.Drawing.Size(425, 198);
            this.groupBoxDefaultDateTimeFormats.TabIndex = 31;
            this.groupBoxDefaultDateTimeFormats.TabStop = false;
            this.groupBoxDefaultDateTimeFormats.Text = "Default Date\\Time Formats";
            // 
            // SettingsPanelDateTime
            // 
            this.Controls.Add(this.labelDataTimeFormat);
            this.Controls.Add(this.labelTimeFormat);
            this.Controls.Add(this.labelDateFormat);
            this.Controls.Add(this.cmbDateFormat);
            this.Controls.Add(this.cmbTimeFormat);
            this.Controls.Add(this.cmbDateTimeFormat);
            this.Controls.Add(this.groupBoxDefaultDateTimeFormats);
            this.Name = "SettingsPanelDateTime";
            this.Size = new System.Drawing.Size(431, 381);
            this.Load += new System.EventHandler(this.SettingsPanelDateTime_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.ComboBox cmbDateFormat;
		private System.Windows.Forms.ComboBox cmbTimeFormat;
        private System.Windows.Forms.ComboBox cmbDateTimeFormat;
        private System.Windows.Forms.Label labelDateFormat;
        private System.Windows.Forms.Label labelTimeFormat;
        private System.Windows.Forms.Label labelDataTimeFormat;
        private System.Windows.Forms.GroupBox groupBoxDefaultDateTimeFormats;

    }
}