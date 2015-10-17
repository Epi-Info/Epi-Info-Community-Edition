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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapServerFeatureDialog));
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
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cbxVisibleLayer
            // 
            this.cbxVisibleLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxVisibleLayer.FormattingEnabled = true;
            resources.ApplyResources(this.cbxVisibleLayer, "cbxVisibleLayer");
            this.cbxVisibleLayer.Name = "cbxVisibleLayer";
            this.cbxVisibleLayer.SelectedIndexChanged += new System.EventHandler(this.cbxVisibleLayer_SelectedIndexChanged);
            this.cbxVisibleLayer.DataSourceChanged += new System.EventHandler(this.cbxVisibleLayer_DataSourceChanged);
            // 
            // btnConnect
            // 
            resources.ApplyResources(this.btnConnect, "btnConnect");
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblExample
            // 
            resources.ApplyResources(this.lblExample, "lblExample");
            this.lblExample.Name = "lblExample";
            // 
            // lblFeatures
            // 
            resources.ApplyResources(this.lblFeatures, "lblFeatures");
            this.lblFeatures.Name = "lblFeatures";
            // 
            // txtURL
            // 
            resources.ApplyResources(this.txtURL, "txtURL");
            this.txtURL.Name = "txtURL";
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
            // 
            // lblUrl
            // 
            resources.ApplyResources(this.lblUrl, "lblUrl");
            this.lblUrl.Name = "lblUrl";
            // 
            // rdbCustom
            // 
            resources.ApplyResources(this.rdbCustom, "rdbCustom");
            this.rdbCustom.Name = "rdbCustom";
            this.rdbCustom.TabStop = true;
            this.rdbCustom.UseVisualStyleBackColor = true;
            this.rdbCustom.CheckedChanged += new System.EventHandler(this.rdbCustom_CheckedChanged);
            // 
            // cbxMapServers
            // 
            this.cbxMapServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMapServers.FormattingEnabled = true;
            this.cbxMapServers.Items.AddRange(new object[] {
            resources.GetString("cbxMapServers.Items"),
            resources.GetString("cbxMapServers.Items1"),
            resources.GetString("cbxMapServers.Items2"),
            resources.GetString("cbxMapServers.Items3")});
            resources.ApplyResources(this.cbxMapServers, "cbxMapServers");
            this.cbxMapServers.Name = "cbxMapServers";
            this.cbxMapServers.SelectedIndexChanged += new System.EventHandler(this.cbxMapServers_SelectedIndexChanged);
            // 
            // rdbKnown
            // 
            resources.ApplyResources(this.rdbKnown, "rdbKnown");
            this.rdbKnown.Checked = true;
            this.rdbKnown.Name = "rdbKnown";
            this.rdbKnown.TabStop = true;
            this.rdbKnown.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // MapServerFeatureDialog
            // 
            this.AcceptButton = this.btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapServerFeatureDialog";
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