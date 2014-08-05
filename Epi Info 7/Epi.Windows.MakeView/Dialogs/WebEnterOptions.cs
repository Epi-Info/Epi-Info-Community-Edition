using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Epi.Web.Common.Security;



namespace Epi.Windows.MakeView.Dialogs
{
    public partial class WebEnterOptions : Form
    {
        private string WebServiceBindingMode;
        private int WebServiceAuthMode;
        private string WebServiceEndpointAddress;
        private Configuration config;
        public WebEnterOptions()
        {
            InitializeComponent();

            
            pnlError.Visible = false;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
           //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (this.WsHTTpRadioButton.Checked == true)
            {
                config.Settings.EWEServiceBindingMode = "WSHTTP";
            }
            else
            {
                config.Settings.EWEServiceBindingMode = "BASIC";
            }
            if(this.YesRadioButton.Checked == true)
            {
                 config.Settings.EWEServiceAuthMode  = 1;
            }
            else
            {
                config.Settings.EWEServiceAuthMode = 0;
            }
            config.Settings.EWEServiceEndpointAddress = this.EndPointTextBox.Text.ToString();

            Configuration.Save(config);
           
            if (ValidateSittings())
            {

                //this.pnlError.Visible = true;
                //this.pnlError.BackColor = System.Drawing.Color.LightGreen;
                //this.lblError.Text = " valid settings!";
                //this.lblError.ForeColor = System.Drawing.Color.Green;
                //this.CancelButton.Text = "Close";
          //  System.Windows.Forms = System.Windows.Forms.DialogResult.Cancel;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else 
            {
                //this.DialogResult = System.Windows.Forms.DialogResult.No;
                this.pnlError.Visible = true;
                this.pnlError.BackColor = System.Drawing.Color.FromArgb(243, 217, 217);
                this.lblError.ForeColor = System.Drawing.Color.FromArgb(123, 5, 15);
                this.lblError.Text = "One or more settings provided are invalid. Please enter the correct settings and try again.";
               
            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private bool ValidateSittings() 
        {
            bool IsValid = true;
          
            try
            { 
               
                EWEManagerService.EWEManagerServiceClient client = Epi.Windows.MakeView.Utils.EWEServiceClient.GetClient();
                Epi.Windows.MakeView.EWEManagerService.OrganizationRequest Request = new Epi.Windows.MakeView.EWEManagerService.OrganizationRequest();
                var Result = client.GetOrganization(Request);

            }
            catch (Exception ex)
            {
                IsValid = false;
            }
            return IsValid;
        }

        private void WebSurveyOptions_Load(object sender, EventArgs e)
        {
            config = Configuration.GetNewInstance();
            WebServiceBindingMode = config.Settings.EWEServiceBindingMode;
            WebServiceAuthMode = config.Settings.EWEServiceAuthMode;
            WebServiceEndpointAddress = config.Settings.EWEServiceEndpointAddress;

            this.EndPointTextBox.Text = WebServiceEndpointAddress;

            if (((WebServiceBindingMode).ToUpper()).Contains("BASIC"))
            {
                this.BasicRadioButton.Checked = true;
                this.WsHTTpRadioButton.Checked = false;
            }
            if (((WebServiceBindingMode).ToUpper()).Contains("WSHTTP"))
            {
                this.BasicRadioButton.Checked = false;
                this.WsHTTpRadioButton.Checked = true;
            }
            if (WebServiceAuthMode == 1)
            {
                this.YesRadioButton.Checked = true;
                this.NoRadioButton.Checked = false;
            }
            else 
            {
                this.YesRadioButton.Checked = false;
                this.NoRadioButton.Checked = true;
            }
        }


       
    }
}
