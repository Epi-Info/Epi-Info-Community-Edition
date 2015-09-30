namespace Epi.Windows.MakeView.Dialogs
{
    partial class WebPublishDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebPublishDialog));
            this.dtpSurveyClosingDate = new System.Windows.Forms.DateTimePicker();
            this.txtDepartment = new System.Windows.Forms.TextBox();
            this.txtIntroductionText = new System.Windows.Forms.TextBox();
            this.txtExitText = new System.Windows.Forms.TextBox();
            this.txtOrganization = new System.Windows.Forms.TextBox();
            this.txtSurveyID = new System.Windows.Forms.TextBox();
            this.txtSurveyName = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.tabPublishWebForm = new System.Windows.Forms.TabControl();
            this.tabIntro = new System.Windows.Forms.TabPage();
            this.ClosingTimecomboBox = new System.Windows.Forms.ComboBox();
            this.closingTimelabel = new System.Windows.Forms.Label();
            this.StartTimelabel = new System.Windows.Forms.Label();
            this.StartTimecomboBox = new System.Windows.Forms.ComboBox();
            this.StartDateDatePicker = new System.Windows.Forms.DateTimePicker();
            this.lblSurveyStartDate = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.pnlSurveyName = new System.Windows.Forms.Panel();
            this.lblClosingDate = new System.Windows.Forms.Label();
            this.pnlSurvNumOrgDept = new System.Windows.Forms.Panel();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.lblSurvNum = new System.Windows.Forms.Label();
            this.lblDepartment = new System.Windows.Forms.Label();
            this.tabExit = new System.Windows.Forms.TabPage();
            this.rdbSingleResponse = new System.Windows.Forms.RadioButton();
            this.lblSurveyType = new System.Windows.Forms.Label();
            this.rdbMultipleResponse = new System.Windows.Forms.RadioButton();
            this.grpExitText = new System.Windows.Forms.GroupBox();
            this.picExitCheckmark = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblDepMirr = new System.Windows.Forms.Label();
            this.lblOrgMirr = new System.Windows.Forms.Label();
            this.lblSurvNumMirr = new System.Windows.Forms.Label();
            this.txtOrganizationMirror = new System.Windows.Forms.TextBox();
            this.txtSurveyIDMirror = new System.Windows.Forms.TextBox();
            this.txtDepartmentMirror = new System.Windows.Forms.TextBox();
            this.pnlSurveyNameMirror = new System.Windows.Forms.Panel();
            this.txtSurveyNameMirror = new System.Windows.Forms.TextBox();
            this.tabEntry = new System.Windows.Forms.TabPage();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtOrganizationKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnShowLog = new System.Windows.Forms.Button();
            this.lblSuccessNotice2 = new System.Windows.Forms.Label();
            this.lblSuccessNotice = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnDataKeyCopy = new System.Windows.Forms.Button();
            this.btnKeyCopy = new System.Windows.Forms.Button();
            this.txtStatusSummary = new System.Windows.Forms.TextBox();
            this.lblSecurityToken = new System.Windows.Forms.Label();
            this.lblSurveyKey = new System.Windows.Forms.Label();
            this.txtDataKey = new System.Windows.Forms.TextBox();
            this.txtSurveyKey = new System.Windows.Forms.TextBox();
            this.btnCopyAllURLs = new System.Windows.Forms.Button();
            this.btnURLCopy = new System.Windows.Forms.Button();
            this.btnGo = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblURL = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.lblPublishModeStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPublishForm = new System.Windows.Forms.Button();
            this.ttWebPubDialog = new System.Windows.Forms.ToolTip(this.components);
            this.btnNext = new System.Windows.Forms.Button();
            this.lblHeading1 = new System.Windows.Forms.Label();
            this.lblHeading2 = new System.Windows.Forms.Label();
            this.WebSurveyOptionsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.tabPublishWebForm.SuspendLayout();
            this.tabIntro.SuspendLayout();
            this.pnlSurveyName.SuspendLayout();
            this.pnlSurvNumOrgDept.SuspendLayout();
            this.tabExit.SuspendLayout();
            this.grpExitText.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picExitCheckmark)).BeginInit();
            this.panel1.SuspendLayout();
            this.pnlSurveyNameMirror.SuspendLayout();
            this.tabEntry.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtpSurveyClosingDate
            // 
            resources.ApplyResources(this.dtpSurveyClosingDate, "dtpSurveyClosingDate");
            this.dtpSurveyClosingDate.Name = "dtpSurveyClosingDate";
            // 
            // txtDepartment
            // 
            resources.ApplyResources(this.txtDepartment, "txtDepartment");
            this.txtDepartment.Name = "txtDepartment";
            this.txtDepartment.TextChanged += new System.EventHandler(this.txtDepartment_TextChanged);
            this.txtDepartment.Enter += new System.EventHandler(this.txtDepartment_Enter);
            this.txtDepartment.Leave += new System.EventHandler(this.txtDepartment_Leave);
            // 
            // txtIntroductionText
            // 
            resources.ApplyResources(this.txtIntroductionText, "txtIntroductionText");
            this.txtIntroductionText.Name = "txtIntroductionText";
            this.txtIntroductionText.Enter += new System.EventHandler(this.txtIntroductionText_Enter);
            this.txtIntroductionText.Leave += new System.EventHandler(this.txtIntroductionText_Leave);
            // 
            // txtExitText
            // 
            resources.ApplyResources(this.txtExitText, "txtExitText");
            this.txtExitText.Name = "txtExitText";
            this.txtExitText.Enter += new System.EventHandler(this.txtExitText_Enter);
            this.txtExitText.Leave += new System.EventHandler(this.txtExitText_Leave);
            // 
            // txtOrganization
            // 
            resources.ApplyResources(this.txtOrganization, "txtOrganization");
            this.txtOrganization.Name = "txtOrganization";
            this.txtOrganization.TextChanged += new System.EventHandler(this.txtOrganization_TextChanged);
            this.txtOrganization.Enter += new System.EventHandler(this.txtOrganization_Enter);
            this.txtOrganization.Leave += new System.EventHandler(this.txtOrganization_Leave);
            // 
            // txtSurveyID
            // 
            resources.ApplyResources(this.txtSurveyID, "txtSurveyID");
            this.txtSurveyID.Name = "txtSurveyID";
            this.txtSurveyID.TextChanged += new System.EventHandler(this.txtSurveyID_TextChanged);
            this.txtSurveyID.Enter += new System.EventHandler(this.txtSurveyID_Enter);
            this.txtSurveyID.Leave += new System.EventHandler(this.txtSurveyID_Leave);
            // 
            // txtSurveyName
            // 
            resources.ApplyResources(this.txtSurveyName, "txtSurveyName");
            this.txtSurveyName.Name = "txtSurveyName";
            this.txtSurveyName.TextChanged += new System.EventHandler(this.txtSurveyName_TextChanged);
            this.txtSurveyName.Enter += new System.EventHandler(this.txtSurveyName_Enter);
            this.txtSurveyName.Leave += new System.EventHandler(this.txtSurveyName_Leave);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPrevious
            // 
            resources.ApplyResources(this.btnPrevious, "btnPrevious");
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // tabPublishWebForm
            // 
            this.tabPublishWebForm.Controls.Add(this.tabIntro);
            this.tabPublishWebForm.Controls.Add(this.tabExit);
            this.tabPublishWebForm.Controls.Add(this.tabEntry);
            resources.ApplyResources(this.tabPublishWebForm, "tabPublishWebForm");
            this.tabPublishWebForm.Name = "tabPublishWebForm";
            this.tabPublishWebForm.SelectedIndex = 0;
            this.tabPublishWebForm.TabStop = false;
            this.tabPublishWebForm.SelectedIndexChanged += new System.EventHandler(this.tabPublishWebForm_SelectedIndexChanged);
            // 
            // tabIntro
            // 
            resources.ApplyResources(this.tabIntro, "tabIntro");
            this.tabIntro.CausesValidation = false;
            this.tabIntro.Controls.Add(this.ClosingTimecomboBox);
            this.tabIntro.Controls.Add(this.closingTimelabel);
            this.tabIntro.Controls.Add(this.StartTimelabel);
            this.tabIntro.Controls.Add(this.StartTimecomboBox);
            this.tabIntro.Controls.Add(this.StartDateDatePicker);
            this.tabIntro.Controls.Add(this.lblSurveyStartDate);
            this.tabIntro.Controls.Add(this.lblWelcome);
            this.tabIntro.Controls.Add(this.pnlSurveyName);
            this.tabIntro.Controls.Add(this.lblClosingDate);
            this.tabIntro.Controls.Add(this.txtIntroductionText);
            this.tabIntro.Controls.Add(this.dtpSurveyClosingDate);
            this.tabIntro.Controls.Add(this.pnlSurvNumOrgDept);
            this.tabIntro.Name = "tabIntro";
            this.tabIntro.UseVisualStyleBackColor = true;
            this.tabIntro.Click += new System.EventHandler(this.tabIntro_Click);
            // 
            // ClosingTimecomboBox
            // 
            this.ClosingTimecomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ClosingTimecomboBox, "ClosingTimecomboBox");
            this.ClosingTimecomboBox.Name = "ClosingTimecomboBox";
            // 
            // closingTimelabel
            // 
            resources.ApplyResources(this.closingTimelabel, "closingTimelabel");
            this.closingTimelabel.Name = "closingTimelabel";
            // 
            // StartTimelabel
            // 
            resources.ApplyResources(this.StartTimelabel, "StartTimelabel");
            this.StartTimelabel.Name = "StartTimelabel";
            // 
            // StartTimecomboBox
            // 
            this.StartTimecomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.StartTimecomboBox, "StartTimecomboBox");
            this.StartTimecomboBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.StartTimecomboBox.Name = "StartTimecomboBox";
            // 
            // StartDateDatePicker
            // 
            resources.ApplyResources(this.StartDateDatePicker, "StartDateDatePicker");
            this.StartDateDatePicker.Name = "StartDateDatePicker";
            // 
            // lblSurveyStartDate
            // 
            resources.ApplyResources(this.lblSurveyStartDate, "lblSurveyStartDate");
            this.lblSurveyStartDate.Name = "lblSurveyStartDate";
            // 
            // lblWelcome
            // 
            resources.ApplyResources(this.lblWelcome, "lblWelcome");
            this.lblWelcome.Name = "lblWelcome";
            // 
            // pnlSurveyName
            // 
            this.pnlSurveyName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(231)))), ((int)(((byte)(234)))));
            this.pnlSurveyName.Controls.Add(this.txtSurveyName);
            resources.ApplyResources(this.pnlSurveyName, "pnlSurveyName");
            this.pnlSurveyName.Name = "pnlSurveyName";
            // 
            // lblClosingDate
            // 
            resources.ApplyResources(this.lblClosingDate, "lblClosingDate");
            this.lblClosingDate.Name = "lblClosingDate";
            // 
            // pnlSurvNumOrgDept
            // 
            this.pnlSurvNumOrgDept.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(214)))), ((int)(((byte)(155)))));
            this.pnlSurvNumOrgDept.Controls.Add(this.lblOrganization);
            this.pnlSurvNumOrgDept.Controls.Add(this.lblSurvNum);
            this.pnlSurvNumOrgDept.Controls.Add(this.lblDepartment);
            this.pnlSurvNumOrgDept.Controls.Add(this.txtOrganization);
            this.pnlSurvNumOrgDept.Controls.Add(this.txtDepartment);
            this.pnlSurvNumOrgDept.Controls.Add(this.txtSurveyID);
            resources.ApplyResources(this.pnlSurvNumOrgDept, "pnlSurvNumOrgDept");
            this.pnlSurvNumOrgDept.Name = "pnlSurvNumOrgDept";
            // 
            // lblOrganization
            // 
            resources.ApplyResources(this.lblOrganization, "lblOrganization");
            this.lblOrganization.Name = "lblOrganization";
            // 
            // lblSurvNum
            // 
            resources.ApplyResources(this.lblSurvNum, "lblSurvNum");
            this.lblSurvNum.Name = "lblSurvNum";
            // 
            // lblDepartment
            // 
            resources.ApplyResources(this.lblDepartment, "lblDepartment");
            this.lblDepartment.Name = "lblDepartment";
            // 
            // tabExit
            // 
            this.tabExit.Controls.Add(this.rdbSingleResponse);
            this.tabExit.Controls.Add(this.lblSurveyType);
            this.tabExit.Controls.Add(this.rdbMultipleResponse);
            this.tabExit.Controls.Add(this.grpExitText);
            this.tabExit.Controls.Add(this.panel1);
            this.tabExit.Controls.Add(this.pnlSurveyNameMirror);
            resources.ApplyResources(this.tabExit, "tabExit");
            this.tabExit.Name = "tabExit";
            this.tabExit.UseVisualStyleBackColor = true;
            this.tabExit.Click += new System.EventHandler(this.tabExit_Click);
            // 
            // rdbSingleResponse
            // 
            this.rdbSingleResponse.Checked = true;
            resources.ApplyResources(this.rdbSingleResponse, "rdbSingleResponse");
            this.rdbSingleResponse.Name = "rdbSingleResponse";
            this.rdbSingleResponse.TabStop = true;
            this.rdbSingleResponse.UseVisualStyleBackColor = true;
            // 
            // lblSurveyType
            // 
            resources.ApplyResources(this.lblSurveyType, "lblSurveyType");
            this.lblSurveyType.Name = "lblSurveyType";
            // 
            // rdbMultipleResponse
            // 
            resources.ApplyResources(this.rdbMultipleResponse, "rdbMultipleResponse");
            this.rdbMultipleResponse.Name = "rdbMultipleResponse";
            this.rdbMultipleResponse.TabStop = true;
            this.rdbMultipleResponse.UseVisualStyleBackColor = true;
            // 
            // grpExitText
            // 
            this.grpExitText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))));
            this.grpExitText.Controls.Add(this.picExitCheckmark);
            this.grpExitText.Controls.Add(this.txtExitText);
            resources.ApplyResources(this.grpExitText, "grpExitText");
            this.grpExitText.Name = "grpExitText";
            this.grpExitText.TabStop = false;
            this.grpExitText.Enter += new System.EventHandler(this.grpExitText_Enter);
            // 
            // picExitCheckmark
            // 
            this.picExitCheckmark.BackgroundImage = global::Epi.Windows.MakeView.Properties.Resources.ExitCheckmark;
            resources.ApplyResources(this.picExitCheckmark, "picExitCheckmark");
            this.picExitCheckmark.Name = "picExitCheckmark";
            this.picExitCheckmark.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(214)))), ((int)(((byte)(155)))));
            this.panel1.Controls.Add(this.lblDepMirr);
            this.panel1.Controls.Add(this.lblOrgMirr);
            this.panel1.Controls.Add(this.lblSurvNumMirr);
            this.panel1.Controls.Add(this.txtOrganizationMirror);
            this.panel1.Controls.Add(this.txtSurveyIDMirror);
            this.panel1.Controls.Add(this.txtDepartmentMirror);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lblDepMirr
            // 
            resources.ApplyResources(this.lblDepMirr, "lblDepMirr");
            this.lblDepMirr.Name = "lblDepMirr";
            // 
            // lblOrgMirr
            // 
            resources.ApplyResources(this.lblOrgMirr, "lblOrgMirr");
            this.lblOrgMirr.Name = "lblOrgMirr";
            // 
            // lblSurvNumMirr
            // 
            resources.ApplyResources(this.lblSurvNumMirr, "lblSurvNumMirr");
            this.lblSurvNumMirr.Name = "lblSurvNumMirr";
            // 
            // txtOrganizationMirror
            // 
            resources.ApplyResources(this.txtOrganizationMirror, "txtOrganizationMirror");
            this.txtOrganizationMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtOrganizationMirror.Name = "txtOrganizationMirror";
            this.txtOrganizationMirror.ReadOnly = true;
            this.txtOrganizationMirror.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtOrganizationMirror_MouseClick);
            // 
            // txtSurveyIDMirror
            // 
            resources.ApplyResources(this.txtSurveyIDMirror, "txtSurveyIDMirror");
            this.txtSurveyIDMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtSurveyIDMirror.Name = "txtSurveyIDMirror";
            this.txtSurveyIDMirror.ReadOnly = true;
            this.txtSurveyIDMirror.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtSurveyIDMirror_MouseClick);
            // 
            // txtDepartmentMirror
            // 
            resources.ApplyResources(this.txtDepartmentMirror, "txtDepartmentMirror");
            this.txtDepartmentMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtDepartmentMirror.Name = "txtDepartmentMirror";
            this.txtDepartmentMirror.ReadOnly = true;
            this.txtDepartmentMirror.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtDepartmentMirror_MouseClick);
            // 
            // pnlSurveyNameMirror
            // 
            this.pnlSurveyNameMirror.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(231)))), ((int)(((byte)(234)))));
            this.pnlSurveyNameMirror.Controls.Add(this.txtSurveyNameMirror);
            resources.ApplyResources(this.pnlSurveyNameMirror, "pnlSurveyNameMirror");
            this.pnlSurveyNameMirror.Name = "pnlSurveyNameMirror";
            // 
            // txtSurveyNameMirror
            // 
            resources.ApplyResources(this.txtSurveyNameMirror, "txtSurveyNameMirror");
            this.txtSurveyNameMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtSurveyNameMirror.Name = "txtSurveyNameMirror";
            this.txtSurveyNameMirror.ReadOnly = true;
            this.txtSurveyNameMirror.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtSurveyNameMirror_MouseClick);
            // 
            // tabEntry
            // 
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
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // txtOrganizationKey
            // 
            resources.ApplyResources(this.txtOrganizationKey, "txtOrganizationKey");
            this.txtOrganizationKey.Name = "txtOrganizationKey";
            this.txtOrganizationKey.TextChanged += new System.EventHandler(this.txtOrganizationKey_TextChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.txtStatus);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.btnDetails);
            this.panel3.Controls.Add(this.btnShowLog);
            this.panel3.Controls.Add(this.lblSuccessNotice2);
            this.panel3.Controls.Add(this.lblSuccessNotice);
            this.panel3.Name = "panel3";
            // 
            // btnDetails
            // 
            resources.ApplyResources(this.btnDetails, "btnDetails");
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnShowLog
            // 
            resources.ApplyResources(this.btnShowLog, "btnShowLog");
            this.btnShowLog.Name = "btnShowLog";
            this.ttWebPubDialog.SetToolTip(this.btnShowLog, resources.GetString("btnShowLog.ToolTip"));
            this.btnShowLog.UseVisualStyleBackColor = true;
            this.btnShowLog.Click += new System.EventHandler(this.btnShowLog_Click);
            // 
            // lblSuccessNotice2
            // 
            resources.ApplyResources(this.lblSuccessNotice2, "lblSuccessNotice2");
            this.lblSuccessNotice2.Name = "lblSuccessNotice2";
            // 
            // lblSuccessNotice
            // 
            this.lblSuccessNotice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))));
            resources.ApplyResources(this.lblSuccessNotice, "lblSuccessNotice");
            this.lblSuccessNotice.Name = "lblSuccessNotice";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.txtURL);
            this.panel2.Controls.Add(this.btnDataKeyCopy);
            this.panel2.Controls.Add(this.btnKeyCopy);
            this.panel2.Controls.Add(this.txtStatusSummary);
            this.panel2.Controls.Add(this.lblSecurityToken);
            this.panel2.Controls.Add(this.lblSurveyKey);
            this.panel2.Controls.Add(this.txtDataKey);
            this.panel2.Controls.Add(this.txtSurveyKey);
            this.panel2.Controls.Add(this.btnCopyAllURLs);
            this.panel2.Controls.Add(this.btnURLCopy);
            this.panel2.Controls.Add(this.btnGo);
            this.panel2.Controls.Add(this.lblStatus);
            this.panel2.Controls.Add(this.lblURL);
            this.panel2.Name = "panel2";
            // 
            // txtURL
            // 
            resources.ApplyResources(this.txtURL, "txtURL");
            this.txtURL.Name = "txtURL";
            this.txtURL.ReadOnly = true;
            // 
            // btnDataKeyCopy
            // 
            resources.ApplyResources(this.btnDataKeyCopy, "btnDataKeyCopy");
            this.btnDataKeyCopy.Name = "btnDataKeyCopy";
            this.btnDataKeyCopy.UseVisualStyleBackColor = true;
            this.btnDataKeyCopy.Click += new System.EventHandler(this.btnDataKeyCopy_Click);
            // 
            // btnKeyCopy
            // 
            resources.ApplyResources(this.btnKeyCopy, "btnKeyCopy");
            this.btnKeyCopy.Name = "btnKeyCopy";
            this.btnKeyCopy.UseVisualStyleBackColor = true;
            this.btnKeyCopy.Click += new System.EventHandler(this.btnKeyCopy_Click);
            // 
            // txtStatusSummary
            // 
            resources.ApplyResources(this.txtStatusSummary, "txtStatusSummary");
            this.txtStatusSummary.Name = "txtStatusSummary";
            this.txtStatusSummary.ReadOnly = true;
            // 
            // lblSecurityToken
            // 
            resources.ApplyResources(this.lblSecurityToken, "lblSecurityToken");
            this.lblSecurityToken.Name = "lblSecurityToken";
            // 
            // lblSurveyKey
            // 
            resources.ApplyResources(this.lblSurveyKey, "lblSurveyKey");
            this.lblSurveyKey.Name = "lblSurveyKey";
            // 
            // txtDataKey
            // 
            resources.ApplyResources(this.txtDataKey, "txtDataKey");
            this.txtDataKey.Name = "txtDataKey";
            this.txtDataKey.ReadOnly = true;
            // 
            // txtSurveyKey
            // 
            resources.ApplyResources(this.txtSurveyKey, "txtSurveyKey");
            this.txtSurveyKey.Name = "txtSurveyKey";
            this.txtSurveyKey.ReadOnly = true;
            // 
            // btnCopyAllURLs
            // 
            resources.ApplyResources(this.btnCopyAllURLs, "btnCopyAllURLs");
            this.btnCopyAllURLs.Name = "btnCopyAllURLs";
            this.btnCopyAllURLs.UseVisualStyleBackColor = true;
            this.btnCopyAllURLs.Click += new System.EventHandler(this.btnCopyAllURLs_Click);
            // 
            // btnURLCopy
            // 
            resources.ApplyResources(this.btnURLCopy, "btnURLCopy");
            this.btnURLCopy.Name = "btnURLCopy";
            this.btnURLCopy.UseVisualStyleBackColor = true;
            this.btnURLCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnGo
            // 
            resources.ApplyResources(this.btnGo, "btnGo");
            this.btnGo.Name = "btnGo";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // lblURL
            // 
            resources.ApplyResources(this.lblURL, "lblURL");
            this.lblURL.Name = "lblURL";
            // 
            // txtStatus
            // 
            resources.ApplyResources(this.txtStatus, "txtStatus");
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            // 
            // lblPublishModeStatus
            // 
            this.lblPublishModeStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lblPublishModeStatus, "lblPublishModeStatus");
            this.lblPublishModeStatus.Name = "lblPublishModeStatus";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnPublishForm
            // 
            resources.ApplyResources(this.btnPublishForm, "btnPublishForm");
            this.btnPublishForm.Name = "btnPublishForm";
            this.btnPublishForm.UseVisualStyleBackColor = true;
            this.btnPublishForm.Click += new System.EventHandler(this.btnPublishForm_Click);
            // 
            // btnNext
            // 
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // lblHeading1
            // 
            resources.ApplyResources(this.lblHeading1, "lblHeading1");
            this.lblHeading1.Name = "lblHeading1";
            // 
            // lblHeading2
            // 
            resources.ApplyResources(this.lblHeading2, "lblHeading2");
            this.lblHeading2.Name = "lblHeading2";
            // 
            // WebSurveyOptionsLinkLabel
            // 
            resources.ApplyResources(this.WebSurveyOptionsLinkLabel, "WebSurveyOptionsLinkLabel");
            this.WebSurveyOptionsLinkLabel.BackColor = System.Drawing.Color.Transparent;
            this.WebSurveyOptionsLinkLabel.Name = "WebSurveyOptionsLinkLabel";
            this.WebSurveyOptionsLinkLabel.TabStop = true;
            this.WebSurveyOptionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // WebPublishDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WebSurveyOptionsLinkLabel);
            this.Controls.Add(this.lblHeading2);
            this.Controls.Add(this.lblHeading1);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tabPublishWebForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebPublishDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WebPublishDialog_FormClosing);
            this.Load += new System.EventHandler(this.WebPublishDialog_Load);
            this.tabPublishWebForm.ResumeLayout(false);
            this.tabIntro.ResumeLayout(false);
            this.tabIntro.PerformLayout();
            this.pnlSurveyName.ResumeLayout(false);
            this.pnlSurveyName.PerformLayout();
            this.pnlSurvNumOrgDept.ResumeLayout(false);
            this.pnlSurvNumOrgDept.PerformLayout();
            this.tabExit.ResumeLayout(false);
            this.grpExitText.ResumeLayout(false);
            this.grpExitText.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picExitCheckmark)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlSurveyNameMirror.ResumeLayout(false);
            this.pnlSurveyNameMirror.PerformLayout();
            this.tabEntry.ResumeLayout(false);
            this.tabEntry.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtpSurveyClosingDate;
        private System.Windows.Forms.TextBox txtDepartment;
        private System.Windows.Forms.TextBox txtIntroductionText;
        private System.Windows.Forms.TextBox txtExitText;
        private System.Windows.Forms.TextBox txtOrganization;
        private System.Windows.Forms.TextBox txtSurveyID;
        private System.Windows.Forms.TextBox txtSurveyName;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.TabControl tabPublishWebForm;
        private System.Windows.Forms.TabPage tabIntro;
        private System.Windows.Forms.TabPage tabEntry;
        private System.Windows.Forms.TabPage tabExit;
        private System.Windows.Forms.ToolTip ttWebPubDialog;
        private System.Windows.Forms.TextBox txtOrganizationMirror;
        private System.Windows.Forms.TextBox txtSurveyIDMirror;
        private System.Windows.Forms.TextBox txtDepartmentMirror;
        private System.Windows.Forms.TextBox txtSurveyNameMirror;
        private System.Windows.Forms.Label lblClosingDate;
        private System.Windows.Forms.Button btnPublishForm;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lblHeading1;
        private System.Windows.Forms.Label lblHeading2;
        private System.Windows.Forms.Panel pnlSurveyName;
        private System.Windows.Forms.Panel pnlSurvNumOrgDept;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblDepartment;
        private System.Windows.Forms.Label lblOrganization;
        private System.Windows.Forms.Label lblSurvNum;
        private System.Windows.Forms.GroupBox grpExitText;
        private System.Windows.Forms.Panel pnlSurveyNameMirror;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblDepMirr;
        private System.Windows.Forms.Label lblOrgMirr;
        private System.Windows.Forms.Label lblSurvNumMirr;
        private System.Windows.Forms.PictureBox picExitCheckmark;
        private System.Windows.Forms.DateTimePicker StartDateDatePicker;
        private System.Windows.Forms.Label lblSurveyStartDate;
        private System.Windows.Forms.RadioButton rdbSingleResponse;
        private System.Windows.Forms.Label lblSurveyType;
        private System.Windows.Forms.RadioButton rdbMultipleResponse;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblSuccessNotice2;
        private System.Windows.Forms.Label lblSuccessNotice;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnDataKeyCopy;
        private System.Windows.Forms.Button btnKeyCopy;
        private System.Windows.Forms.Button btnShowLog;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Label lblSecurityToken;
        private System.Windows.Forms.Label lblSurveyKey;
        private System.Windows.Forms.TextBox txtDataKey;
        private System.Windows.Forms.TextBox txtSurveyKey;
        private System.Windows.Forms.Button btnCopyAllURLs;
        private System.Windows.Forms.Button btnURLCopy;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.Label lblPublishModeStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel WebSurveyOptionsLinkLabel;
        private System.Windows.Forms.TextBox txtOrganizationKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStatusSummary;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label StartTimelabel;
        private System.Windows.Forms.ComboBox StartTimecomboBox;
        private System.Windows.Forms.ComboBox ClosingTimecomboBox;
        private System.Windows.Forms.Label closingTimelabel;
    }
}