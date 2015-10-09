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
using Epi.Windows.MakeView.PresentationLogic;
namespace Epi.Windows.MakeView.Dialogs
{
    public partial class WebPublishDialog : Form
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
        private string OrganizationKey = null;
        private bool IsMetaDataOnly = false;
        private bool isRepublishableConfig = false;
        private GuiMediator mediater;
        private SurveyManagerService.SurveyInfoDTO currentSurveyInfoDTO ;

        //=======
        //private Epi.Web.Common.Message.PublishResponse Result;
        //=======
        #endregion //Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
       // private delegate void PublishDelegate(Epi.Web.Common.Message.PublishResponse Result);
        private delegate void PublishDelegate(SurveyManagerService.PublishResponse Result);
        private delegate void FinishWithErrorDelegate(string errorMessage);
        private delegate void FinishWithCustomFaultExceptionDelegate(FaultException<CustomFaultException> cfe);
        private delegate void FinishWithFaultExceptionDelegate(FaultException fe);
        private delegate void FinishWithSecurityNegotiationExceptionDelegate(SecurityNegotiationException sne);
        private delegate void FinishWithCommunicationExceptionDelegate(CommunicationException ce);
        private delegate void FinishWithTimeoutExceptionDelegate(TimeoutException te);
        private delegate void FinishWithExceptionDelegate(Exception ex);
        #endregion //Delegates

        #region Constructors

        public WebPublishDialog()
        {
            InitializeComponent();
        }
        public string GetOrgKey
            {
            get { return this.OrganizationKey; }
        }
        public WebPublishDialog(string pOrganizationKey,GuiMediator pMediator, Epi.View pView, string pTemplate, bool pIsMetaDataOnly = false)
        {
            InitializeComponent();
            this.mediater = pMediator;
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

            PopulateTimeDropDowns();

            Construct();
        }

        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Construct method
        /// </summary>
        /// 
        private void Construct()
        {
            this.publishWorker = new BackgroundWorker();
            this.publishWorker.WorkerSupportsCancellation = true;
        }

        void PopulateTimeDropDowns()
        {
            DateTime dateTime = new DateTime();
            TimeSpan timeSpan = new TimeSpan(0,30,0);
            string timeString = string.Empty;

            StartTimecomboBox.Items.Add(string.Empty);
            ClosingTimecomboBox.Items.Add(string.Empty);

            for (int i = 0; i < 48; i++)
            {
                timeString = dateTime.ToShortTimeString();
                StartTimecomboBox.Items.Add(timeString);
                ClosingTimecomboBox.Items.Add(timeString);

                dateTime += timeSpan;
            }

            dateTime = dateTime.Subtract(new TimeSpan(0, 1, 0));
            timeString = dateTime.ToShortTimeString();
            ClosingTimecomboBox.Items.Add(timeString);
        }

        /// <summary>
        /// Initiates a single form publishing process
        /// </summary>
        private void DoPublish()
        {

            UserPublishGuid = Guid.NewGuid();
            txtSurveyName.Enabled = false;
            txtSurveyID.Enabled = false;
            txtOrganization.Enabled = false;
            txtDepartment.Enabled = false;
            txtIntroductionText.Enabled = false;
            txtExitText.Enabled = false;
            dtpSurveyClosingDate.Enabled = false;
            txtOrganizationKey.Enabled = false;
            btnPrevious.Enabled = false;
            txtStatus.Clear();
            txtURL.Clear();
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[2];
          
            btnPublishForm.Enabled = true;
            progressBar.Visible = true;

            stopwatch = new Stopwatch();
            stopwatch.Start();

          //  Epi.Web.Common.Message.PublishRequest Request = new Epi.Web.Common.Message.PublishRequest();
            SurveyManagerService.PublishRequest Request = new SurveyManagerService.PublishRequest();
            Request.SurveyInfo = new SurveyManagerService.SurveyInfoDTO();

            if (string.IsNullOrEmpty(ClosingTimecomboBox.Text))
            {
                Request.SurveyInfo.ClosingDate = dtpSurveyClosingDate.Value.Date + new TimeSpan(0, 23, 59, 0);
            }
            else 
            {
                Request.SurveyInfo.ClosingDate = GetdateTimeFormat(dtpSurveyClosingDate.Value.Date, ClosingTimecomboBox.Text); 
            }
           
            if (string.IsNullOrEmpty(StartTimecomboBox.Text))
            {
                Request.SurveyInfo.StartDate = StartDateDatePicker.Value.Date;
            }
            else
            {
                Request.SurveyInfo.StartDate = GetdateTimeFormat(StartDateDatePicker.Value.Date ,StartTimecomboBox.Text);
            }

            Request.SurveyInfo.DBConnectionString = RemoveUserName(this.mediater.Project.CollectedDataConnectionString);

            if (this.mediater.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
                {
                Request.SurveyInfo.IsSqlProject = true;
                }
            Request.SurveyInfo.DepartmentName = txtDepartment.Text;
            Request.SurveyInfo.IntroductionText = txtIntroductionText.Text;
            Request.SurveyInfo.ExitText = txtExitText.Text;
            
            Request.SurveyInfo.SurveyName = txtSurveyName.Text;
            
            Request.SurveyInfo.OrganizationKey = new Guid(txtOrganizationKey.Text.ToString());
            Request.SurveyInfo.UserPublishKey = UserPublishGuid;
            Request.SurveyInfo.XML = template;
            Request.SurveyInfo.IsDraftMode = true;
            Request.SurveyInfo.SurveyType = (rdbSingleResponse.Checked) ? 1 : 2;
            if (txtOrganization.Text.Equals("Your Organization Name (optional)", StringComparison.OrdinalIgnoreCase))
            {
                Request.SurveyInfo.OrganizationName = null;
            }
            else
            {
                Request.SurveyInfo.OrganizationName = txtOrganization.Text;
            }

            if (txtSurveyID.Text.Equals("Your Survey ID (optional)", StringComparison.OrdinalIgnoreCase))
            {
                Request.SurveyInfo.SurveyNumber = null;
            }
            else
            {
                Request.SurveyInfo.SurveyNumber = txtSurveyID.Text;
            }


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
              //  Epi.Web.Common.Message.PublishResponse Result = new Epi.Web.Common.Message.PublishResponse();
                SurveyManagerService.PublishResponse Result = new SurveyManagerService.PublishResponse();
               
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
            txtSurveyName.Enabled = false;
            txtSurveyID.Enabled = false;
            txtOrganization.Enabled = false;
            txtDepartment.Enabled = false;
            txtIntroductionText.Enabled = false;
            txtExitText.Enabled = false;
            dtpSurveyClosingDate.Enabled = false;
            txtOrganizationKey.Enabled = false;
            btnPrevious.Enabled = false;
            txtStatus.Clear();
            txtURL.Clear();
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[2];

            btnPublishForm.Enabled = false;


            progressBar.Visible = true;

            SurveyManagerService.PublishRequest Request = new SurveyManagerService.PublishRequest();
            Request.Action = "Update";

          
            //this.currentSurveyInfoDTO.ClosingDate =  dtpSurveyClosingDate.Value.Date + GetTimeFormat(ClosingTimecomboBox.Text);
             
             this.currentSurveyInfoDTO.ClosingDate = GetdateTimeFormat( dtpSurveyClosingDate.Value.Date ,ClosingTimecomboBox.Text); 

            //this.currentSurveyInfoDTO.StartDate = StartDateDatePicker.Value.Date + GetTimeFormat(StartTimecomboBox.Text);
            this.currentSurveyInfoDTO.StartDate = GetdateTimeFormat(StartDateDatePicker.Value.Date,StartTimecomboBox.Text);
            this.currentSurveyInfoDTO.DepartmentName = txtDepartment.Text;
            
            //this.currentSurveyInfoDTO.DepartmentName = txtDepartment.Text;
            this.currentSurveyInfoDTO.IntroductionText = txtIntroductionText.Text;
            this.currentSurveyInfoDTO.ExitText = txtExitText.Text;
            if (txtOrganization.Text.Equals("Your Organization Name (optional)", StringComparison.OrdinalIgnoreCase))
            {
                this.currentSurveyInfoDTO.OrganizationName = null;
            }
            else
            {
                this.currentSurveyInfoDTO.OrganizationName = txtOrganization.Text;
            }

            if (txtSurveyID.Text.Equals("Your Survey ID (optional)", StringComparison.OrdinalIgnoreCase))
            {
                this.currentSurveyInfoDTO.SurveyNumber = null;
            }
            else
            {
                this.currentSurveyInfoDTO.SurveyNumber = txtSurveyID.Text;
            }


            this.currentSurveyInfoDTO.SurveyName = txtSurveyName.Text;
            
            this.currentSurveyInfoDTO.OrganizationKey = new Guid(txtOrganizationKey.Text.ToString());
            //this.currentSurveyInfoDTO.UserPublishKey = UserPublishGuid;
            if (!this.IsMetaDataOnly)
            {
                this.currentSurveyInfoDTO.XML = this.template;
            }
            this.currentSurveyInfoDTO.SurveyType = (rdbSingleResponse.Checked) ? 1 : 2;

            Request.SurveyInfo  = this.currentSurveyInfoDTO;

            Request.SurveyInfo.DBConnectionString = RemoveUserName(this.mediater.Project.CollectedDataConnectionString);

            if (this.mediater.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
                {
                Request.SurveyInfo.IsSqlProject = true;
                }
            try
            {
            SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                SurveyManagerService.PublishResponse Result = client.RePublishSurvey(Request);

                panel2.Visible = true;
                panel3.Visible = true;

                if (Result.PublishInfo.IsPulished)
                {
                    panel3.Visible = true;

                    txtURL.Text = Result.PublishInfo.URL;
                    lblSuccessNotice.Visible = true;
                    lblSuccessNotice2.Visible = true;
                    lblSuccessNotice.Text = "Your survey has been published!  Please copy and paste the following URL and Keys to be used later.";
                    lblSuccessNotice.BackColor = Color.FromArgb(230, 255, 191);
                    lblSuccessNotice2.Visible = true;
                    btnShowLog.Visible = true;
                    this.btnKeyCopy.Enabled = true;
                    this.btnCopyAllURLs.Enabled = true;
                    this.btnDataKeyCopy.Enabled = true;
                    this.btnGo.Enabled = true;
                    this.btnURLCopy.Enabled = true;
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
                    btnPublishForm.Visible = true;
                    btnPublishForm.Enabled = false;
                    btnURLCopy.Enabled = true;
                    btnGo.Enabled = true;
                    btnKeyCopy.Enabled = true;
                    btnDataKeyCopy.Enabled = true;
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

        //private void AfterPublish(Epi.Web.Common.Message.PublishResponse Result)
        private void AfterPublish(SurveyManagerService.PublishResponse Result)
        {
            if (Result.PublishInfo.IsPulished)
            {
                txtURL.Text = Result.PublishInfo.URL;
                txtSurveyKey.Text = txtURL.Text.Substring(txtURL.Text.LastIndexOf('/') + 1);

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
                btnPublishForm.Visible = true;
                btnPublishForm.Enabled = false;
                btnURLCopy.Enabled = true;
                btnGo.Enabled = true;
                btnKeyCopy.Enabled = true;
                btnDataKeyCopy.Enabled = true;
                btnShowLog.Enabled = true;
                btnShowLog.Visible = true;
                btnCopyAllURLs.Enabled = true;

                txtSurveyName.Enabled = true;
                txtSurveyID.Enabled = true;
                txtOrganization.Enabled = true;
                txtDepartment.Enabled = true;
                txtIntroductionText.Enabled = true;
                txtExitText.Enabled = true;
                dtpSurveyClosingDate.Enabled = true;
                txtOrganizationKey.Enabled = false;
                //txtOrganizationKey.Clear(); 
                btnPrevious.Enabled = false;
                //btnNext.Enabled = false;
                panel3.Visible = true;
                panel2.Visible = true;
                if (this.isRepublishableConfig)
                {
                    // save survey id to metadata
                    //this.viewma.metad
                    this.view.WebSurveyId = txtSurveyKey.Text;
                    this.view.SaveToDb();
                    this.view.CheckCodeBefore = txtOrganizationKey.Text.ToString();
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
            btnURLCopy.Enabled = btnGo.Enabled;
        }

        /// <summary>
        /// Handles the Click event of the Clear button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSurveyName.Text = string.Empty;
            txtSurveyID.Text = string.Empty;
            txtOrganization.Text = string.Empty;
            txtDepartment.Text = string.Empty;
            txtExitText.Text = string.Empty;
            txtStatusSummary.Text = string.Empty;
            txtStatus.Text = string.Empty;
            rdbSingleResponse.Checked = true;
            rdbMultipleResponse.Checked = false;
            txtURL.Text = string.Empty;
            txtIntroductionText.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the Copy URL button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCopyAllURLs_Click(object sender, EventArgs e)
        {
            StringBuilder PubValues = new StringBuilder();
            PubValues.Append("Survey Name: " + StringLiterals.NEW_LINE + txtSurveyName.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            PubValues.Append(SharedStrings.WEBFORM_URLTOSEND + StringLiterals.NEW_LINE + txtURL.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
            PubValues.Append(SharedStrings.WEBFORM_SURVEYKEY + StringLiterals.NEW_LINE + txtSurveyKey.Text + StringLiterals.NEW_LINE + StringLiterals.NEW_LINE);
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

        private void txtSurveyName_TextChanged(object sender, EventArgs e)
        {
            txtSurveyNameMirror.Text = txtSurveyName.Text;
        }

        private void txtSurveyID_TextChanged(object sender, EventArgs e)
        {
            txtSurveyIDMirror.Text = txtSurveyID.Text;
        }

        private void txtOrganization_TextChanged(object sender, EventArgs e)
        {
            txtOrganizationMirror.Text = txtOrganization.Text;
        }

        private void txtDepartment_TextChanged(object sender, EventArgs e)
        {
            this.txtDepartmentMirror.Text = txtDepartment.Text;
        }

        private void WebPublishDialog_Load(object sender, EventArgs e)
        {
            if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(this.view.WebSurveyId))
            {
                TimeSpan t = new TimeSpan(10, 23, 59, 59);
                dtpSurveyClosingDate.Value = DateTime.Now.Date + t;
                this.txtSurveyName.SelectAll();
                txtSurveyName.Text = SharedStrings.WEBFORM_TITLE;
                txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtSurveyNameMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtSurveyID.Text = SharedStrings.WEBFORM_SURVEYID;
                txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtSurveyIDMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtSurveyIDMirror.Text = SharedStrings.WEBFORM_SURVEYID;
                txtOrganization.Text = SharedStrings.WEBFORM_ORGANIZATION;
                txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtOrganizationMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtOrganizationMirror.Text = SharedStrings.WEBFORM_ORGANIZATION;
                txtDepartment.Text = SharedStrings.WEBFORM_DEPARTMENT;
                txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtDepartmentMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtDepartmentMirror.Text = SharedStrings.WEBFORM_DEPARTMENT;
                txtIntroductionText.Text = SharedStrings.WEBFORM_INTRODUCTIONTEXT;
                txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                txtExitText.Text = SharedStrings.WEBFORM_EXITTEXT;
                txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;

                if (!this.isRepublishableConfig)
                    {
                    this.lblSurveyStartDate.Visible = false;
                    this.StartDateDatePicker.Visible = false;
                    this.StartTimecomboBox.Visible = false;
                    this.StartTimelabel.Visible = false;
                    this.WebSurveyOptionsLinkLabel.Visible = false;
                    this.btnPublishForm.Enabled = true;

                    lblPublishModeStatus.Visible = false;
                    label1.Visible = false;
                    lblClosingDate.Left = 18;
                    dtpSurveyClosingDate.Left = 18;
                   // this.ClosingTimecomboBox.Left = 240;
                   // this.closingTimelabel.Left = 240;
                    //ClosingTimecomboBox.SelectedIndex = 0;
                    ClosingTimecomboBox.Visible = false;
                    closingTimelabel.Visible = false;
                }
                else
                {
                    this.txtDepartment.Visible = false;
                    this.lblDepartment.Visible = false;


                        this.txtOrganization.Left = 295;
                        this.txtOrganization.Size = new System.Drawing.Size(540, 23);
                        this.lblOrganization.Left = 292;


                        this.lblDepMirr.Visible = false;
                        this.txtDepartmentMirror.Visible = false;

                        this.txtOrganizationMirror.Left = 295;
                        this.txtOrganizationMirror.Size = new System.Drawing.Size(540, 23);
                        this.lblOrgMirr.Left = 292;
                       ClosingTimecomboBox.SelectedIndex = 0;
                       StartTimecomboBox.SelectedIndex = 0;
                       
                    }
                
            }
            else
            {
            SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();

                    this.txtDepartment.Visible = false;
                    this.lblDepartment.Visible = false;
                    
                   
                    this.txtOrganization.Left = 295;
                    this.txtOrganization.Size = new System.Drawing.Size(540, 23);
                    this.lblOrganization.Left = 292;
                     
                     
                    this.lblDepMirr.Visible = false;
                    this.txtDepartmentMirror.Visible = false;

                    this.txtOrganizationMirror.Left = 295;
                    this.txtOrganizationMirror.Size = new System.Drawing.Size(540, 23);
                    this.lblOrgMirr.Left = 292;

                     
                    ClosingTimecomboBox.SelectedIndex = 0;
                    StartTimecomboBox.SelectedIndex = 0;
                  

                this.txtOrganizationKey.Text = this.OrganizationKey;

                SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();
                Request.Criteria = new SurveyManagerService.SurveyInfoCriteria();
                Request.Criteria.SurveyType = -1;
                Request.Criteria.SurveyIdList = new string[]{this.view.WebSurveyId};
                Request.Criteria.OrganizationKey = new Guid(this.OrganizationKey);
                SurveyManagerService.SurveyInfoResponse response = client.GetSurveyInfo(Request);
                if (response.SurveyInfoList.Length > 0)
                {
                    currentSurveyInfoDTO = response.SurveyInfoList[0];
                    if (currentSurveyInfoDTO.IsDraftMode)
                    {
                        this.lblPublishModeStatus.Text = "DRAFT";
                    }
                    else
                    {
                        this.lblPublishModeStatus.Text = "FINAL";
                    }

                        dtpSurveyClosingDate.Value = currentSurveyInfoDTO.ClosingDate;

                        StartDateDatePicker.Value = currentSurveyInfoDTO.StartDate;
                        DateTime CloseTime = Convert.ToDateTime(currentSurveyInfoDTO.ClosingDate);
                        DateTime StartTime = Convert.ToDateTime(currentSurveyInfoDTO.StartDate);

                        ClosingTimecomboBox.SelectedItem = CloseTime.ToShortTimeString();
                        StartTimecomboBox.SelectedItem = StartTime.ToShortTimeString();

                        txtSurveyName.Text = currentSurveyInfoDTO.SurveyName;
                        //txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        //txtSurveyNameMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        txtSurveyID.Text = currentSurveyInfoDTO.SurveyNumber;
                        //txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        //txtSurveyIDMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        txtSurveyIDMirror.Text = currentSurveyInfoDTO.SurveyNumber;
                        txtOrganization.Text = currentSurveyInfoDTO.OrganizationName;
                        //txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        //txtOrganizationMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        txtOrganizationMirror.Text = currentSurveyInfoDTO.OrganizationName;
                        txtDepartment.Text = currentSurveyInfoDTO.DepartmentName;
                        //txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        //txtDepartmentMirror.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        txtDepartmentMirror.Text = currentSurveyInfoDTO.DepartmentName;
                        txtIntroductionText.Text = currentSurveyInfoDTO.IntroductionText;
                        //txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        txtExitText.Text = currentSurveyInfoDTO.ExitText;
                        //txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                        
                        btnPublishForm.Enabled = true;
                    }
                //}
            }

        }

        private void txtSurveyName_Enter(object sender, EventArgs e)
        {
            if (txtSurveyName.Text == SharedStrings.WEBFORM_TITLE)
            {
                txtSurveyName.Text = string.Empty;
                txtSurveyName.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtSurveyName_Leave(object sender, EventArgs e)
        {
            if (txtSurveyName.Text == string.Empty)
            {
                txtSurveyName.Text = SharedStrings.WEBFORM_TITLE;
                txtSurveyName.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }

        private void txtSurveyID_Enter(object sender, EventArgs e)
        {
            if (txtSurveyID.Text == SharedStrings.WEBFORM_SURVEYID)
            {
                txtSurveyID.Text = string.Empty;
                txtSurveyID.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtSurveyID_Leave(object sender, EventArgs e)
        {
            if (txtSurveyID.Text == string.Empty)
            {
                txtSurveyID.Text = SharedStrings.WEBFORM_SURVEYID;
                txtSurveyID.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }

        private void txtOrganization_Enter(object sender, EventArgs e)
        {
            if (txtOrganization.Text == SharedStrings.WEBFORM_ORGANIZATION)
            {
                txtOrganization.Text = string.Empty;
                txtOrganization.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtOrganization_Leave(object sender, EventArgs e)
        {
            if (txtOrganization.Text == string.Empty)
            {
                txtOrganization.Text = SharedStrings.WEBFORM_ORGANIZATION;
                txtOrganization.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }
        
        private void txtDepartment_Enter(object sender, EventArgs e)
        {
            if (txtDepartment.Text == SharedStrings.WEBFORM_DEPARTMENT)
            {
                txtDepartment.Text = string.Empty;
                txtDepartment.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtDepartment_Leave(object sender, EventArgs e)
        {
            if (txtDepartment.Text == string.Empty)
            {
                txtDepartment.Text = SharedStrings.WEBFORM_DEPARTMENT;
                txtDepartment.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }
        
        private void txtIntroductionText_Enter(object sender, EventArgs e)
        {
            if (txtIntroductionText.Text == SharedStrings.WEBFORM_INTRODUCTIONTEXT)
            {
                txtIntroductionText.Text = string.Empty;
                txtIntroductionText.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtIntroductionText_Leave(object sender, EventArgs e)
        {
            if (txtIntroductionText.Text == string.Empty)
            {
                txtIntroductionText.Text = SharedStrings.WEBFORM_INTRODUCTIONTEXT;
                txtIntroductionText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }

        private void txtExitText_Enter(object sender, EventArgs e)
        {
            if (txtExitText.Text == SharedStrings.WEBFORM_EXITTEXT)
            {
                txtExitText.Text = string.Empty;
                txtExitText.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void txtExitText_Leave(object sender, EventArgs e)
        {
            if (txtExitText.Text == string.Empty)
            {
                txtExitText.Text = SharedStrings.WEBFORM_EXITTEXT;
                txtExitText.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
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
            else if (dtpSurveyClosingDate.Value + new TimeSpan(0, 23, 59, 59) <= DateTime.Now)
            {
                MsgBox.ShowError(SharedStrings.WEBFORM_FUTUREDATE);
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            }
            //--Ei-82
            else if (ValidateStartDate() == false)
            {
                MsgBox.ShowError(string.Format(SharedStrings.WEBFORM_STARTDATE, StartDateDatePicker.Value.ToShortDateString().ToString(), dtpSurveyClosingDate.Value.ToShortDateString().ToString()));
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
                this.StartDateDatePicker.Focus();
            }
            //--
            else if (txtSurveyName.Text.Equals(string.Empty) || txtSurveyName.Text.Equals(SharedStrings.WEBFORM_TITLE))
            {
                MsgBox.ShowError(SharedStrings.WEBFORM_GIVETITLE);
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
                txtSurveyName.SelectAll();
            }
            else if (txtExitText.Text.Equals(string.Empty) || txtExitText.Text.Equals(SharedStrings.WEBFORM_EXITTEXT))
            {
                OkToPublish = MsgBox.ShowQuestion(SharedStrings.WEBFORM_PUBWOEXIT);
                if (OkToPublish.Equals(DialogResult.No))
                {
                    this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[1];
                    this.txtExitText.SelectAll();
                }
            }
            else
            {
                OkToPublish = DialogResult.Yes;
            }


            if (OkToPublish.Equals(DialogResult.Yes))
            {
                if (!this.isRepublishableConfig || string.IsNullOrWhiteSpace(this.view.WebSurveyId))
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
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            txtSurveyName.SelectAll();
        }

        private void btnKeyCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtSurveyKey.Text);
        }

        private void txtSurveyIDMirror_MouseClick(object sender, MouseEventArgs e)
        {
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            txtSurveyID.SelectAll();
        }

        private void txtOrganizationMirror_MouseClick(object sender, MouseEventArgs e)
        {
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            txtOrganization.SelectAll();
        }

        private void txtDepartmentMirror_MouseClick(object sender, MouseEventArgs e)
        {
            this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[0];
            txtDepartment.SelectAll();
        }

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
            stopwatch.Stop();

            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Publishing complete. Time elapsed: " + stopwatch.Elapsed.ToString());

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
            SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                SurveyManagerService.PublishRequest Request = (SurveyManagerService.PublishRequest)((object[])e.Argument)[0];
                SurveyManagerService.PublishResponse Result = (SurveyManagerService.PublishResponse)((object[])e.Argument)[1];
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

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.tabPublishWebForm.SelectedIndex < 2)
            {
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[this.tabPublishWebForm.SelectedIndex + 1];
                if (this.tabPublishWebForm.SelectedIndex.Equals(1))
                {
                    this.tabExit.Focus();
                }
            }
            if (this.tabPublishWebForm.SelectedIndex.Equals(2))
            {
                btnNext.Enabled = false;
            }
            btnPrevious.Enabled = true;
        }

        private void tabIntro_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = true;
        }

        private void tabExit_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = true;
        }

        private void tabEntry_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
        }

        private void tabPublishWebForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = (this.tabPublishWebForm.SelectedIndex.Equals(2)) ? false : true ;
        }

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

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebSurveyOptions wso = new WebSurveyOptions();
            wso.Show();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (this.tabPublishWebForm.SelectedIndex > 0)
            {
                this.tabPublishWebForm.SelectedTab = this.tabPublishWebForm.TabPages[this.tabPublishWebForm.SelectedIndex - 1];
                if (this.tabPublishWebForm.SelectedIndex.Equals(1))
                {
                    this.tabIntro.Focus();
                }
            }

            if (this.tabPublishWebForm.SelectedIndex.Equals(0))
            {
                btnPrevious.Enabled = false;
            }
            btnNext.Enabled = true;
        }

        private DateTime GetdateTimeFormat(DateTime dateTime , string time) 
        {
            string dateTimeString = dateTime.ToShortDateString();
            
            return Convert.ToDateTime(dateTimeString + " " + time);
        }

        //--EI-82
        private bool ValidateStartDate()
        {

           DateTime StartDate = GetdateTimeFormat(StartDateDatePicker.Value.Date, StartTimecomboBox.Text);
           DateTime CloseDate = GetdateTimeFormat(dtpSurveyClosingDate.Value.Date, ClosingTimecomboBox.Text); 
           if (StartDate <= CloseDate) 
           {
               return true;
           }
           else
           {
               return false;
           }
           
        }
    }
}
