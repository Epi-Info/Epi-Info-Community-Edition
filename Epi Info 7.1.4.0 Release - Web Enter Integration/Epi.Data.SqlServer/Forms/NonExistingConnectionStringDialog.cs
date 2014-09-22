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

#endregion  //Namespaces

namespace Epi.Data.SqlServer.Forms
{
    /// <summary>
    /// Partial class for Data.SqlServer form NonExistingConnectionStringDialog
    /// </summary>
    public partial class NonExistingConnectionStringDialog : Epi.Data.SqlServer.Forms.ConnectionStringDialog
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public NonExistingConnectionStringDialog()
        {
            InitializeComponent();
        }

        #endregion  //Constructor

        #region Event Handlers

        /// <summary>
        /// Loads the form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void NonExistingConnectionStringDialog_Load(object sender, EventArgs e)
        {
            this.btnTest.Visible = true;
            this.Text = "Create a SQL Server database";
            lblInstructions.Text = "Enter information to create the SQL Server database.";

            this.cmbDatabaseName.Visible = false;
            this.txtDatabaseName.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the Test button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void btnTest_Click(object sender, EventArgs e)
        {
            string database = this.txtDatabaseName.Text;

            Epi.Data.SqlServer.SqlDatabase db = new SqlDatabase();
            //db.ConnectionString = this.DbConnectionStringBuilder.ToString(); 

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

            db.ConnectionString = this.DbConnectionStringBuilder.ToString();

            try
            {
                if (db.TestConnection())
                {
                    MessageBox.Show("Connection OK!"); // TODO: hard coded string.

                    //this.txtDatabaseName.Text = string.Empty;
                    //List<string> databaseNamesList = db.GetDatabaseNameList();
                    //foreach (string name in databaseNamesList)
                    //{
                    //    this.txtDatabaseName.Text = name;
                    //}
                }
                else
                {
                    MessageBox.Show("Connection failed."); // TODO: hard coded string.
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message); // TODO: hard coded string
            }
        }

        // dcs0 9/3/2008 don't allow any characters that are not allowed as "valid SQL Server identifiers"
        // TODO: Revisit this later. This validation should not be done in UI layer.
        private void txtDatabaseName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString().IndexOfAny(new char[] { '#', '@', '!', '%', '^', '&', '*', '.', ' ' }) >= 0)
            {
                e.KeyChar = '\0';
                e.Handled = false;
                Console.Beep();
            }
        }

        #endregion  //Event Handlers

        #region Public Properties


        /// <summary>
        /// Sets the preferred database name
        /// </summary>
        public override string PreferredDatabaseName
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
        public override string ConnectionStringDescription
        {
            get
            {
                return cmbServerName.Text + "::" + txtDatabaseName.Text;
            }
        }

        #endregion  //Public Properties

        #region Protected Methods

        /// <summary>
        /// Validates if all the necessary input is provided.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (string.IsNullOrEmpty(txtDatabaseName.Text.Trim()))
            {
                ErrorMessages.Add("Database name is required"); // TODO: Hard coded string
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Occurs when OK is clicked
        /// </summary>
        protected override void OnOkClick()
        {
            if (ValidateInput() == true)
            {
                //BuildConnectionString(txtDatabaseName.Text);
                dbConnectionStringBuilder = new SqlConnectionStringBuilder();
                dbConnectionStringBuilder.DataSource = cmbServerName.Text;
                dbConnectionStringBuilder.InitialCatalog = txtDatabaseName.Text;
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

        #endregion Protected Methods

        #region Private Methods

        private bool ValidDatabaseName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 30)
            {
                return false;
            }
            else if (name.IndexOfAny(new char[] { '#', '@', '!', '%', '^', '&', '*', '.', ' ' }) >= 0)
            {
                return false;
            }
            else
            {
                char firstChar = name.ToCharArray()[0];
                return (Char.IsLetter(firstChar) || ((firstChar == '_') && name.Length > 1));
            }
        }

        #endregion  //Private Methods

        #region Public Methods
        public override void SetDatabaseName(string databaseName)
        {
            this.txtDatabaseName.Text = databaseName;
        }
        #endregion // Public Methods
    }
}

