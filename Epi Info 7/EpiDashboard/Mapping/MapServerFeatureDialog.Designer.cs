namespace EpiDashboard.Mapping
{
    partial class MapServerFeatureDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbxVisibleLayer = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblExample = new System.Windows.Forms.Label();
            this.lblFeatures = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.lblUrl = new System.Windows.Forms.Label();
            this.rdbCustom = new System.Windows.Forms.RadioButton();
            this.cbxMapServers = new System.Windows.Forms.ComboBox();
            this.rdbKnown = new System.Windows.Forms.RadioButton();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxVisibleLayer);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.lblExample);
            this.groupBox1.Controls.Add(this.lblFeatures);
            this.groupBox1.Controls.Add(this.txtURL);
            this.groupBox1.Controls.Add(this.lblUrl);
            this.groupBox1.Controls.Add(this.rdbCustom);
            this.groupBox1.Controls.Add(this.cbxMapServers);
            this.groupBox1.Controls.Add(this.rdbKnown);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(488, 190);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Map Server Location";
            // 
            // cbxVisibleLayer
            // 
            this.cbxVisibleLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxVisibleLayer.FormattingEnabled = true;
            this.cbxVisibleLayer.Location = new System.Drawing.Point(7, 157);
            this.cbxVisibleLayer.Name = "cbxVisibleLayer";
            this.cbxVisibleLayer.Size = new System.Drawing.Size(475, 21);
            this.cbxVisibleLayer.TabIndex = 9;
            this.cbxVisibleLayer.SelectedIndexChanged += new System.EventHandler(this.cbxVisibleLayer_SelectedIndexChanged);
            this.cbxVisibleLayer.DataSourceChanged += new System.EventHandler(this.cbxVisibleLayer_DataSourceChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Enabled = false;
            this.btnConnect.Location = new System.Drawing.Point(413, 116);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(65, 23);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblExample
            // 
            this.lblExample.AutoSize = true;
            this.lblExample.Enabled = false;
            this.lblExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExample.Location = new System.Drawing.Point(44, 100);
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new System.Drawing.Size(416, 13);
            this.lblExample.TabIndex = 7;
            this.lblExample.Text = "(example: http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer" +
                ")";
            // 
            // lblFeatures
            // 
            this.lblFeatures.AutoSize = true;
            this.lblFeatures.Enabled = false;
            this.lblFeatures.Location = new System.Drawing.Point(6, 141);
            this.lblFeatures.Name = "lblFeatures";
            this.lblFeatures.Size = new System.Drawing.Size(76, 13);
            this.lblFeatures.TabIndex = 5;
            this.lblFeatures.Text = "Select feature:";
            // 
            // txtURL
            // 
            this.txtURL.Enabled = false;
            this.txtURL.Location = new System.Drawing.Point(7, 118);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(400, 20);
            this.txtURL.TabIndex = 4;
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
            // 
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Enabled = false;
            this.lblUrl.Location = new System.Drawing.Point(6, 100);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(32, 13);
            this.lblUrl.TabIndex = 3;
            this.lblUrl.Text = "URL:";
            // 
            // rdbCustom
            // 
            this.rdbCustom.AutoSize = true;
            this.rdbCustom.Location = new System.Drawing.Point(7, 77);
            this.rdbCustom.Name = "rdbCustom";
            this.rdbCustom.Size = new System.Drawing.Size(210, 17);
            this.rdbCustom.TabIndex = 2;
            this.rdbCustom.TabStop = true;
            this.rdbCustom.Text = "Provide location of another Map Server";
            this.rdbCustom.UseVisualStyleBackColor = true;
            this.rdbCustom.CheckedChanged += new System.EventHandler(this.rdbCustom_CheckedChanged);
            // 
            // cbxMapServers
            // 
            this.cbxMapServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMapServers.FormattingEnabled = true;
            this.cbxMapServers.Items.AddRange(new object[] {
            "NationalMap.gov - New York County Boundaries",
            "NationalMap.gov - Rhode Island Zip Code Boundaries",
            "NationalMap.gov - U.S. State Boundaries",
            "NationalMap.gov - World Boundaries"});
            this.cbxMapServers.Location = new System.Drawing.Point(7, 43);
            this.cbxMapServers.Name = "cbxMapServers";
            this.cbxMapServers.Size = new System.Drawing.Size(475, 21);
            this.cbxMapServers.TabIndex = 1;
            this.cbxMapServers.SelectedIndexChanged += new System.EventHandler(this.cbxMapServers_SelectedIndexChanged);
            // 
            // rdbKnown
            // 
            this.rdbKnown.AutoSize = true;
            this.rdbKnown.Checked = true;
            this.rdbKnown.Location = new System.Drawing.Point(7, 20);
            this.rdbKnown.Name = "rdbKnown";
            this.rdbKnown.Size = new System.Drawing.Size(179, 17);
            this.rdbKnown.TabIndex = 0;
            this.rdbKnown.TabStop = true;
            this.rdbKnown.Text = "Connect to a known Map Server";
            this.rdbKnown.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(345, 209);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(426, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // MapServerFeatureDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(513, 243);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapServerFeatureDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Map Server";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblFeatures;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.RadioButton rdbCustom;
        private System.Windows.Forms.ComboBox cbxMapServers;
        private System.Windows.Forms.RadioButton rdbKnown;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblExample;
        private System.Windows.Forms.ComboBox cbxVisibleLayer;
    }
}