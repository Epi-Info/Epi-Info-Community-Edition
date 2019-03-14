using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Data;
using Epi.Data.MySQL;
using MySql.Data.MySqlClient;
using MySql.Data;

using Epi.Windows.Dialogs;


namespace Epi.Data.MySQL.Forms
{
    /// <summary>
    /// Dialog for building a MySQL connection string
    /// </summary>
    public partial class ConnectionStringDialog : DialogBase, IConnectionStringGui 
    {

        #region Public Data Members

        #endregion  //Public Data Members

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionStringDialog()
        {
            
            InitializeComponent();
            this.cmbServerName.Items.Add("<Browse for more>");
            this.cmbServerName.Text = "";
            dbConnectionStringBuilder = new MySqlConnectionStringBuilder();
        }

        #endregion  //Constructors

        #region IConnectionStringBuilder Members

        public bool ShouldIgnoreNonExistance
        {
            set { }
        }

        public virtual void SetDatabaseName(string databaseName) { }
        public virtual void SetServerName(string serverName) { }
        public virtual void SetUserName(string userName) { }
        public virtual void SetPassword(string password) { }

        /// <summary>
        /// Module-level SqlDBConnectionString object member.
        /// </summary>
        protected MySqlConnectionStringBuilder dbConnectionStringBuilder;
        /// <summary>
        /// Gets or sets the SqlDBConnectionString Object
        /// </summary>
        public DbConnectionStringBuilder DbConnectionStringBuilder
        {
            get
            {
                return dbConnectionStringBuilder;
            }
            //set
            //{
            //    dbConnectionStringBuilder = (SqlConnectionStringBuilder)value;
            //}
        }

        
        /// <summary>
        /// Sets the preferred database name
        /// </summary>
        public virtual string PreferredDatabaseName
        {
            set
            {
                txtDatabaseName.Text = value;
            }
            get
            {
                return txtDatabaseName.Text;
            }
        }

        /// <summary>
        /// Gets a user friendly description of the connection string
        /// </summary>
        public virtual string ConnectionStringDescription
        {
            get
            {
                return cmbServerName.Text + "::" + txtDatabaseName.Text;
            }
        }

        /// <summary>
        /// Gets whether or not the user entered a password
        /// </summary>
        public bool UsesPassword
        {
            get
            {
                if (this.txtPassword.Text.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion  //IConnectionStringBuilder Members

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            OnOkClick();
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            OnCancelClick();
        }

        /// <summary>
        /// Handles the Change event of the Server Name's selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbServerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if last item in list (Browse for Servers)
            if (cmbServerName.SelectedIndex == (cmbServerName.Items.Count - 1))
            {
                string serverName = BrowseForServers.BrowseNetworkServers();
                if (!string.IsNullOrEmpty(serverName))
                {
                    this.cmbServerName.Items.Insert(0, serverName);
                    this.cmbServerName.SelectedIndex = 0;
                    return;
                }

                this.cmbServerName.SelectedText = string.Empty;
            }
        }
        #endregion  Event Handlers

        #region Private Methods
         
        #endregion  //Private Methods

        #region Protected Methods

        /// <summary>
        /// Validates if all the necessary input is provided.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            // Server name
            if (Util.IsEmpty(this.cmbServerName.Text.Trim()))
            {
                ErrorMessages.Add("Server name is required"); // TODO: Hard coded string
            }

            // Database name
            string databaseName = txtDatabaseName.Text.Trim();
            Epi.Validator.ValidateDatabaseName(databaseName, ErrorMessages);

            // User Id            
            if (Util.IsEmpty(this.txtUserName.Text.Trim()))
            {
                ErrorMessages.Add("User name is required"); // TODO: Hard coded string
            }

            // Password
            if (Util.IsEmpty(txtPassword.Text.Trim()))
            {
                ErrorMessages.Add("Password is required"); // TODO: Hard coded string
            }

            if (!string.IsNullOrEmpty(txtPort.Text))
            {
                uint result;
                if (!uint.TryParse(txtPort.Text, out result))
                {
                    ErrorMessages.Add("Invalid port");
                }
            }

            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Occurs when cancel is clicked
        /// </summary>
        protected void OnCancelClick()
        {
            this.dbConnectionStringBuilder.ConnectionString = string.Empty ;
            this.PreferredDatabaseName = string.Empty; 

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Occurs when OK is clicked
        /// </summary>
        protected virtual void OnOkClick()
        {
            if (ValidateInput())
            {
                dbConnectionStringBuilder = new MySqlConnectionStringBuilder();
                dbConnectionStringBuilder.Database = txtDatabaseName.Text.Trim();
                dbConnectionStringBuilder.Server = cmbServerName.Text.Trim();
                dbConnectionStringBuilder.UserID = txtUserName.Text.Trim();
                dbConnectionStringBuilder.Password = txtPassword.Text.Trim();
                if (!string.IsNullOrEmpty(txtPort.Text))
                {
                    dbConnectionStringBuilder.Port = uint.Parse(txtPort.Text);
                }
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                ShowErrorMessages();
            }
        }
        #endregion  Protected Methods

    }
}