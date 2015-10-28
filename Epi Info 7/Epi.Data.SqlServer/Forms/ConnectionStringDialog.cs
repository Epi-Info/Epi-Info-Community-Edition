#region Namespaces

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

#endregion

namespace Epi.Data.SqlServer.Forms
{
    /// <summary>
    /// Dialog for building a SQL Server connection string
    /// </summary>
    public partial class ConnectionStringDialog : DialogBase, IConnectionStringGui  // Form,
    {

        #region Public Data Members

        ///// <summary>
        ///// Sql server connection string
        ///// </summary>
        //public string connectionString;

        #endregion  //Public Data Members

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionStringDialog()
        {
            InitializeComponent();
            this.cmbServerName.Items.Add(SharedStrings.BROWSE_FOR_MORE);
            this.cmbServerName.Items.Add("(local)");
            this.cmbServerName.Text = "";
            dbConnectionStringBuilder = new SqlConnectionStringBuilder();
        }

        #endregion

        #region IConnectionStringBuilder Members

        public bool ShouldIgnoreNonExistance
        {
            set { }
        }

        public virtual void SetDatabaseName(string databaseName)
        {
            this.cmbDatabaseName.Text = databaseName;
        }

        public virtual void SetServerName(string serverName)
        {
            this.cmbServerName.Text = serverName;
        }

        public virtual void SetUserName(string userName)
        {
            txtUserName.Text = userName;
        }

        public virtual void SetPassword(string password)
        {
            txtPassword.Text = password;
        }

        /// <summary>
        /// 
        /// </summary>
        protected SqlConnectionStringBuilder dbConnectionStringBuilder;
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
                cmbDatabaseName.Text = value;
            }
            get
            {
                return cmbDatabaseName.Text;
            }
        }

        /// <summary>
        /// Gets a user friendly description of the connection string
        /// </summary>
        public virtual string ConnectionStringDescription
        {
            get
            {
                return cmbServerName.Text + "::" + cmbDatabaseName.Text;
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
        /// Class the OnAuthenticationCheckChanged method
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void rdbAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            lblPassword.Enabled = rdbSqlAuthentication.Checked;
            lblUserName.Enabled = rdbSqlAuthentication.Checked;
            txtPassword.Enabled = rdbSqlAuthentication.Checked;
            txtUserName.Enabled = rdbSqlAuthentication.Checked;
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
            if ((string)cmbServerName.SelectedItem == SharedStrings.BROWSE_FOR_MORE)
            {
                string serverName = BrowseForServers.BrowseNetworkServers().ToString();
                if (!string.IsNullOrEmpty(serverName))
                {                   
                    this.cmbServerName.Items.Insert(0, serverName);
                    this.cmbServerName.SelectedIndex = 0;

                    List<String> databases = new List<String>();               
                    using (var con = new SqlConnection("Data Source=" + serverName + ";Initial Catalog=master;Trusted_Connection=yes"))
                    {                        
                            con.Open();                       
                        DataTable Databases = con.GetSchema("Databases");
                        con.Close();
                        foreach (DataRow databas in Databases.Rows)
                        {
                           databases.Add(databas.Field<String>("database_name"));
                        }
                    }
                    if (databases.Count>0)
                    cmbDatabaseName.DataSource = databases;


                  /*  string database = this.cmbDatabaseName.Text;
                    Epi.Data.SqlServer.SqlDatabase db = new SqlDatabase();

                    dbConnectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                    dbConnectionStringBuilder.DataSource = cmbServerName.Text;
                    dbConnectionStringBuilder.UserID = txtUserName.Text;
                    if (rdbWindowsAuthentication.Checked)
                    {
                        dbConnectionStringBuilder.IntegratedSecurity = true;
                    }
                    else
                    {
                        dbConnectionStringBuilder.UserID = txtUserName.Text;
                        dbConnectionStringBuilder.Password = txtPassword.Text;
                    }

                    db.ConnectionString = this.DbConnectionStringBuilder.ToString();

                    try
                    {
                        if (db.TestConnection())
                        {
                            this.cmbDatabaseName.Text = string.Empty;
                            List<string> databaseNamesList = db.GetDatabaseNameList();
                            cmbDatabaseName.DataSource = databaseNamesList;
                        }
                        else
                        {
                            MessageBox.Show("Connection failed."); // TODO: hard coded string.
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection failed: " + ex.Message); // TODO: hard coded string
                    }*/

                    return;
                }

                this.cmbServerName.SelectedText = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the Test button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void btnTest_Click(object sender, EventArgs e)
        {
            //string database = this.cmbDatabaseName.Text;

            //Epi.Data.SqlServer.SqlDatabase db = new SqlDatabase();

            //db.ConnectionString = this.DbConnectionStringBuilder.ToString();    //connectionString;

            //try
            //{
            //    if (db.TestConnection())
            //    {
            //        MessageBox.Show("Success!");

            //        this.cmbDatabaseName.Items.Clear();
            //        List<string> databaseNamesList = db.GetDatabaseNameList();
            //        foreach (string name in databaseNamesList)
            //        {
            //            this.cmbDatabaseName.Items.Add(name);
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Connection failed.");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Connection failed: " + ex.Message);
            //}
        }
        
        #endregion  Event Handlers

        #region Private Methods
        #endregion  Private Methods

        #region Protected Methods

        /// <summary>
        /// Validates if all the necessary input is provided.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (string.IsNullOrEmpty(cmbServerName.Text.Trim()))
            {
                ErrorMessages.Add("Server name is required"); // TODO: Hard coded string
            }

            // TODO: Check if this is required.
            //if (string.IsNullOrEmpty(txtDatabaseName.Text.Trim()))
            //{
            //    ErrorMessages.Add("Database name is required"); // TODO: Hard coded string
            //}

            if (rdbSqlAuthentication.Checked)
            {
                // Check if user id and password are provided ...
                if (string.IsNullOrEmpty(txtUserName.Text.Trim()))
                {
                    ErrorMessages.Add("Login name is required");
                }
                if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                {
                    ErrorMessages.Add("Password is required");
                }
            }

            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Occurs when cancel is clicked
        /// </summary>
        protected void OnCancelClick()
        {
            this.dbConnectionStringBuilder.ConnectionString = string.Empty;
            this.PreferredDatabaseName = string.Empty; 

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Occurs when OK is clicked
        /// </summary>
        protected virtual void OnOkClick()
        {
            if (ValidateInput() == true)
            {
                //BuildConnectionString(cmbDatabaseName.Text);
                dbConnectionStringBuilder = new SqlConnectionStringBuilder();
                dbConnectionStringBuilder.DataSource = cmbServerName.Text;
                dbConnectionStringBuilder.InitialCatalog = cmbDatabaseName.Text;
                dbConnectionStringBuilder.UserID = txtUserName.Text;
                if (rdbWindowsAuthentication.Checked)
                {
                    dbConnectionStringBuilder.IntegratedSecurity = true;
                }
                else
                {
                    dbConnectionStringBuilder.UserID = txtUserName.Text;
                    dbConnectionStringBuilder.Password = txtPassword.Text;
                }
                //removed //zack
                //connectionString = dbConnectionStringBuilder.ToString();  
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                ShowErrorMessages();
            }

        }
        #endregion  //Protected Methods

        private void cmbDatabaseName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}