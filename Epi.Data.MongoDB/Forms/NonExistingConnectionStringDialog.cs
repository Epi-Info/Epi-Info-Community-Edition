using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Data;
using Epi.Data.MongoDB;
using System.Data.CData.MongoDB;

namespace Epi.Data.MongoDB.Forms
{   
    /// <summary>
    /// Partial class for Data.SqlServer form NonExistingConnectionStringDialog
    /// </summary>
    public partial class NonExistingConnectionStringDialog : Epi.Data.MongoDB.Forms.ConnectionStringDialog
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
            this.Text = "Create a MySQL database";
            lblInstructions.Text = "Enter information to create the MySQL database.";

           // this.txtDatabaseName2.Visible = false;
            this.txtDatabaseName.Visible = true;
        }

        // dcs0 9/3/2008 don't allow any characters that are not allowed as "valid SQL Server identifiers"
        private void txtDatabaseName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString().IndexOfAny(new char[] { '#', '@', '!', '%', '^', '&', '*', '.', ' '})>=0)
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

        #endregion  Public Properties

        #region Protected Methods

        /// <summary>
        /// Occurs when OK is clicked
        /// </summary>
        protected override void OnOkClick()
        {
            if (ValidateInput())
            {
                dbConnectionStringBuilder = new MongoDBConnectionStringBuilder();
                dbConnectionStringBuilder.AutoCache = false; //.PersistSecurityInfo = false;
                dbConnectionStringBuilder.Server = cmbServerName.Text.Trim();
                dbConnectionStringBuilder.Database = txtDatabaseName.Text.Trim();
                dbConnectionStringBuilder.User = txtUserName.Text;
                dbConnectionStringBuilder.Password = txtPassword.Text;
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

        //private bool ValidDatabaseName(string name)
        //{
        //    if (string.IsNullOrEmpty(name) || name.Length > 30)
        //    {
        //        return false;
        //    }
        //    else if (name.IndexOfAny(new char[] { '#', '@', '!', '%', '^', '&', '*', '.', ' '})>=0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        char firstChar = name.ToCharArray()[0];
        //        return(Char.IsLetter(firstChar) || ((firstChar == '_') && name.Length > 1));
        //    }
        //}
        
        #endregion  //Private Methods

    }
}

