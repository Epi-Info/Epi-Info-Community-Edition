using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Enter.Forms
    {
    public partial class UserAuthentication : Form
        {
        public UserAuthentication()
            {
            InitializeComponent();
            }

        private void LoginButton_Click(object sender, EventArgs e )
            {
            string errorMessage;
            if (!ValidEmailAddress(EmailAddresstextBox1.Text, out errorMessage))
                {
                this.errorProvider1.SetError(EmailAddresstextBox1, errorMessage);

                }
            else if(string.IsNullOrEmpty(this.PassWordTextBox1.Text) ) 
                {
                this.errorProvider1.SetError(PassWordTextBox1, SharedStrings.WEBENTER_PW_REQUIRED);
                }
            else if (!ValidateUser(out errorMessage))
                {
                    if (errorMessage.Contains("Service communication Error"))
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.No;
                }
                else
                {
                 this.errorProvider1.SetError(PassWordTextBox1, SharedStrings.WEBENTER_LOGIN_AGAIN);
                this.errorProvider1.SetError(EmailAddresstextBox1, SharedStrings.WEBENTER_LOGIN_AGAIN);
                
                }
                }
            else
                {
                 this.errorProvider1.SetError(EmailAddresstextBox1, "");
                 this.errorProvider1.SetError(PassWordTextBox1, "");
                 this.DialogResult = System.Windows.Forms.DialogResult.OK;
                
                }
            
            }

        private bool ValidateUser( out string message  )
        {
            message = "";
           
            EWEManagerService.UserAuthenticationRequest Request = new EWEManagerService.UserAuthenticationRequest();
            var rUser = new EWEManagerService.UserDTO();
            
            Request.User = rUser;
            Request.User.Operation = EWEManagerService.ConstantOperationMode.NoChange;
            Request.User.EmailAddress = this.EmailAddresstextBox1.Text;
            Request.User.UserName = this.EmailAddresstextBox1.Text;
            Request.User.PasswordHash = this.PassWordTextBox1.Text;

            var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();

            try
            {
                var Result = client.UserLogin(Request);

                if (Result.User != null)
                {
                    Epi.Windows.Enter.LoginInfo.UserID = Result.User.UserId;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "Service communication Error";
                return false;
            }
        }

        public bool ValidEmailAddress(string emailAddress, out string errorMessage)
            {
            // Confirm that the e-mail address string is not empty. 
            if (emailAddress.Length == 0)
                {
                errorMessage = SharedStrings.WEBENTER_EMAIL_REQUIRED;
                return false;
                }

            // Confirm that there is an "@" and a "." in the e-mail address, and in the correct order.
            if (emailAddress.IndexOf("@") > -1)
                {
                if (emailAddress.IndexOf(".", emailAddress.IndexOf("@")) > emailAddress.IndexOf("@"))
                    {
                    errorMessage = String.Empty;
                    return true;
                    }
                }

                errorMessage = SharedStrings.WEBENTER_EMAIL_FORMAT;
               
            return false;
            }

        }
    }
