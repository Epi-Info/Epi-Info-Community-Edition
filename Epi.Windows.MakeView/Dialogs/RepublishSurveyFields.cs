using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Security;
using Epi.Web.Common.Exception;
using Epi.Web.Common.Message;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class RepublishSurveyFields : Form
    {

        private Guid UserPublishGuid;
        private View view = null;
        private string OrganizationKey = null;
        private string template = null;
        private SurveyManagerService.SurveyInfoDTO currentSurveyInfoDTO;

        public RepublishSurveyFields(string pOrganizationKey, Epi.View pView, string pTemplate)
        {
            InitializeComponent();

            this.OrganizationKey = pOrganizationKey;
            this.view = pView;
            this.template = pTemplate;
        }


        /// <summary>
        /// Initiates a single form publishing process
        /// </summary>
        private void DoRePublish()
        {

            if (this.UserPublishGuid == null)
            {
                this.UserPublishGuid = new Guid(this.txtPublisherKey.Text);
            }


            if(string.IsNullOrWhiteSpace(this.OrganizationKey))
            {
                this.OrganizationKey = this.txtOrganizationKey.Text;
            }

            this.QueryForExistingSurveyInfo();

            if (this.UserPublishGuid != null)
            {
                SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();
                Request.Action = "Update";

                this.currentSurveyInfoDTO.XML = template;

                Request.SurveyInfoList = new SurveyManagerService.SurveyInfoDTO[]{this.currentSurveyInfoDTO};
                try
                {
                    Epi.Web.Common.Message.SurveyInfoResponse Result = new Epi.Web.Common.Message.SurveyInfoResponse();

                    //txtStatusSummary.Text = SharedStrings.WEBFORM_SUCCESS;
                    //txtStatusSummary.Text = "Your Form has been republished: " + Result.Message;
                    ////string message = DateTime.Now + ": " + SharedStrings.WEBFORM_SUCCESS + ": " + Result.PublishInfo.URL;
                    //Logger.Log(message);
                    //message = DateTime.Now + ": Survey Key= " + txtSurveyKey.Text;
                    //Logger.Log(message);
                    //message = DateTime.Now + ": Data Key= " + txtDataKey.Text;
                    //Logger.Log(message);
                }
                catch (Exception ex)
                {
                    //txtStatusSummary.AppendText("An error occurred while trying to publish the survey.");
                    //txtStatus.AppendText(ex.ToString());
                    //btnDetails.Visible = true;
                    //this.progressBar.Visible = false;
                    //this.Cursor = Cursors.Default;
                }
            }
        }

        private void UpdateSurveyFields_Load(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(this.OrganizationKey))
            {
                QueryForExistingSurveyInfo();
            }

            if (this.currentSurveyInfoDTO != null)
            {
                if (this.currentSurveyInfoDTO.IsDraftMode)
                {
                    this.lblModeDisplay.Text = "Draft - the survey is currently in test mode.";
                }
                else
                {
                    this.lblModeDisplay.Text = "Final - the survey is currently in production mode.";
                }
            }

            this.txtOrganizationKey.Text = this.OrganizationKey;
            //this.txtPublisherKey.Text = this.UserPublishGuid.ToString();

        }

        private void QueryForExistingSurveyInfo()
        {
        SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
            SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();
            Request.Criteria.SurveyIdList = new string[]{this.view.WebSurveyId};
            Request.Criteria.OrganizationKey = new Guid(this.OrganizationKey);
            SurveyManagerService.SurveyInfoResponse response = client.GetSurveyInfo(Request);
            
            if (response.SurveyInfoList.Length > 0)
            {
                this.currentSurveyInfoDTO = response.SurveyInfoList[0];
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();
            Request.Action = "Update";

            this.currentSurveyInfoDTO.XML = this.template;
            Request.SurveyInfoList = new SurveyManagerService.SurveyInfoDTO[] { this.currentSurveyInfoDTO };

            try
            {
                Epi.Web.Common.Message.SurveyInfoResponse Result = new Epi.Web.Common.Message.SurveyInfoResponse();

                if (Result.Acknowledge == Web.Common.MessageBase.AcknowledgeType.Success)
                {
                    //txtStatusSummary.Text = SharedStrings.WEBFORM_SUCCESS;
                    lblOutput.Text = "Your Form has been republished: " + Result.Message;
                    ////string message = DateTime.Now + ": " + SharedStrings.WEBFORM_SUCCESS + ": " + Result.PublishInfo.URL;
                    //Logger.Log(message);
                    //message = DateTime.Now + ": Survey Key= " + txtSurveyKey.Text;
                    //Logger.Log(message);
                    //message = DateTime.Now + ": Data Key= " + txtDataKey.Text;
                    //Logger.Log(message);
                }
                else
                {
                    lblOutput.Text = "Your Form has NOT been republished: " + Result.Message;
                }

                //this.progressBar.Visible = false;
                //this.btnPublish.Enabled = true;

            }
            catch (Exception ex)
            {
                lblOutput.Text = lblOutput.Text + "An error occurred while trying to publish the survey.";
                lblOutput.Text = lblOutput.Text + ex.ToString();
                //btnDetails.Visible = true;
                //this.progressBar.Visible = false;
                this.Cursor = Cursors.Default;
            }
        }
    }
}

