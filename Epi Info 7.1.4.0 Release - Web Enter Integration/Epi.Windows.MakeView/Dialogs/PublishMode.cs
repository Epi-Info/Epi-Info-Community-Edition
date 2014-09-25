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

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class PublishMode : Form
    {
        private delegate void PublishDelegate(Epi.Web.Common.Message.PublishResponse Result);
        private BackgroundWorker SurveyInfoWorker;
        private BackgroundWorker UpdateWorker;
        
        private Stopwatch stopwatch;
        private List<Epi.Web.Common.DTO.SurveyInfoDTO> SurveyInfoList;
        private static object syncLock = new object();
        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);


            private string  SurveyName = "";
            private string DepartmentName= "";
            private string SurveyNumber  = "";
            private string OrganizationName= "";
            private string OrganizationKey = "";
            private DateTime StartDate  ;
            private DateTime CloseDate  ;
            private bool IsSingleResponse;
            private bool IsDraftMode ;
            private string IntroductionText ;
            private string ExitText;
            private string TemplateXML;
            private string UserPublishKey;
            private int SurveyType;
            private string SurveyId ;



        public PublishMode(string pSurveId, string pOrganizationKey, string pUserPublishKey = null)
        {
            InitializeComponent();
            this.SurveyId = pSurveId;
            this.OrganizationKey = pOrganizationKey;
            this.UserPublishKey = pUserPublishKey;
           
             
        }

        
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //stopwatch.Stop();

           /* this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Publishing complete. Time elapsed: " + stopwatch.Elapsed.ToString());

            btnCancel.Enabled = true;
            btnPublish.Enabled = true;  //Ok button
            btnPublishForm.Visible = false;
            progressBar.Visible = false;

            publishFinished = true;*/
            if (IsDraftMode)
            {
                DraftRadioButton.Checked = true;
            }
            else
            {
                FinalRadioButton.Checked = true;
            } 
            this.Cursor = Cursors.Default;
        }
        private void UpDateButton_Click(object sender, EventArgs e)
        {
            try
            {
                DoUpDate();
                MessageBox.Show("The publish mode was successfully changed ", "My Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                this.Close();
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

            }
            catch (TimeoutException te)
            {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

            }
        }
        private void SurveyInfoworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                SurveyManagerService.ManagerServiceClient client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();
                
                SurveyManagerService.SurveyInfoRequest Request = (SurveyManagerService.SurveyInfoRequest)((object[])e.Argument)[0];
                SurveyManagerService.SurveyInfoResponse Result = (SurveyManagerService.SurveyInfoResponse)((object[])e.Argument)[1];
            
                Result = client.GetSurveyInfo(Request);
                
                if (Result != null && Result.SurveyInfoList.Length > 0) 
                { 
                      SurveyName = Result.SurveyInfoList[0].SurveyName;
                      DepartmentName= Result.SurveyInfoList[0].DepartmentName;
                      SurveyNumber = Result.SurveyInfoList[0].SurveyNumber;
                      OrganizationName = Result.SurveyInfoList[0].OrganizationName;
                      StartDate = Result.SurveyInfoList[0].StartDate;
                      CloseDate = Result.SurveyInfoList[0].ClosingDate;
                      IsDraftMode = Result.SurveyInfoList[0].IsDraftMode;
                      IntroductionText = Result.SurveyInfoList[0].IntroductionText;
                      ExitText = Result.SurveyInfoList[0].ExitText;
                      TemplateXML = Result.SurveyInfoList[0].XML;
                      OrganizationKey =  Result.SurveyInfoList[0].OrganizationKey.ToString();
                      SurveyType = Result.SurveyInfoList[0].SurveyType;
                      UserPublishKey = Result.SurveyInfoList[0].UserPublishKey.ToString();
                      //SurveyId = Result.SurveyInfoList[0].SurveyId;
                 }
            }
            catch (FaultException<CustomFaultException> cfe)
            {
               // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);
              
            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);
                
            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);
             
            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);
                
            }
            catch (TimeoutException te)
            {
               // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);
                
            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);
                 
            }
        }
        private void UpdateSurveyInfoworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                SurveyManagerService.ManagerServiceClient client;
                Configuration config = Configuration.GetNewInstance();
                client = GetClient();
                SurveyManagerService.SurveyInfoRequest Request = (SurveyManagerService.SurveyInfoRequest)((object[])e.Argument)[0];
                SurveyManagerService.SurveyInfoResponse Result = (SurveyManagerService.SurveyInfoResponse)((object[])e.Argument)[1];

                Request.Criteria.ClosingDate =this.CloseDate;
                Request.Criteria.OrganizationKey =  new Guid(this.OrganizationKey);
                Request.Criteria.UserPublishKey = new Guid(this.UserPublishKey);
                Request.Criteria.SurveyIdList = new string[]{this.SurveyId};
                Request.Action = "Update";

                SurveyManagerService.SurveyInfoDTO SurveyInfoDTO = new SurveyManagerService.SurveyInfoDTO();

                SurveyInfoDTO.ClosingDate = this.CloseDate;
                SurveyInfoDTO.StartDate = this.StartDate;
                SurveyInfoDTO.SurveyId = new Guid(this.SurveyId).ToString();
                SurveyInfoDTO.SurveyType = this.SurveyType;
                SurveyInfoDTO.SurveyNumber = this.SurveyNumber;
                SurveyInfoDTO.SurveyName = this.SurveyName;
              //  SurveyInfoDTO.OrganizationKey = new Guid(this.OrganizationKey);
                SurveyInfoDTO.OrganizationKey = new Guid(this.OrgKeytextBox.Text.ToString());
                SurveyInfoDTO.UserPublishKey = new Guid(this.SecurityKeytextBox.Text.ToString());
                SurveyInfoDTO.OrganizationName = this.OrganizationName;
                SurveyInfoDTO.XML = this.TemplateXML;
                SurveyInfoDTO.ExitText = this.ExitText;
                SurveyInfoDTO.IntroductionText = this.IntroductionText;
                SurveyInfoDTO.DepartmentName = this.DepartmentName;

                Request.Criteria.SurveyType = this.SurveyType;

                if (this.DraftRadioButton.Checked)
                {
                    Request.Criteria.IsDraftMode = true;
                    SurveyInfoDTO.IsDraftMode = true;
                }
                else {
                    Request.Criteria.IsDraftMode = false;
                    SurveyInfoDTO.IsDraftMode = false;
                }
                 Request.SurveyInfoList = new SurveyManagerService.SurveyInfoDTO[]{SurveyInfoDTO};
               
                Result = client.SetSurveyInfo(Request);

               
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

            }
            catch (TimeoutException te)
            {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

            }
        }

        public SurveyManagerService.ManagerServiceClient GetClient() {

            SurveyManagerService.ManagerServiceClient client;
            Configuration config = Configuration.GetNewInstance();

            if (config.Settings.WebServiceAuthMode == 1) // Windows Authentication
            {
                System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                binding.Name = "BasicHttpBinding";
                binding.CloseTimeout = new TimeSpan(0, 1, 0);
                binding.OpenTimeout = new TimeSpan(0, 1, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                binding.SendTimeout = new TimeSpan(0, 1, 0);
                binding.AllowCookies = false;
                binding.BypassProxyOnLocal = false;
                binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                binding.TextEncoding = System.Text.Encoding.UTF8;
                binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                binding.UseDefaultWebProxy = true;
                binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                binding.Security.Transport.Realm = string.Empty;

                binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

                System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.WebServiceEndpointAddress);

                client = new SurveyManagerService.ManagerServiceClient(binding, endpoint);

                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                client.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
            }
            else
            {


                if (config.Settings.WebServiceBindingMode.Equals("wshttp", StringComparison.OrdinalIgnoreCase))
                {
                    System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                    binding.Name = "WSHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 1, 0);
                    binding.OpenTimeout = new TimeSpan(0, 1, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 1, 0);
                    binding.BypassProxyOnLocal = false;
                    binding.TransactionFlow = false;
                    binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                    binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.UseDefaultWebProxy = true;
                    binding.AllowCookies = false;

                    binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                    binding.ReliableSession.Ordered = true;
                    binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
                    binding.ReliableSession.Enabled = false;

                    binding.Security.Mode = System.ServiceModel.SecurityMode.Message;
                    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                    binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                    binding.Security.Transport.Realm = string.Empty;
                    binding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
                    binding.Security.Message.NegotiateServiceCredential = true;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.WebServiceEndpointAddress);

                    client = new SurveyManagerService.ManagerServiceClient(binding, endpoint);

                }
                else
                {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 1, 0);
                    binding.OpenTimeout = new TimeSpan(0, 1, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 1, 0);
                    binding.AllowCookies = false;
                    binding.BypassProxyOnLocal = false;
                    binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                    binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    binding.UseDefaultWebProxy = true;
                    binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;


                    binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                    binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                    binding.Security.Transport.Realm = string.Empty;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.WebServiceEndpointAddress);

                    client = new SurveyManagerService.ManagerServiceClient(binding, endpoint);
                }

            }

            return client;
        }

        private void AfterPublish(Epi.Web.Common.Message.PublishResponse Result)
        {
            /* if (Result.PublishInfo.IsPulished)
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
                 btnPublishForm.Visible = false;
                 btnPublish.Enabled = true; //OK button
                 btnURLCopy.Enabled = true;
                 btnGo.Enabled = true;
                 btnKeyCopy.Enabled = true;
                 btnDataKeyCopy.Enabled = true;
                 btnShowLog.Enabled = true;
                 btnCopyAllURLs.Enabled = true;

                 txtSurveyName.Enabled = true;
                 txtSurveyID.Enabled = true;
                 txtOrganization.Enabled = true;
                 txtDepartment.Enabled = true;
                 txtIntroductionText.Enabled = true;
                 txtExitText.Enabled = true;
                 dtpSurveyClosingDate.Enabled = true;
                 txtOrganizationKey.Enabled = true;
                 txtOrganizationKey.Clear();
                 btnClear.Enabled = true;

             }
             else
             {
                 txtStatusSummary.Text = "The survey failed to publish. Check that the organization key is correct and try again.";
             }
             txtStatus.AppendText(Environment.NewLine);

         }*/
        }

        private void DoGetSurveyInfo()
        {
           
            try
            {

                Epi.Web.Common.Message.SurveyInfoRequest Request = new Epi.Web.Common.Message.SurveyInfoRequest();
             
                //Request.Criteria.OrganizationKey = new Guid(this.OrganizationKey);
                Request.Criteria.OrganizationKey = new Guid(this.OrgKeytextBox.Text.ToString());
                //Request.Criteria.UserPublishKey = new Guid(this.UserPublishKey);
                Request.Criteria.ReturnSizeInfoOnly = false;
                
                Request.Criteria.SurveyIdList.Add(this.SurveyId);
                lock (syncLock)
                {
                    Epi.Web.Common.Message.SurveyInfoResponse Result = new Epi.Web.Common.Message.SurveyInfoResponse();
                    this.Cursor = Cursors.WaitCursor;
                    SurveyInfoWorker = new BackgroundWorker();
                    SurveyInfoWorker.WorkerSupportsCancellation = true;
                    SurveyInfoWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(SurveyInfoworker_DoWork);

                    SurveyInfoWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    object[] args = new object[2];
                    args[0] = Request;
                    args[1] = Result;
                    SurveyInfoWorker.RunWorkerAsync(args);
                }
              
            }
            catch (FaultException<CustomFaultException> cfe)
            {
              //  SurveyInfoResponseTextBox.AppendText("FaultException<CustomFaultException>:\n");
               /// SurveyInfoResponseTextBox.AppendText(cfe.ToString());
            }
            catch (FaultException fe)
            {
               // SurveyInfoResponseTextBox.AppendText("FaultException:\n");
               // SurveyInfoResponseTextBox.AppendText(fe.ToString());
            }
            catch (CommunicationException ce)
            {
              //  SurveyInfoResponseTextBox.AppendText("CommunicationException:\n");
               // SurveyInfoResponseTextBox.AppendText(ce.ToString());
            }
            catch (TimeoutException te)
            {
               // SurveyInfoResponseTextBox.AppendText("TimeoutException:\n");
               // SurveyInfoResponseTextBox.AppendText(te.ToString());
            }
            catch (Exception ex)
            {
              //  SurveyInfoResponseTextBox.AppendText("Exception:\n");
              //  SurveyInfoResponseTextBox.AppendText(ex.ToString());
            }
        }
        private void DoUpDate()
        {

            try
            {

                Epi.Web.Common.Message.SurveyInfoRequest Request = new Epi.Web.Common.Message.SurveyInfoRequest();

                
                lock (syncLock)
                {
                    Epi.Web.Common.Message.SurveyInfoResponse Result = new Epi.Web.Common.Message.SurveyInfoResponse();
                    //this.Cursor = Cursors.WaitCursor;
                    UpdateWorker = new BackgroundWorker();
                    UpdateWorker.WorkerSupportsCancellation = true;
                    
                    UpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(UpdateSurveyInfoworker_DoWork);
                   // UpdateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    object[] args = new object[2];
                    args[0] = Request;
                    args[1] = Result;
                    UpdateWorker.RunWorkerAsync(args);
                }
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                //  SurveyInfoResponseTextBox.AppendText("FaultException<CustomFaultException>:\n");
                /// SurveyInfoResponseTextBox.AppendText(cfe.ToString());
            }
            catch (FaultException fe)
            {
                // SurveyInfoResponseTextBox.AppendText("FaultException:\n");
                // SurveyInfoResponseTextBox.AppendText(fe.ToString());
            }
            catch (CommunicationException ce)
            {
                //  SurveyInfoResponseTextBox.AppendText("CommunicationException:\n");
                // SurveyInfoResponseTextBox.AppendText(ce.ToString());
            }
            catch (TimeoutException te)
            {
                // SurveyInfoResponseTextBox.AppendText("TimeoutException:\n");
                // SurveyInfoResponseTextBox.AppendText(te.ToString());
            }
            catch (Exception ex)
            {
                //  SurveyInfoResponseTextBox.AppendText("Exception:\n");
                //  SurveyInfoResponseTextBox.AppendText(ex.ToString());
            }
        }

        
       
        public static bool IsGuid(string Guid)
        {
            bool isValid = false;


            if (Guid != null)
            {

                if (isGuid.IsMatch(Guid))
                {
                   
                    isValid = true;
                }
            }

            return isValid;
        }
         
      
        

        private void SecurityKeytextBox_TextChanged(object sender, EventArgs e)
       {

            //if (IsGuid(this.SecurityKeytextBox.Text.ToString()))
            //{

            //    this.UpDateButton.Enabled = true;
            //}
            //else
            //{

            //    this.UpDateButton.Enabled = false;

            //}
        }

        private void PublishMode_Load(object sender, EventArgs e)
        {
          
            if (string.IsNullOrEmpty(this.OrganizationKey))
            {
                // Epi.Windows.MakeView.Dialogs. inputDialog = new Epi.Windows.MakeView.Dialogs.InputGuidDialog("Enter organization key", "Organization Key", "", null, EpiInfo.Plugin.DataType.Text);
                //DialogResult result = inputDialog.ShowDialog();
                //if (result == System.Windows.Forms.DialogResult.OK)
                //{
                //    this.OrganizationKey = inputDialog.TextInput;
                //    OrgKeytextBox.Text = OrganizationKey;
                //    UpDateButton.Visible = true;
                //    this.DraftRadioButton.Visible = true;
                //    this.groupBox1.Visible = true;
                //    this.groupBox2.Visible = true;
                //    this.UpDateButton.Enabled = true;
                //    this.CancelButton.Visible = true;
                //    this.SecurityKey.Visible = true;
                //    this.SecurityKeytextBox.Visible = true;
                //    DoGetSurveyInfo();
                //}
                //else
                //{

                //    this.Close();
                //}
            }
            else
            {
                OrgKeytextBox.Text = OrganizationKey;
                UpDateButton.Visible = true;
                this.DraftRadioButton.Visible = true;
                this.groupBox1.Visible = true;
                this.groupBox2.Visible = true;
                this.UpDateButton.Enabled = true;
                this.CancelButton.Visible = true;
                this.SecurityKey.Visible = true;
                this.SecurityKeytextBox.Visible = true;
                DoGetSurveyInfo();
            
            }
        }

        
    }
}
