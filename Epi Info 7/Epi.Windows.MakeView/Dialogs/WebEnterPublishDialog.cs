using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Security;
using Epi.Web.Common.Exception;
using Epi.Web.Common.Message;
using Epi.Windows.MakeView.PresentationLogic;
using Epi.Web.Enter.Common;
namespace Epi.Windows.MakeView.Dialogs
{
    public partial class WebEnterPublishDialog : Form
    {
        #region Private Members
        private string template;
        private BackgroundWorker publishWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool publishFinished = false;
        private bool boolError = false;
        private Guid UserPublishGuid;
        private View view = null;
        private GuiMediator mediater;
        private string OrganizationKey = null;
        private bool IsMetaDataOnly = false;
        private bool isRepublishableConfig = false;
        private Dictionary<int, string> _DataAccessRuleIds;
        //private Epi.Web.Enter.Common.DTO.SurveyInfoDTO currentSurveyInfoDTO;
        private Epi.EWEManagerService.SurveyInfoDTO currentSurveyInfoDTO = new EWEManagerService.SurveyInfoDTO();
        //=======
        //private Epi.Web.Common.Message.PublishResponse Result;
        //=======
        #endregion //Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
      //  private delegate void PublishDelegate(Epi.Web.Common.Message.PublishResponse Result);
        private delegate void PublishDelegate(Epi.EWEManagerService.PublishResponse Result);
        private delegate void FinishWithErrorDelegate(string errorMessage);
        private delegate void FinishWithCustomFaultExceptionDelegate(FaultException<CustomFaultException> cfe);
        private delegate void FinishWithFaultExceptionDelegate(FaultException fe);
        private delegate void FinishWithSecurityNegotiationExceptionDelegate(SecurityNegotiationException sne);
        private delegate void FinishWithCommunicationExceptionDelegate(CommunicationException ce);
        private delegate void FinishWithTimeoutExceptionDelegate(TimeoutException te);
        private delegate void FinishWithExceptionDelegate(Exception ex);
        #endregion //Delegates

        #region Constructors

        public WebEnterPublishDialog()
        {
            InitializeComponent();
        }
        public string GetOrgKey
            {
            get { return this.OrganizationKey; }
            }
        public WebEnterPublishDialog(string pOrganizationKey, Epi.View pView, string pTemplate, bool pIsMetaDataOnly = false)
        {
            InitializeComponent();
            this.view = pView;
            this.template = pTemplate;
            this.OrganizationKey = pOrganizationKey;
            this.IsMetaDataOnly = pIsMetaDataOnly;

             Configuration config = Configuration.GetNewInstance();
             try
             {
                 this.isRepublishableConfig = config.Settings.Republish_IsRepbulishable;
             }
             catch (Exception ex)
             {
                 this.isRepublishableConfig = false;
             }


            Construct();
        }

        public WebEnterPublishDialog(string pOrganizationKey, GuiMediator pMediator, string pTemplate, bool pIsMetaDataOnly = false)
            {
            InitializeComponent();
            this.mediater = pMediator;
            this.template = pTemplate;
            this.OrganizationKey = pOrganizationKey;
            this.IsMetaDataOnly = pIsMetaDataOnly;

            Configuration config = Configuration.GetNewInstance();
            
            DataTable table = mediater.Project.Metadata.GetPublishedViewKeys(this.mediater.ProjectExplorer.CurrentView.Id);
            DataRow ViewRow = table.Rows[0];

            string WebSurveyId = ViewRow.ItemArray[3].ToString();
            
            var Request = new Epi.EWEManagerService.SurveyInfoRequest();

            EWEManagerService.SurveyInfoCriteria Criteria  = new EWEManagerService.SurveyInfoCriteria();
           
             List<string> IdList = new List<string>();
              IdList.Add(WebSurveyId);
               Criteria.SurveyIdList = IdList.ToArray();
               //if (!string.IsNullOrEmpty(pOrganizationKey))
               //{
               //Criteria.OrganizationKey = new Guid(pOrganizationKey);
               //}
               Criteria.SurveyType = -1;
               Request.Criteria = Criteria;
              
            try
            {
            EWEManagerService.EWEManagerServiceClient client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();

               var Result = client.GetSurveyInfo(Request);
                EWEManagerService.FormSettingRequest  FormSettingRequest = new EWEManagerService.FormSettingRequest ();
                FormSettingRequest.FormInfo = new EWEManagerService.FormInfoDTO ();
                FormSettingRequest.FormInfo.FormId = WebSurveyId;
                FormSettingRequest.FormInfo.UserId = LoginInfo.UserID;
                //Getting Column Name  List
                EWEManagerService.FormSettingResponse FormSettingResponse = client.GetSettings(FormSettingRequest);
                var Tooltip = "";
                foreach (var item in FormSettingResponse.FormSetting.DataAccessRuleDescription)
                {
                    Tooltip = Tooltip + item.Key.ToString() + " : " + item.Value + "\n";
                }


                if (FormSettingResponse.FormSetting.DataAccessRuleIds.Count() > 0)
                {
                    _DataAccessRuleIds = FormSettingResponse.FormSetting.DataAccessRuleIds;
                    if (Result.SurveyInfoList.Count() > 0)
                    {
                        if (Result.SurveyInfoList[0].IsShareable)
                        {
                            var SelectedVal = "";
                            this.DataAccessRuleList.Visible = true;
                            InfoToolTip.SetToolTip(this.DataAccessRuleList, Tooltip);
                            this.Shareable.Checked = Result.SurveyInfoList[0].IsShareable;
                            foreach (var item in FormSettingResponse.FormSetting.DataAccessRuleIds)
                            {
                                this.DataAccessRuleList.Items.Add(item.Value);
                                if (item.Key == FormSettingResponse.FormSetting.SelectedDataAccessRule)
                                {
                                    SelectedVal = item.Value;
                                }
                            }
                            this.DataAccessRuleList.SelectedItem = SelectedVal;
                        }
                        else
                        {
                            this.Shareable.Checked = Result.SurveyInfoList[0].IsShareable;
                            this.DataAccessRuleList.Visible = false;
                            //this.DataAccessRuleList.SelectedValue = "";
                            foreach (var item in FormSettingResponse.FormSetting.DataAccessRuleIds)
                            {
                                this.DataAccessRuleList.Items.Add(item.Value);

                            }
                        }
                    }
                    else {

                        this.Shareable.Checked = false;
                        this.DataAccessRuleList.Visible = false;
                        foreach (var item in FormSettingResponse.FormSetting.DataAccessRuleIds)
                        {
                            this.DataAccessRuleList.Items.Add(item.Value);
                             
                        }
                        
                    
                    }
                }
               
            }
            catch (Exception ex)
            {
               
            }
          

            try
                {
                this.isRepublishableConfig = config.Settings.Republish_IsRepbulishable;
                }
            catch (Exception ex)
                {
                this.isRepublishableConfig = false;
                }


            Construct();
            }
        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Construct method
        /// </summary>
        private void Construct()
        {
            this.publishWorker = new BackgroundWorker();
            this.publishWorker.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// Initiates a single form publishing process
        /// </summary>
        private void DoPublish()
        {

            UserPublishGuid = Guid.NewGuid();
           
            txtOrganizationKey.Enabled = false;
         //   btnPrevious.Enabled = false;
            txtStatus.Clear();
            txtURL.Clear();
          //  this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[2];

            TimeSpan tClose = new TimeSpan(0, 23, 59, 59);
          
            btnPublishForm.Enabled = true;

            
            progressBar.Visible = true;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            EWEManagerService.PublishRequest Request = new Epi.EWEManagerService.PublishRequest();
            EWEManagerService.SurveyInfoDTO SurveyInfoDTO = new EWEManagerService.SurveyInfoDTO();
            Request.SurveyInfo = SurveyInfoDTO;
             
           //Request.SurveyInfo.ClosingDate = dtpSurveyClosingDate.Value.Date + t;
            //if (string.IsNullOrEmpty(ClosingTimecomboBox.Text))
            //    {
            //    Request.SurveyInfo.ClosingDate = dtpSurveyClosingDate.Value.Date + GetTimeFormat(tClose.ToString());
            //    }
            //else {
            //    // Request.SurveyInfo.ClosingDate = dtpSurveyClosingDate.Value.Date + GetTimeFormat(ClosingTimecomboBox.Text);
            //       Request.SurveyInfo.ClosingDate = GetdateTimeFormat(dtpSurveyClosingDate.Value.Date, ClosingTimecomboBox.Text); 
            //    }
           
            Request.SurveyInfo.DBConnectionString = RemoveUserName(this.mediater.Project.CollectedDataConnectionString);
               
            if (this.mediater.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
            {
                Request.SurveyInfo.IsSqlProject = true;
            }
        
            Request.SurveyInfo.OrganizationKey = new Guid(txtOrganizationKey.Text.ToString());
            Request.SurveyInfo.UserPublishKey = UserPublishGuid;
            Request.SurveyInfo.XML = template;
            Request.SurveyInfo.IsDraftMode = true;
            Request.SurveyInfo.SurveyType = 2;
            Request.SurveyInfo.SurveyName = this.mediater.Project.Name;
            Request.SurveyInfo.ViewId = this.mediater.ProjectExplorer.CurrentView.Id;
            Request.SurveyInfo.OwnerId = LoginInfo.UserID;
            Request.SurveyInfo.StartDate = DateTime.Now;
            Request.SurveyInfo.IsShareable = this.Shareable.Checked;
            //Request.SurveyInfo.ShowAllRecords = this.AccessAll.Checked;
            if (this.DataAccessRuleList.SelectedItem != null)
           {
            if (_DataAccessRuleIds.Count() > 0 && this.DataAccessRuleList.SelectedItem != null)
                {
                    foreach (var Rule in _DataAccessRuleIds)
                    {
                        if (this.DataAccessRuleList.SelectedItem.ToString() == Rule.Value)
                        {
                            Request.SurveyInfo.DataAccessRuleId = Rule.Key;

                        }

                    }
                }
                else
                {
                    Request.SurveyInfo.DataAccessRuleId = 1;

                }
            }
            //else {

            //   Request.SurveyInfo.DataAccessRuleId = 1;
            
            // }
            
            //Request.SurveyInfo.SurveyType = (rdbSingleResponse.Checked) ? 1 : 2;
            //if (txtOrganization.Text.Equals("Your Organization Name (optional)", StringComparison.OrdinalIgnoreCase))
            //{
            //    Request.SurveyInfo.OrganizationName = null;
            //}
            //else
            //{
            //    Request.SurveyInfo.OrganizationName = txtOrganization.Text;
            //}

            //if (txtSurveyID.Text.Equals("Your Survey ID (optional)", StringComparison.OrdinalIgnoreCase))
            //{
            //    Request.SurveyInfo.SurveyNumber = null;
            //}
            //else
            //{
            //    Request.SurveyInfo.SurveyNumber = txtSurveyID.Text;
            //}


            Configuration config = Configuration.GetNewInstance();
            try
            {
                if (config.Settings.Republish_IsRepbulishable)
                {
                    Request.SurveyInfo.IsDraftMode = true;
                }
                else
                {
                    Request.SurveyInfo.IsDraftMode = false;
                }

            }
            catch (Exception ex)
            {
                Request.SurveyInfo.IsDraftMode = false;
            }
            try
            {
                Epi.EWEManagerService.PublishResponse Result = new Epi.EWEManagerService.PublishResponse();

                lock (syncLock)
                {
                    this.Cursor = Cursors.WaitCursor;
                    publishWorker = new BackgroundWorker();
                    publishWorker.WorkerSupportsCancellation = true;
                    publishWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                    publishWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    object[] args = new object[2];
                    args[0] = Request;
                    args[1] = Result;
                    publishWorker.RunWorkerAsync(args);
                }

                if (publishWorker.WorkerSupportsCancellation)
                {
                    publishWorker.CancelAsync();
                }
            }
            catch (Exception ex)
            {
                txtStatusSummary.AppendText("An error occurred while trying to publish the survey.");
                txtStatus.AppendText(ex.ToString());
                btnDetails.Visible = true;
                this.progressBar.Visible = false;
                this.Cursor = Cursors.Default;
            }
        }

        private string RemoveUserName(string ConnectionString)
        {
            int indexOfUserId = ConnectionString.IndexOf("User ID");

            if (indexOfUserId > 0)
            {
            ConnectionString = ConnectionString.Remove(indexOfUserId - 1);
            }

            return ConnectionString;
        }

        /// <summary>
        /// Initiates a single form publishing process
        /// </summary>
        private void DoRePublish()
        {
            //txtSurveyName.Enabled = false;
            //txtSurveyID.Enabled = false;
            //txtOrganization.Enabled = false;
            //txtDepartment.Enabled = false;
            //txtIntroductionText.Enabled = false;
            //txtExitText.Enabled = false;
            //dtpSurveyClosingDate.Enabled = false;
            txtOrganizationKey.Enabled = false;
            //btnPrevious.Enabled = false;
            txtStatus.Clear();
            txtURL.Clear();
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];

            btnPublishForm.Enabled = false;


            progressBar.Visible = true;

            //stopwatch = new Stopwatch();
            //stopwatch.Start();

            //SurveyManagerService.ManagerServiceClient client = new SurveyManagerService.ManagerServiceClient();

           // Epi.Web.Enter.Common.Message.PublishRequest Request = new Epi.Web.Enter.Common.Message.PublishRequest();
            var Request = new Epi.EWEManagerService.PublishRequest();
            Request.Action = "Update";

          
            //this.currentSurveyInfoDTO.ClosingDate =  dtpSurveyClosingDate.Value.Date + GetTimeFormat(ClosingTimecomboBox.Text);
             
            // this.currentSurveyInfoDTO.ClosingDate = GetdateTimeFormat( dtpSurveyClosingDate.Value.Date ,ClosingTimecomboBox.Text); 

            ////this.currentSurveyInfoDTO.StartDate = StartDateDatePicker.Value.Date + GetTimeFormat(StartTimecomboBox.Text);
            //this.currentSurveyInfoDTO.StartDate = GetdateTimeFormat(StartDateDatePicker.Value.Date,StartTimecomboBox.Text);
            //this.currentSurveyInfoDTO.DepartmentName = txtDepartment.Text;
            
            ////this.currentSurveyInfoDTO.DepartmentName = txtDepartment.Text;
            //this.currentSurveyInfoDTO.IntroductionText = txtIntroductionText.Text;
            //this.currentSurveyInfoDTO.ExitText = txtExitText.Text;
            //if (txtOrganization.Text.Equals("Your Organization Name (optional)", StringComparison.OrdinalIgnoreCase))
            //{
            //    this.currentSurveyInfoDTO.OrganizationName = null;
            //}
            //else
            //{
            //    this.currentSurveyInfoDTO.OrganizationName = txtOrganization.Text;
            //}

            //if (txtSurveyID.Text.Equals("Your Survey ID (optional)", StringComparison.OrdinalIgnoreCase))
            //{
            //    this.currentSurveyInfoDTO.SurveyNumber = null;
            //}
            //else
            //{
            //    this.currentSurveyInfoDTO.SurveyNumber = txtSurveyID.Text;
            //}


            //this.currentSurveyInfoDTO.SurveyName = txtSurveyName.Text;
            
            this.currentSurveyInfoDTO.OrganizationKey = new Guid(txtOrganizationKey.Text.ToString());
            //this.currentSurveyInfoDTO.UserPublishKey = UserPublishGuid;
            if (!this.IsMetaDataOnly)
            {
                this.currentSurveyInfoDTO.XML = this.template;
            }
           // this.currentSurveyInfoDTO.SurveyType = (rdbSingleResponse.Checked) ? 1 : 2;
            DataTable table;
            View RootView = this.mediater.Project.Metadata.GetParentView(this.mediater.ProjectExplorer.CurrentView.Id);
            if (RootView == null)
                {
                table = this.mediater.Project.Metadata.GetPublishedViewKeys(this.mediater.ProjectExplorer.CurrentView.Id);
                }
            else
                {
                table = this.mediater.Project.Metadata.GetPublishedViewKeys(RootView.Id);
                }


            DataRow ViewRow = table.Rows[0];
 

            this.currentSurveyInfoDTO.SurveyId = ViewRow.ItemArray[3].ToString();
          

            this.currentSurveyInfoDTO.OwnerId = LoginInfo.UserID;
            this.currentSurveyInfoDTO.StartDate = DateTime.Now;
            this.currentSurveyInfoDTO.SurveyName = this.mediater.Project.Name;
            this.currentSurveyInfoDTO.DBConnectionString = RemoveUserName(this.mediater.Project.CollectedDataConnectionString);
            if (this.mediater.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
                {
                this.currentSurveyInfoDTO.IsSqlProject = true;
                }
              Request.SurveyInfo  = this.currentSurveyInfoDTO;
              Request.SurveyInfo.IsShareable = this.Shareable.Checked;
              if (_DataAccessRuleIds.Count() > 0)
              {
                  foreach (var Rule in _DataAccessRuleIds)
                  {
                      if (this.DataAccessRuleList.SelectedItem != null)
                      {
                          if (this.DataAccessRuleList.SelectedItem.ToString() == Rule.Value)
                          {
                              Request.SurveyInfo.DataAccessRuleId = Rule.Key;

                          }
                      }
                  }
              }
              else
              {
                  Request.SurveyInfo.DataAccessRuleId = 1;

              }
            try
            {
            EWEManagerService.EWEManagerServiceClient client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
          //  Epi.Web.Enter.Common.Message.PublishResponse Result = client.RePublishSurvey(Request);
                //Epi.Web.Enter.Common.Message.PublishResponse Result = client.RePublishSurvey(Request);
               var Result = client.RePublishSurvey(Request);
                panel2.Visible = true;
                panel3.Visible = true;

                if (Result.PublishInfo.IsPulished)
                {
                    panel3.Visible = true;

                    txtURL.Text = Result.PublishInfo.URL;
                    lblSuccessNotice.Visible = true;
                    lblSuccessNotice2.Visible = true;
                    Importantlabel3.Visible = true;
                    lblSuccessNotice.Text = "Your survey has been published!  Please copy and paste the following URL and Keys to be used later.";
                    lblSuccessNotice.BackColor = Color.FromArgb(230, 255, 191);
                    lblSuccessNotice2.Visible = true;
                    btnShowLog.Visible = true;
                   // this.btnKeyCopy.Enabled = true;
                    this.btnCopyAllURLs.Enabled = true;
                   // this.btnDataKeyCopy.Enabled = true;
                    this.btnGo.Enabled = true;
                   // this.btnURLCopy.Enabled = true;
                   //txtURL.Text = Result.PublishInfo.URL;
                    txtSurveyKey.Text = this.currentSurveyInfoDTO.SurveyId;

                    txtDataKey.Text = this.currentSurveyInfoDTO.UserPublishKey.ToString();
                    //txtDataKey.Text = txtSurveyKey.Text;
                    txtStatusSummary.Text = SharedStrings.WEBFORM_SUCCESS;
                   // txtURL.Text = Result.PublishInfo.URL;

                    //txtURL.Text =;

                    //txtStatusSummary.Text = SharedStrings.WEBFORM_SUCCESS;
                    //txtStatusSummary.Text = "Your Form has been republished: " + Result.Message;

                    lblSuccessNotice.Visible = true;
                    lblSuccessNotice2.Visible = true;
                    lblSuccessNotice.Text = "Your survey has been published!  Please copy and paste the following URL and Keys to be used later.";
                    lblSuccessNotice.BackColor = Color.FromArgb(230, 255, 191);
                    lblSuccessNotice2.Visible = true;
                    Importantlabel3.Visible = true;
                    btnPublishForm.Visible = true;
                    btnPublishForm.Enabled = false;
                    //btnURLCopy.Enabled = true;
                    btnGo.Enabled = true;
                   // btnKeyCopy.Enabled = true;
                   // btnDataKeyCopy.Enabled = true;
                    btnShowLog.Enabled = true;
                    btnShowLog.Visible = true;
                    btnCopyAllURLs.Enabled = true;

                    string message = DateTime.Now + ": " + SharedStrings.WEBFORM_SUCCESS + ": " + txtURL.Text; ///Result.PublishInfo.URL;
                    Logger.Log(message);
                    message = DateTime.Now + ": Survey Key= " + txtSurveyKey.Text;
                    Logger.Log(message);
                    message = DateTime.Now + ": Data Key= " + txtDataKey.Text;
                    Logger.Log(message);
                }
                else
                {
                    txtStatusSummary.Text = Result.Message;
                    lblSuccessNotice.Text = "The survey failed to publish. Please check that the organization key is correct and try again.";
                    //panel2.Visible = true;
                    btnShowLog.Visible = false;
                    lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
                    panel3.Visible = true;
                    lblSuccessNotice2.Visible = false;
                    Importantlabel3.Visible = false;
                    txtOrganizationKey.Enabled = true;
                    btnPublishForm.Enabled = true;
                }


                //this.progressBar.Visible = false;
                //this.btnPublish.Enabled = true;

            }
            catch (Exception ex)
            {
                txtStatusSummary.AppendText("An error occurred while trying to publish the survey.");
                txtStatus.AppendText(ex.ToString());
                btnDetails.Visible = true;
                //this.progressBar.Visible = false;
                this.Cursor = Cursors.Default;
            }
            

            this.progressBar.Visible = false;
            //this.btnPublish.Enabled = true;
        }

        private void AfterPublish(Epi.EWEManagerService.PublishResponse Result)
        {
            if (Result.PublishInfo.IsPulished)
            {
                txtURL.Text = Result.PublishInfo.URL;
                //txtSurveyKey.Text = txtURL.Text.Substring(txtURL.Text.LastIndexOf('/') + 1);
                txtSurveyKey.Text = Result.PublishInfo.ViewIdAndFormIdList.ToList()[0].Value.ToString();//Result.PublishInfo.ViewIdAndFormIdList[1];
                txtDataKey.Text = UserPublishGuid.ToString();
                //txtDataKey.Text = txtSurveyKey.Text;
                txtStatusSummary.Text = SharedStrings.WEBFORM_SUCCESS;
                string message = DateTime.Now + ": " + SharedStrings.WEBFORM_SUCCESS + ": " + Result.PublishInfo.URL;
                Logger.Log(message);
                message = DateTime.Now + ": Survey Key= " + txtSurveyKey.Text;
                Logger.Log(message);
                message = DateTime.Now + ": Data Key= " + txtDataKey.Text;
                Logger.Log(message);

                lblSuccessNotice.Visible = true;
                lblSuccessNotice2.Visible = true;
                lblSuccessNotice.Text = "Your survey has been published!  Please copy and paste the following URL and Keys to be used later.";
                lblSuccessNotice.BackColor = Color.FromArgb(230, 255, 191);
                lblSuccessNotice2.Visible = true;
                Importantlabel3.Visible = true;
                btnPublishForm.Visible = true;
                btnPublishForm.Enabled = false;
                //btnURLCopy.Enabled = true;
                btnGo.Enabled = true;
              //  btnKeyCopy.Enabled = true;
               // btnDataKeyCopy.Enabled = true;
                btnShowLog.Enabled = true;
                btnShowLog.Visible = true;
                btnCopyAllURLs.Enabled = true;
              
                //txtSurveyName.Enabled = true;
                //txtSurveyID.Enabled = true;
                //txtOrganization.Enabled = true;
                //txtDepartment.Enabled = true;
                //txtIntroductionText.Enabled = true;
                //txtExitText.Enabled = true;
                //dtpSurveyClosingDate.Enabled = true;
                txtOrganizationKey.Enabled = false;
                //txtOrganizationKey.Clear(); 
                //btnPrevious.Enabled = false;
                //btnNext.Enabled = false;
                panel3.Visible = true;
                panel2.Visible = true;


                if (this.isRepublishableConfig)
                    {
                     foreach (var item in Result.PublishInfo.ViewIdAndFormIdList){
                    // save survey id to metadata
                     
                       View NewView = new View(this.mediater.Project);

                       NewView  =  this.mediater.Project.GetViewById(item.Key);
 
                     
                       NewView.EWEFormId = item.Value;
                       NewView.EWEOrganizationKey = Configuration.Encrypt(txtOrganizationKey.Text.ToString());////Encrypting the OrgKey
                       NewView.SaveToDb();
                       
                       }
                    }

            }
            else
            {
                txtStatusSummary.Text = "The survey failed to publish. Please check that the organization key is correct and try again.";
                lblSuccessNotice.Text = "The survey failed to publish. Please check that the organization key is correct and try again.";
                lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
                panel2.Visible = false;
                panel3.Visible = true;
                btnPublishForm.Enabled = true;
                btnPublishForm.Visible = true;
                txtOrganizationKey.Enabled = true;
                btnPublishForm.Visible = true;
                btnShowLog.Visible = false;
                lblSuccessNotice2.Visible = false;
                Importantlabel3.Visible = false;
            }
            txtStatus.AppendText(Environment.NewLine);

           // panel2.Visible = false;
            panel3.Visible = true;
            
            

        }


        /// <summary>
        /// Handles custom fault exception during publishing
        /// </summary>
        /// <param name="CustomFaultException"></param>
        private void FinishWithCustomFaultException(FaultException<CustomFaultException> cfe)
        {
            boolError = true;
            string statusMessage = "Fault Exception<CustomFaultException>";
            AddStatusMessage(statusMessage);
            btnShowLog.Enabled = true;
            txtStatusSummary.Text = statusMessage; 
            txtStatus.AppendText(cfe.ToString());
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "Fault Exception<CustomFaultException>";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
        }

        /// <summary>
        /// Handles fault exception during publishing
        /// </summary>
        /// <param name="CustomFaultException"></param>
        private void FinishWithFaultException(FaultException fe)
        {
            boolError = true;
            string statusMessage = "Fault Exception";
            AddStatusMessage(statusMessage);
            btnShowLog.Enabled = true;
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(fe.ToString());
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "Fault Exception";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
        }

        /// <summary>
        /// Handles a security negotiation exception during publishing
        /// </summary>
        /// <param name="SecurityNegotiationException"></param>
        private void FinishWithSecurityNegotiationException(SecurityNegotiationException sne)
        {
            boolError = true;
            string statusMessage = "A Security Negotiation error occurred while trying to publish the survey.";
            AddStatusMessage(statusMessage);
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(sne.ToString());
            btnShowLog.Enabled = true;
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "A Security Negotiation error occurred while trying to publish the survey.";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
        }
        
        /// <summary>
        /// Handles a communication exception during publishing
        /// </summary>
        /// <param name="CommunicationException"></param>
        private void FinishWithCommunicationException(CommunicationException ce)
        {
            boolError = true;
            string statusMessage = "A Communication error occurred while trying to publish the survey.";
            AddStatusMessage(statusMessage);
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(ce.ToString());
            btnShowLog.Enabled = true;
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "A Communication error occurred while trying to publish the survey.";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            btnPublishForm.Enabled = false;
            Importantlabel3.Visible = false;


        }

        /// <summary>
        /// Handles Timeout exception during publishing
        /// </summary>
        /// <param name="TimeoutException"></param>
        private void FinishWithTimeoutException(TimeoutException te)
        {
            boolError = true;
            string statusMessage = "A Timeout error occurred while trying to publish the survey.";
            AddStatusMessage(statusMessage);
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(te.ToString());
            btnShowLog.Enabled = true;
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "A Timeout error occurred while trying to publish the survey.";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
            btnPublishForm.Enabled = false;
        }

        /// <summary>
        /// Handles general exception during publishing
        /// </summary>
        /// <param name="CustomFaultException"></param>
        private void FinishWithException(Exception ex)
        {
            boolError = true;
            string statusMessage = "An error occurred while trying to publish the survey.";
            AddStatusMessage(statusMessage);
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(ex.ToString());
            btnShowLog.Enabled = true;
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "An error occurred while trying to publish the survey.";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
            btnPublishForm.Enabled = false;
        }
        
        /// <summary>
        /// Adds a status message to the status list box
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": " + statusMessage;
            Logger.Log(message);
        }


        /// <summary>
        /// Adds a status error message to the log.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddErrorStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": Error: " + statusMessage;
            Logger.Log(message);
        }

        /// <summary>
        /// Sets the status message of the import form
        /// </summary>
        /// <param name="message">The status message to display</param>
        private void SetStatusMessage(string message)
        {
            txtStatusSummary.Text = message;
        }

        #endregion //Private Methods

        #region Event Handlers
        /// <summary>
        /// Handles the Click event of the Publish  button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnPublish_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    

        /// <summary>
        /// Handles the Click event of the Go button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnGo_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(txtURL.Text);
        }

        /// <summary>
        /// Handles the TextChanged event of the URL text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            btnGo.Enabled = !string.IsNullOrEmpty(txtURL.Text);
            //btnURLCopy.Enabled = btnGo.Enabled;
        }

        /// <summary>
        /// Handles the Click event of the Clear button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            //txtSurveyName.Text = string.Empty;
            //txtSurveyID.Text = string.Empty;
            //txtOrganization.Text = string.Empty;
            //txtDepartment.Text = string.Empty;
            //txtExitText.Text = string.Empty;
            txtStatusSummary.Text = string.Empty;
            txtStatus.Text = string.Empty;
          //  rdbSingleResponse.Checked = true;
          //  rdbMultipleResponse.Checked = false;
            txtURL.Text = string.Empty;
           // txtIntroductionText.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the Copy URL button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCopyAllURLs_Click(object sender, EventArgs e)
        {
            StringBuilder PubValues = new StringBuilder();
          //  PubValues.Append("Survey Name: " + StringLiterals.NEW_LINE + txtSurveyName.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            PubValues.Append(SharedStrings.WEBENTER_URLTOLOGIN + StringLiterals.NEW_LINE + txtURL.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            PubValues.Append(SharedStrings.WEBENTER_FORMID + StringLiterals.NEW_LINE + txtSurveyKey.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            PubValues.Append(SharedStrings.WEBFORM_SECURITYTOKEN + StringLiterals.NEW_LINE + txtDataKey.Text +StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            // PubValues.Append(txtSurveyName.Text + StringLiterals.NEW_LINE);
            // PubValues.Append(txtSurveyID.Text + StringLiterals.SEMI_COLON + StringLiterals.SPACE + txtOrganization.Text + StringLiterals.SEMI_COLON + StringLiterals.SPACE + txtDepartment.Text + StringLiterals.NEW_LINE);
            //PubValues.Append(txtIntroductionText.Text + StringLiterals.NEW_LINE);
            //PubValues.Append(dtpSurveyClosingDate.Text + StringLiterals.NEW_LINE);
            // PubValues.Append(txtExitText.Text + StringLiterals.NEW_LINE);

            Clipboard.SetText(PubValues.ToString());

         }

        /// <summary>
        /// Handles the Click event of the Copy All URLs to Clipboard button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtURL.Text);
        }

        /// <summary>
        /// Cancel button closes this dialog 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        //private void txtSurveyName_TextChanged(object sender, EventArgs e)
        //{
        //    txtSurveyNameMirror.Text = txtSurveyName.Text;
        //}

        //private void txtSurveyID_TextChanged(object sender, EventArgs e)
        //{
        //    txtSurveyIDMirror.Text = txtSurveyID.Text;
        //}

        //private void txtOrganization_TextChanged(object sender, EventArgs e)
        //{
        //    txtOrganizationMirror.Text = txtOrganization.Text;
        //}

        //private void txtDepartment_TextChanged(object sender, EventArgs e)
        //{
        //    this.txtDepartmentMirror.Text = txtDepartment.Text;
        //}

        private void WebPublishDialog_Load(object sender, EventArgs e)
        {
       // string WebSurveyId = this.mediater.Project.views[this.mediater.Project.Name].EWEFormId;

        DataTable table = mediater.Project.Metadata.GetPublishedViewKeys(this.mediater.ProjectExplorer.CurrentView.Id);
        DataRow ViewRow = table.Rows[0];

        string WebSurveyId = ViewRow.ItemArray[3].ToString();
         
        try
            {
            if (!string.IsNullOrEmpty(this.OrganizationKey))
                {
                this.txtOrganizationKey.Text = this.OrganizationKey;
                }
            //  if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(this.view.WebSurveyId))
            if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(WebSurveyId))
                {
                TimeSpan t = new TimeSpan(10, 23, 59, 59);
                //dtpSurveyClosingDate.Value = DateTime.Now.Date + t;
                //this.txtSurveyName.SelectAll();
                //txtSurveyName.Text = SharedStrings.WEBFORM_TITLE;
                //txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtSurveyNameMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtSurveyID.Text = SharedStrings.WEBFORM_SURVEYID;
                //txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtSurveyIDMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtSurveyIDMirror.Text = SharedStrings.WEBFORM_SURVEYID;
                //txtOrganization.Text = SharedStrings.WEBFORM_ORGANIZATION;
                //txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtOrganizationMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtOrganizationMirror.Text = SharedStrings.WEBFORM_ORGANIZATION;
                //txtDepartment.Text = SharedStrings.WEBFORM_DEPARTMENT;
                //txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtDepartmentMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtDepartmentMirror.Text = SharedStrings.WEBFORM_DEPARTMENT;
                //txtIntroductionText.Text = SharedStrings.WEBFORM_INTRODUCTIONTEXT;
                //txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                //txtExitText.Text = SharedStrings.WEBFORM_EXITTEXT;
                //txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;

                if (!this.isRepublishableConfig)
                    {
                    //this.lblSurveyStartDate.Visible = false;
                    //this.StartDateDatePicker.Visible = false;
                    //this.StartTimecomboBox.Visible = false;
                    //this.StartTimelabel.Visible = false;
                    this.OrganizationKeyLinkLabel.Visible = false;
                    this.OrganizationKeyValueLabel.Visible = false;
                    this.WebSurveyOptionsLinkLabel.Visible = false;
                    this.DividerLabel.Visible = false;
                    this.btnPublishForm.Enabled = true;

                    lblPublishModeStatus.Visible = false;
                    label1.Visible = false;
                    //lblClosingDate.Left = 18;
                    //dtpSurveyClosingDate.Left = 18;
                    // this.ClosingTimecomboBox.Left = 240;
                    // this.closingTimelabel.Left = 240;
                    //ClosingTimecomboBox.SelectedIndex = 0;
                    //ClosingTimecomboBox.Visible = false;
                    //closingTimelabel.Visible = false;
                    }
                else
                    {
                    // this.txtDepartment.Visible = false;
                    // this.lblDepartment.Visible = false;


                    // this.txtOrganization.Left = 381;
                    // this.txtOrganization.Size = new System.Drawing.Size(453, 23);
                    // this.lblOrganization.Left = 300;


                    // this.lblDepMirr.Visible = false;
                    // this.txtDepartmentMirror.Visible = false;

                    // this.txtOrganizationMirror.Left = 381;
                    // this.txtOrganizationMirror.Size = new System.Drawing.Size(453, 23);
                    // this.lblOrgMirr.Left = 300;
                    //ClosingTimecomboBox.SelectedIndex = 0;
                    //StartTimecomboBox.SelectedIndex = 0;

                    }

                }
            else
                {
                EWEManagerService.EWEManagerServiceClient client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();

                //this.txtDepartment.Visible = false;
                //this.lblDepartment.Visible = false;


                //this.txtOrganization.Left = 381;
                //this.txtOrganization.Size = new  System.Drawing.Size(453,23);
                //this.lblOrganization.Left = 300;


                //this.lblDepMirr.Visible = false;
                //this.txtDepartmentMirror.Visible = false;

                //this.txtOrganizationMirror.Left = 381;
                //this.txtOrganizationMirror.Size = new System.Drawing.Size(453, 23);
                //this.lblOrgMirr.Left = 300;


                //ClosingTimecomboBox.SelectedIndex = 0;
                //StartTimecomboBox.SelectedIndex = 0;



                // Epi.Windows.Dialogs.InputDialog inputDialog = new Windows.Dialogs.InputDialog("Enter organization key", "Organization Key", "", null, EpiInfo.Plugin.DataType.Text);
                //DialogResult result = inputDialog.ShowDialog();

                // if (result == System.Windows.Forms.DialogResult.OK)
                //{
                // this.OrganizationKey = inputDialog.OrganizationKey;
                this.txtOrganizationKey.Text = this.OrganizationKey;
                this.txtOrganizationKey.Enabled = false;

                // Epi.Web.Enter.Common.Message.SurveyInfoRequest Request = new Epi.Web.Enter.Common.Message.SurveyInfoRequest();
                EWEManagerService.SurveyInfoRequest Request = new EWEManagerService.SurveyInfoRequest();
                EWEManagerService.SurveyInfoCriteria Criteria = new EWEManagerService.SurveyInfoCriteria();

                List<string> IdList = new List<string>();
                IdList.Add(WebSurveyId);
                Criteria.SurveyIdList = IdList.ToArray();
                Criteria.OrganizationKey = new Guid(this.OrganizationKey);
                Criteria.SurveyType = -1;
                Request.Criteria = Criteria;
                //  Epi.Web.Enter.Common.Message.SurveyInfoResponse response = client.GetSurveyInfo(Request);
                var response = client.GetSurveyInfo(Request);
                if (response.SurveyInfoList.Count() > 0)
                    {
                    currentSurveyInfoDTO = response.SurveyInfoList[0];
                    if (currentSurveyInfoDTO.IsDraftMode)
                        {
                        this.lblPublishModeStatus.Text = "Draft";
                        }
                    else
                        {
                        this.lblPublishModeStatus.Text = "Final";
                        }

                    //dtpSurveyClosingDate.Value = currentSurveyInfoDTO.ClosingDate;

                    //StartDateDatePicker.Value = currentSurveyInfoDTO.StartDate;
                    DateTime CloseTime = Convert.ToDateTime(currentSurveyInfoDTO.ClosingDate);
                    DateTime StartTime = Convert.ToDateTime(currentSurveyInfoDTO.StartDate);

                    //ClosingTimecomboBox.SelectedItem = this.GetComboBoxTimeFormat(CloseTime.Hour + ":" + CloseTime.Minute);
                    //StartTimecomboBox.SelectedItem = this.GetComboBoxTimeFormat(StartTime.Hour + ":" + StartTime.Minute);

                    //txtSurveyName.Text = currentSurveyInfoDTO.SurveyName;
                    ////txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    ////txtSurveyNameMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    //txtSurveyID.Text = currentSurveyInfoDTO.SurveyNumber;
                    ////txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    ////txtSurveyIDMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    //txtSurveyIDMirror.Text = currentSurveyInfoDTO.SurveyNumber;
                    //txtOrganization.Text = currentSurveyInfoDTO.OrganizationName;
                    ////txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    ////txtOrganizationMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    //txtOrganizationMirror.Text = currentSurveyInfoDTO.OrganizationName;
                    //txtDepartment.Text = currentSurveyInfoDTO.DepartmentName;
                    ////txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    ////txtDepartmentMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    //txtDepartmentMirror.Text = currentSurveyInfoDTO.DepartmentName;
                    //txtIntroductionText.Text = currentSurveyInfoDTO.IntroductionText;
                    ////txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    //txtExitText.Text = currentSurveyInfoDTO.ExitText;
                    ////txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;

                    btnPublishForm.Enabled = true;
                    }
                //}
                }
            }
            catch (Exception ex)
            {
            throw ex;
             }
        }

        private void txtSurveyName_Enter(object sender, EventArgs e)
        {
            //if (txtSurveyName.Text == SharedStrings.WEBFORM_TITLE)
            //{
            //    txtSurveyName.Text = string.Empty;
            //    txtSurveyName.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtSurveyName_Leave(object sender, EventArgs e)
        {
            //if (txtSurveyName.Text == string.Empty)
            //{
            //    txtSurveyName.Text = SharedStrings.WEBFORM_TITLE;
            //    txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            //}
        }

        private void txtSurveyID_Enter(object sender, EventArgs e)
        {
            //if (txtSurveyID.Text == SharedStrings.WEBFORM_SURVEYID)
            //{
            //    txtSurveyID.Text = string.Empty;
            //    txtSurveyID.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtSurveyID_Leave(object sender, EventArgs e)
        {
        //if (txtSurveyID.Text == string.Empty)
        //    {
        //    txtSurveyID.Text = SharedStrings.WEBFORM_SURVEYID;
        //    txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
        //    }
        }

        private void txtOrganization_Enter(object sender, EventArgs e)
        {
            //if (txtOrganization.Text == SharedStrings.WEBFORM_ORGANIZATION)
            //{
            //    txtOrganization.Text = string.Empty;
            //    txtOrganization.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtOrganization_Leave(object sender, EventArgs e)
        {
            //if (txtOrganization.Text == string.Empty)
            //{
            //    txtOrganization.Text = SharedStrings.WEBFORM_ORGANIZATION;
            //    txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            //}
        }

        private void txtDepartment_Enter(object sender, EventArgs e)
        {
            //if (txtDepartment.Text == SharedStrings.WEBFORM_DEPARTMENT)
            //{
            //    txtDepartment.Text = string.Empty;
            //    txtDepartment.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtDepartment_Leave(object sender, EventArgs e)
        {
            //if (txtDepartment.Text == string.Empty)
            //{
            //    txtDepartment.Text = SharedStrings.WEBFORM_DEPARTMENT;
            //    txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            //}
        }

        private void txtIntroductionText_Enter(object sender, EventArgs e)
        {
            //if (txtIntroductionText.Text == SharedStrings.WEBFORM_INTRODUCTIONTEXT)
            //{
            //    txtIntroductionText.Text = string.Empty;
            //    txtIntroductionText.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtIntroductionText_Leave(object sender, EventArgs e)
        {
            //if (txtIntroductionText.Text == string.Empty)
            //{
            //    txtIntroductionText.Text = SharedStrings.WEBFORM_INTRODUCTIONTEXT;
            //    txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            //}
        }

        private void txtExitText_Enter(object sender, EventArgs e)
        {
            //if (txtExitText.Text == SharedStrings.WEBFORM_EXITTEXT)
            //{
            //    txtExitText.Text = string.Empty;
            //    txtExitText.ForeColor = System.Drawing.SystemColors.WindowText;
            //}
        }

        private void txtExitText_Leave(object sender, EventArgs e)
        {
            //if (txtExitText.Text == string.Empty)
            //{
            //    txtExitText.Text = SharedStrings.WEBFORM_EXITTEXT;
            //    txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            //}
        }

        private void btnPublishForm_Click(object sender, EventArgs e)
        {
            if (!txtStatusSummary.Text.Equals(string.Empty))
            {
                txtStatusSummary.Clear();
            }

            DialogResult OkToPublish = DialogResult.No;
            bool validOrgKey = true;

            try
            {
                System.Guid guid = new Guid(txtOrganizationKey.Text);
                this.OrganizationKey = guid.ToString();
            }
            catch (FormatException)
            {
                validOrgKey = false;
            }

            if (!validOrgKey)
            {
                MsgBox.ShowError(SharedStrings.WEBFORM_ORGANIZATION_KEY);
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[2];
                txtOrganizationKey.SelectAll();
            }
            //else if (dtpSurveyClosingDate.Value + new TimeSpan(0, 23, 59, 59) <= DateTime.Now)
            //{
            //    MsgBox.ShowError(SharedStrings.WEBFORM_FUTUREDATE);
            //    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            //}
            //else if (txtSurveyName.Text.Equals(string.Empty) || txtSurveyName.Text.Equals(SharedStrings.WEBFORM_TITLE))
            //{
            //    MsgBox.ShowError(SharedStrings.WEBFORM_GIVETITLE);
            //    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            //    txtSurveyName.SelectAll();
            //}
            //else if (txtExitText.Text.Equals(string.Empty) || txtExitText.Text.Equals(SharedStrings.WEBFORM_EXITTEXT))
            //{
            //    OkToPublish = MsgBox.ShowQuestion(SharedStrings.WEBFORM_PUBWOEXIT);
            //    if (OkToPublish.Equals(DialogResult.No))
            //    {
            //        this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[1];
            //        this.txtExitText.SelectAll();
            //    }
            //}
            else
            {
                OkToPublish = DialogResult.Yes;
            }


            if (OkToPublish.Equals(DialogResult.Yes))
            {

            //DataTable table = this.mediater.Project.Metadata.GetPublishedViewKeys(this.mediater.ProjectExplorer.CurrentView.Id);
            DataTable table;
            View RootView = this.mediater.Project.Metadata.GetParentView(this.mediater.ProjectExplorer.CurrentView.Id);
            if (RootView == null)
                {
                      table = this.mediater.Project.Metadata.GetPublishedViewKeys(this.mediater.ProjectExplorer.CurrentView.Id);
                    }
                else
                    {
                        table = this.mediater.Project.Metadata.GetPublishedViewKeys(RootView.Id);
                    }
            DataRow ViewRow = table.Rows[0];
            
            string EWEFormId = ViewRow.ItemArray[3].ToString();
              //  if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(this.view.WebSurveyId))
            if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(EWEFormId))
                {
                    DoPublish();
                }
                else
                {
                    DoRePublish();
                }
            }
        
}

        private void txtSurveyNameMirror_MouseClick(object sender, MouseEventArgs e)
        {
            //this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            //txtSurveyName.SelectAll();
        }

        private void btnKeyCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtSurveyKey.Text);
        }

        //private void txtSurveyIDMirror_MouseClick(object sender, MouseEventArgs e)
        //{
        //    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
        //    txtSurveyID.SelectAll();
        //}

        //private void txtOrganizationMirror_MouseClick(object sender, MouseEventArgs e)
        //{
        //    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
        //    txtOrganization.SelectAll();
        //}

        //private void txtDepartmentMirror_MouseClick(object sender, MouseEventArgs e)
        //{
        //    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
        //    txtDepartment.SelectAll();
        //}

        private void btnDetails_Click(object sender, EventArgs e)
        {
            txtStatus.Visible = !txtStatus.Visible;
        }

        private void WebPublishDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (publishWorker.IsBusy)
            {
                Epi.Windows.MsgBox.ShowInformation("Aborting the publishing process.");
                publishWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Handles the Click event of the Show Log button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnShowLog_Click(object sender, EventArgs e)
        {
            WinUtil.OpenTextFile(Logger.GetLogFilePath());
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try {
            stopwatch.Stop();

          this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Publishing complete. Time elapsed: " + stopwatch.Elapsed.ToString());
              }
            catch (FaultException<CustomFaultException> cfe)
            {
                this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);
            }
            catch (FaultException fe)
            {
                this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);
            }
            catch (SecurityNegotiationException sne)
            {
                this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);
            }
            catch (CommunicationException ce)
            {
                this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);
            }
            catch (TimeoutException te)
            {
                this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);
            }
            catch (Exception ex)
            {
                // this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);
            boolError = true;
            string statusMessage = "An error occurred while trying to publish the survey.";
            AddStatusMessage(statusMessage);
            txtStatusSummary.Text = statusMessage;
            txtStatus.AppendText(ex.ToString());
            btnShowLog.Enabled = true;
            btnDetails.Visible = true;
            this.progressBar.Visible = false;
            this.Cursor = Cursors.Default;
            panel2.Visible = false;
            lblSuccessNotice.Text = "An error occurred while trying to publish the survey.";
            lblSuccessNotice.BackColor = Color.FromArgb(243, 217, 217);
            panel3.Visible = true;
            lblSuccessNotice2.Visible = false;
            Importantlabel3.Visible = false;
            Importantlabel3.Visible = false;
            btnPublishForm.Enabled = false;
               
            }

            btnCancel.Enabled = true;
            btnPublishForm.Visible = true;
            progressBar.Visible = false;

            publishFinished = true;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Handles the DoWorker event for the publish worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
            var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();

                //Epi.Web.Common.Message.PublishRequest Request = (Epi.Web.Common.Message.PublishRequest)((object[])e.Argument)[0];
                //Epi.Web.Common.Message.PublishResponse Result = (Epi.Web.Common.Message.PublishResponse)((object[])e.Argument)[1];
            EWEManagerService.PublishRequest Request = (EWEManagerService.PublishRequest)((object[])e.Argument)[0];
            EWEManagerService.PublishResponse Result = (EWEManagerService.PublishResponse)((object[])e.Argument)[1];
                Result = client.PublishSurvey(Request);
                this.BeginInvoke(new PublishDelegate(AfterPublish), Result);
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);
            }
            catch (FaultException fe)
            {
                this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);
            }
            catch (SecurityNegotiationException sne)
            {
                this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);
            }
            catch (CommunicationException ce)
            {
                this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);
            }
            catch (TimeoutException te)
            {
                this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);
            }
        }

        //private void btnNext_Click(object sender, EventArgs e)
        //{
        //    if (this.tabPublishWebForm.SelectedIndex < 2)
        //    {
        //        this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[this.tabPublishWebForm.SelectedIndex + 1];
        //        //if (this.tabPublishWebForm.SelectedIndex.Equals(1))
        //        //{
        //        //    this.tabExit.Focus();
        //        //}
        //    }
        //    if (this.tabPublishWebForm.SelectedIndex.Equals(2))
        //    {
        //        btnNext.Enabled = false;
        //    }
        //    btnPrevious.Enabled = true;
        //}

        //private void tabIntro_Click(object sender, EventArgs e)
        //{
        //    btnNext.Enabled = true;
        //}

        //private void tabExit_Click(object sender, EventArgs e)
        //{
        //    btnNext.Enabled = true;
        //}

        //private void tabEntry_Click(object sender, EventArgs e)
        //{
        //    btnNext.Enabled = false;
        //}

        //private void tabPublishWebForm_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    btnNext.Enabled = (this.tabPublishWebForm.SelectedIndex.Equals(2)) ? false : true ;
        //}

        private void btnDataKeyCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtDataKey.Text);
        }
        #endregion //Event Handlers

        private void txtOrganizationKey_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOrganizationKey.Text))
            {          
                string guidPattern = @"[a-fA-F0-9]{8}(-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}";
                Regex regPattern = new Regex(guidPattern);
                btnPublishForm.Enabled = regPattern.IsMatch(txtOrganizationKey.Text);
            }
            else
            {
                btnPublishForm.Enabled = false;
            }
        }

        private void pbExitImage_Click(object sender, EventArgs e)
        {

        }

        private void grpExitText_Enter(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           // OrgKey OK = new OrgKey();
            //OK.Show();
            OrgKey NewDialog = new OrgKey(this.view.EIWSFormId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can change the survey mode.");
            DialogResult result = NewDialog.ShowDialog();
           
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        WebEnterOptions wso = new WebEnterOptions();
            wso.Show();
        }

        //private void btnPrevious_Click(object sender, EventArgs e)
        //{
        //    if (this.tabPublishWebForm.SelectedIndex > 0)
        //    {
        //        this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[this.tabPublishWebForm.SelectedIndex - 1];
        //        //if (this.tabPublishWebForm.SelectedIndex.Equals(1))
        //        //{
        //        //    this.tabIntro.Focus();
        //        //}
        //    }

        //    if (this.tabPublishWebForm.SelectedIndex.Equals(0))
        //    {
        //        btnPrevious.Enabled = false;
        //    }
        //    btnNext.Enabled = true;
        //}



        private TimeSpan GetTimeFormat(string Time) 
            
            {

            TimeSpan t;
            try
                {
                int HH = Convert.ToInt32(Time.Substring(0, 2));
                int MM = Convert.ToInt32(Time.Substring(3, 2));
                string PMAM = Time.Substring(5, 2);

                if (PMAM.Equals("PM"))
                    {
                    t = new TimeSpan(0, HH + 12, MM, 59);
                    }
                else
                    {
                    t = new TimeSpan(0, HH, MM, 59);
                    }
                }
            catch (Exception ex)
                {

                t = new TimeSpan(0, 00, 00, 01);
                }
            return t;
            
            }
        
        private string GetComboBoxTimeFormat(string time) 
            {

           var HHMM = time.Split(':');
           int HH = Convert.ToInt32(HHMM[0]);
           int MM = Convert.ToInt32(HHMM[1]);
           string hh = "";
           string mm = "";

           if (MM.ToString().Length == 1)
               {
               mm = MM.ToString() + "0";
               }
           else 
               {
                mm = MM.ToString();
               }
           if (HH.ToString().Length == 1)
               {
               hh = "0" + HH.ToString();
               }
           else {
               hh =   HH.ToString();
               }

           if (HH < 12)
                {
                if (HH ==0)
                   {
                   time = "12" + ":" + mm + "AM";
                   }
               else
                   {
                   time = hh +":"+ mm + "AM";
                   }
                }
            else
                {
                if (HH == 12)
                   {
                    hh = "12";
                   }
               else
                   {
                    HH = HH - 12;
                    if (HH.ToString().Length == 1)
                       {
                        hh = "0" + HH;
                       }
                     else
                        {
                          hh =   HH.ToString();
                       }
                   }
                time = hh + ":" + mm + "PM";
                }


            return time;
            
            }

        private DateTime GetdateTimeFormat(DateTime DateTime , string Time) 
            {
             
              var JustDate =  String.Format("{0:M/d/yyyy}", DateTime);
              DateTime NewDate;
             
              try
                  {
                  int HH = Convert.ToInt32(Time.Substring(0, 2));
                  int MM = Convert.ToInt32(Time.Substring(3, 2));
                  string PMAM = Time.Substring(5, 2);

                  if (PMAM.Equals("PM"))
                      {
                      string DateAndTime = JustDate + " "+ HH +":" + MM +":00" + " PM";
                      NewDate = Convert.ToDateTime(DateAndTime);
                       
                      }
                  else
                      {
                      string DateAndTime = JustDate +" "+  HH + ":" + MM + ":00" + " AM";
                      NewDate = Convert.ToDateTime(DateAndTime);
                     
                      }
                  }
              catch (Exception ex)
                  {

                 
                  
                  string a = JustDate  + " 00:00:00 AM";
                  NewDate = Convert.ToDateTime(a);
                  }

              

            
              return NewDate;
            }

        private void lblURL_Click(object sender, EventArgs e)
            {

            }

        private void tabEntry_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void Shareable_CheckedChanged(object sender, EventArgs e)
        {
            if(this.Shareable.Checked)
            {
               this.DataAccessRuleList.Visible = true;
               //this.DataAccessRulelabel.Visible  = true;
            }
            else
            {
                this.DataAccessRuleList.Visible = false;
               /// this.DataAccessRulelabel.Visible = false;
                this.DataAccessRuleList.SelectedValue = 1;
                this.DataAccessRuleList.Text = "";
            }
        }
    }
}
