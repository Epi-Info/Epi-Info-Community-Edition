using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.MakeView.Dialogs
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
                this.errorProvider1.SetError(PassWordTextBox1, "Password is required.");
                }
            else if (!ValidateUser(out errorMessage))
                {
                this.errorProvider1.SetError(PassWordTextBox1, "Password or/and Email address are not valid.Please try again.");
                this.errorProvider1.SetError(EmailAddresstextBox1, "Password or/and Email address are not valid.Please try again.");
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
           
            Epi.Windows.MakeView.EWEManagerService.UserAuthenticationRequest Request = new Epi.Windows.MakeView.EWEManagerService.UserAuthenticationRequest();
            var rUser = new Epi.Windows.MakeView.EWEManagerService.UserDTO();
            
            Request.User = rUser;
            Request.User.Operation = Epi.Windows.MakeView.EWEManagerService.ConstantOperationMode.NoChange;
            Request.User.EmailAddress = this.EmailAddresstextBox1.Text;
            Request.User.UserName = this.EmailAddresstextBox1.Text;
            Request.User.PasswordHash = this.PassWordTextBox1.Text;
            
            EWEManagerService.EWEManagerServiceClient client = Epi.Windows.MakeView.Utils.EWEServiceClient.GetClient();

            try
            {
                var Result = client.UserLogin(Request);

                if (Result.User != null)
                {
                    LoginInfo.UserID = Result.User.UserId;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ValidEmailAddress(string emailAddress, out string errorMessage)
            {
            // Confirm that the e-mail address string is not empty. 
            if (emailAddress.Length == 0)
                {
                errorMessage = "e-mail address is required.";
                return false;
                }

            // Confirm that there is an "@" and a "." in the e-mail address, and in the correct order.
            if (emailAddress.IndexOf("@") > -1)
                {
                if (emailAddress.IndexOf(".", emailAddress.IndexOf("@")) > emailAddress.IndexOf("@"))
                    {
                    errorMessage = "";
                    return true;
                    }
                }

            errorMessage = "e-mail address must be valid e-mail address format.\n" +
               "For example 'someone@example.com' ";
            return false;
            }

        

        }
    }
