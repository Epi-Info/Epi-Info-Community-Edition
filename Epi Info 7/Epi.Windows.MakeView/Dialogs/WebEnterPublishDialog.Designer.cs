namespace Epi.Windows.MakeView.Dialogs
{
    partial class WebEnterPublishDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebEnterPublishDialog));
            this.btnCancel = new System.Windows.Forms.Button();
            this.ttWebPubDialog = new System.Windows.Forms.ToolTip(this.components);
            this.lblHeading2 = new System.Windows.Forms.Label();
            this.OrganizationKeyLinkLabel = new System.Windows.Forms.LinkLabel();
            this.OrganizationKeyValueLabel = new System.Windows.Forms.Label();
            this.WebSurveyOptionsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.DividerLabel = new System.Windows.Forms.Label();
            this.btnGo = new System.Windows.Forms.Button();
            this.tabEntry = new System.Windows.Forms.TabPage();
            this.btnPublishForm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPublishModeStatus = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblURL = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtSurveyKey = new System.Windows.Forms.TextBox();
            this.txtDataKey = new System.Windows.Forms.TextBox();
            this.lblSurveyKey = new System.Windows.Forms.Label();
            this.lblSecurityToken = new System.Windows.Forms.Label();
            this.txtStatusSummary = new System.Windows.Forms.TextBox();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnCopyAllURLs = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblSuccessNotice = new System.Windows.Forms.Label();
            this.btnShowLog = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.Importantlabel3 = new System.Windows.Forms.Label();
            this.lblSuccessNotice2 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOrganizationKey = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tabPublishWebForm = new System.Windows.Forms.TabControl();
            this.Shareable = new System.Windows.Forms.CheckBox();
            this.AccessAll = new System.Windows.Forms.CheckBox();
            this.tabEntry.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPublishWebForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblHeading2
            // 
            resources.ApplyResources(this.lblHeading2, "lblHeading2");
            this.lblHeading2.Name = "lblHeading2";
            // 
            // OrganizationKeyLinkLabel
            // 
            resources.ApplyResources(this.OrganizationKeyLinkLabel, "OrganizationKeyLinkLabel");
            this.OrganizationKeyLinkLabel.BackColor = System.Drawing.Color.Transparent;
            this.OrganizationKeyLinkLabel.Name = "OrganizationKeyLinkLabel";
            this.OrganizationKeyLinkLabel.TabStop = true;
            this.OrganizationKeyLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // OrganizationKeyValueLabel
            // 
            resources.ApplyResources(this.OrganizationKeyValueLabel, "OrganizationKeyValueLabel");
            this.OrganizationKeyValueLabel.Name = "OrganizationKeyValueLabel";
            // 
            // WebSurveyOptionsLinkLabel
            // 
            resources.ApplyResources(this.WebSurveyOptionsLinkLabel, "WebSurveyOptionsLinkLabel");
            this.WebSurveyOptionsLinkLabel.BackColor = System.Drawing.Color.Transparent;
            this.WebSurveyOptionsLinkLabel.Name = "WebSurveyOptionsLinkLabel";
            this.WebSurveyOptionsLinkLabel.TabStop = true;
            this.WebSurveyOptionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // DividerLabel
            // 
            resources.ApplyResources(this.DividerLabel, "DividerLabel");
            this.DividerLabel.Name = "DividerLabel";
            // 
            // btnGo
            // 
            resources.ApplyResources(this.btnGo, "btnGo");
            this.btnGo.Name = "btnGo";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // tabEntry
            // 
            this.tabEntry.Controls.Add(this.AccessAll);
            this.tabEntry.Controls.Add(this.Shareable);
            this.tabEntry.Controls.Add(this.progressBar);
            this.tabEntry.Controls.Add(this.txtOrganizationKey);
            this.tabEntry.Controls.Add(this.label2);
            this.tabEntry.Controls.Add(this.flowLayoutPanel1);
            this.tabEntry.Controls.Add(this.lblPublishModeStatus);
            this.tabEntry.Controls.Add(this.label1);
            this.tabEntry.Controls.Add(this.btnPublishForm);
            resources.ApplyResources(this.tabEntry, "tabEntry");
            this.tabEntry.Name = "tabEntry";
            this.tabEntry.UseVisualStyleBackColor = true;
            this.tabEntry.Click += new System.EventHandler(this.tabEntry_Click);
            // 
            // btnPublishForm
            // 
            resources.ApplyResources(this.btnPublishForm, "btnPublishForm");
            this.btnPublishForm.Name = "btnPublishForm";
            this.btnPublishForm.UseVisualStyleBackColor = true;
            this.btnPublishForm.Click += new System.EventHandler(this.btnPublishForm_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lblPublishModeStatus
            // 
            resources.ApplyResources(this.lblPublishModeStatus, "lblPublishModeStatus");
            this.lblPublishModeStatus.Name = "lblPublishModeStatus";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.txtStatus);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // txtStatus
            // 
            resources.ApplyResources(this.txtStatus, "txtStatus");
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.btnCopyAllURLs);
            this.panel2.Controls.Add(this.txtURL);
            this.panel2.Controls.Add(this.txtStatusSummary);
            this.panel2.Controls.Add(this.lblSecurityToken);
            this.panel2.Controls.Add(this.lblSurveyKey);
            this.panel2.Controls.Add(this.txtDataKey);
            this.panel2.Controls.Add(this.txtSurveyKey);
            this.panel2.Controls.Add(this.lblStatus);
            this.panel2.Controls.Add(this.lblURL);
            this.panel2.Name = "panel2";
            // 
            // lblURL
            // 
            resources.ApplyResources(this.lblURL, "lblURL");
            this.lblURL.Name = "lblURL";
            this.lblURL.Click += new System.EventHandler(this.lblURL_Click);
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // txtSurveyKey
            // 
            resources.ApplyResources(this.txtSurveyKey, "txtSurveyKey");
            this.txtSurveyKey.Name = "txtSurveyKey";
            this.txtSurveyKey.ReadOnly = true;
            // 
            // txtDataKey
            // 
            resources.ApplyResources(this.txtDataKey, "txtDataKey");
            this.txtDataKey.Name = "txtDataKey";
            this.txtDataKey.ReadOnly = true;
            // 
            // lblSurveyKey
            // 
            resources.ApplyResources(this.lblSurveyKey, "lblSurveyKey");
            this.lblSurveyKey.Name = "lblSurveyKey";
            // 
            // lblSecurityToken
            // 
            resources.ApplyResources(this.lblSecurityToken, "lblSecurityToken");
            this.lblSecurityToken.Name = "lblSecurityToken";
            // 
            // txtStatusSummary
            // 
            resources.ApplyResources(this.txtStatusSummary, "txtStatusSummary");
            this.txtStatusSummary.Name = "txtStatusSummary";
            this.txtStatusSummary.ReadOnly = true;
            // 
            // txtURL
            // 
            resources.ApplyResources(this.txtURL, "txtURL");
            this.txtURL.Name = "txtURL";
            this.txtURL.ReadOnly = true;
            // 
            // btnCopyAllURLs
            // 
            resources.ApplyResources(this.btnCopyAllURLs, "btnCopyAllURLs");
            this.btnCopyAllURLs.Name = "btnCopyAllURLs";
            this.btnCopyAllURLs.UseVisualStyleBackColor = true;
            this.btnCopyAllURLs.Click += new System.EventHandler(this.btnCopyAllURLs_Click);
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.lblSuccessNotice2);
            this.panel3.Controls.Add(this.Importantlabel3);
            this.panel3.Controls.Add(this.btnDetails);
            this.panel3.Controls.Add(this.btnShowLog);
            this.panel3.Controls.Add(this.lblSuccessNotice);
            this.panel3.Name = "panel3";
            // 
            // lblSuccessNotice
            // 
            this.lblSuccessNotice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))));
            resources.ApplyResources(this.lblSuccessNotice, "lblSuccessNotice");
            this.lblSuccessNotice.Name = "lblSuccessNotice";
            // 
            // btnShowLog
            // 
            resources.ApplyResources(this.btnShowLog, "btnShowLog");
            this.btnShowLog.Name = "btnShowLog";
            this.ttWebPubDialog.SetToolTip(this.btnShowLog, resources.GetString("btnShowLog.ToolTip"));
            this.btnShowLog.UseVisualStyleBackColor = true;
            this.btnShowLog.Click += new System.EventHandler(this.btnShowLog_Click);
            // 
            // btnDetails
            // 
            resources.ApplyResources(this.btnDetails, "btnDetails");
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // Importantlabel3
            // 
            resources.ApplyResources(this.Importantlabel3, "Importantlabel3");
            this.Importantlabel3.ForeColor = System.Drawing.Color.Red;
            this.Importantlabel3.Name = "Importantlabel3";
            // 
            // lblSuccessNotice2
            // 
            resources.ApplyResources(this.lblSuccessNotice2, "lblSuccessNotice2");
            this.lblSuccessNotice2.Name = "lblSuccessNotice2";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtOrganizationKey
            // 
            resources.ApplyResources(this.txtOrganizationKey, "txtOrganizationKey");
            this.txtOrganizationKey.Name = "txtOrganizationKey";
            this.txtOrganizationKey.TextChanged += new System.EventHandler(this.txtOrganizationKey_TextChanged);
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // tabPublishWebForm
            // 
            resources.ApplyResources(this.tabPublishWebForm, "tabPublishWebForm");
            this.tabPublishWebForm.Controls.Add(this.tabEntry);
            this.tabPublishWebForm.Name = "tabPublishWebForm";
            this.tabPublishWebForm.SelectedIndex = 0;
            // 
            // Shareable
            // 
            resources.ApplyResources(this.Shareable, "Shareable");
            this.Shareable.Name = "Shareable";
            this.Shareable.UseVisualStyleBackColor = true;
            this.Shareable.CheckedChanged += new System.EventHandler(this.Shareable_CheckedChanged);
            // 
            // AccessAll
            // 
            resources.ApplyResources(this.AccessAll, "AccessAll");
            this.AccessAll.Name = "AccessAll";
            this.AccessAll.UseVisualStyleBackColor = true;
            // 
            // WebEnterPublishDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DividerLabel);
            this.Controls.Add(this.WebSurveyOptionsLinkLabel);
            this.Controls.Add(this.OrganizationKeyValueLabel);
            this.Controls.Add(this.OrganizationKeyLinkLabel);
            this.Controls.Add(this.lblHeading2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.tabPublishWebForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebEnterPublishDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WebPublishDialog_FormClosing);
            this.Load += new System.EventHandler(this.WebPublishDialog_Load);
            this.tabEntry.ResumeLayout(false);
            this.tabEntry.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tabPublishWebForm.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip ttWebPubDialog;
        private System.Windows.Forms.Label lblHeading2;
        private System.Windows.Forms.LinkLabel OrganizationKeyLinkLabel;
        private System.Windows.Forms.Label OrganizationKeyValueLabel;
        private System.Windows.Forms.LinkLabel WebSurveyOptionsLinkLabel;
        private System.Windows.Forms.Label DividerLabel;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TabPage tabEntry;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtOrganizationKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblSuccessNotice2;
        private System.Windows.Forms.Label Importantlabel3;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Button btnShowLog;
        private System.Windows.Forms.Label lblSuccessNotice;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCopyAllURLs;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.TextBox txtStatusSummary;
        private System.Windows.Forms.Label lblSecurityToken;
        private System.Windows.Forms.Label lblSurveyKey;
        private System.Windows.Forms.TextBox txtDataKey;
        private System.Windows.Forms.TextBox txtSurveyKey;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblPublishModeStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnPublishForm;
        private System.Windows.Forms.TabControl tabPublishWebForm;
        private System.Windows.Forms.CheckBox AccessAll;
        private System.Windows.Forms.CheckBox Shareable;
    }
}