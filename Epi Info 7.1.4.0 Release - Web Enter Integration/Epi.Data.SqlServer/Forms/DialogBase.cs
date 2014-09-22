using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Data.SqlServer.Forms
{
    public partial class DialogBase : Form
    {
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public DialogBase()
        {
            InitializeComponent();
            errorMessages = new List<string>();
        }
        #endregion Constructors

        #region Private Attributes
        private List<string> errorMessages = null;
        #endregion Private Attributes

        #region Protected Properties

        /// <summary>
        /// Gets the list of error messages
        /// </summary>
        protected List<string> ErrorMessages
        {
            get
            {
                return errorMessages;
            }
        }

        #endregion Protected Properties

        #region Protected Methods

        /// <summary>
        /// Validates the input on the dialog
        /// </summary>
        /// <returns>Boolean</returns>
        protected virtual bool ValidateInput()
        {
            errorMessages.Clear();
            return (errorMessages.Count == 0); // Always returns true
        }
        /// <summary>
        /// Method to show error messages
        /// </summary>
        protected void ShowErrorMessages()
        {
            string errorMessagesString = string.Empty;
            foreach (string str in ErrorMessages)
            {
                if (!string.IsNullOrEmpty(errorMessagesString))
                {
                    errorMessagesString += Environment.NewLine;
                }
                //errorMessagesString += Localization.LocalizeString(str);
                errorMessagesString += str;
            }

            MessageBox.Show(errorMessagesString, "error"); // TODO: hardcoded string

            // Done with displaying error messages. Clear the list ..
            ErrorMessages.Clear();
        }
        #endregion Protected Methods
    }
}