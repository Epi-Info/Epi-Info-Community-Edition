#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common; 
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

#endregion

namespace Epi.Data.Office.Forms
{

    /// <summary>
    /// Connection string dialog for a database that already exists
    /// </summary>
    public partial class Access2007ExistingFileDialog : System.Windows.Forms.Form, IConnectionStringGui
    {

        private bool shouldIgnoreNonExistance;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public Access2007ExistingFileDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Ok click for UI inheritance
        /// </summary>       
        protected void OnOkClick()
        {
            if (!shouldIgnoreNonExistance)
            {
                if (!File.Exists(this.txtFileName.Text))
                {
                    MessageBox.Show("Invalid file name.");
                    return;
                }
            }
            //this.connectionString = AccessDatabase.BuildConnectionString(this.txtFileName.Text, this.txtPassword.Text);
            //this.dbConnectionStringBuilder.FileName = this.txtFileName.Text;

            if (this.txtFileName.Text.EndsWith(".accdb", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                this.dbConnectionStringBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
            }

            this.dbConnectionStringBuilder.DataSource = this.txtFileName.Text;
            if (this.UsesPassword)
            {
                this.dbConnectionStringBuilder.ConnectionString = this.dbConnectionStringBuilder.ConnectionString + ";Jet OLEDB:Database Password=" + this.txtPassword.Text;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Occurs when the file name has changed
        /// </summary>
        protected virtual void OnFileNameChanged()
        {
            btnOK.Enabled = !string.IsNullOrEmpty(txtFileName.Text);
        }

        /// <summary>
        /// Occurs when the Cancel button is clicked
        /// </summary>
        protected virtual void OnCancelClick()
        {
            this.dbConnectionStringBuilder.ConnectionString = string.Empty;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Occurs when the Browse button is clicked
        /// </summary>
        protected virtual void OnBrowseClick()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Microsoft Access 2007 Files (*.accdb)|*.accdb";
            if (shouldIgnoreNonExistance)
            {
                dialog.CheckFileExists = false;
            }
            
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFileName.Text = dialog.FileName;
            }
        }
        #endregion

        #region IConnectionStringBuilder

        public virtual void SetDatabaseName(string databaseName) { }
        public virtual void SetServerName(string serverName) { }
        public virtual void SetUserName(string userName) { }
        public virtual void SetPassword(string password) { }

        /// <summary>
        /// Gets the connection string's description
        /// </summary>
        public string ConnectionStringDescription
        {
            get
            {
                return "MS Access File: " + txtFileName.Text;
            }
        }

        public bool ShouldIgnoreNonExistance
        {
            set 
            { 
                this.shouldIgnoreNonExistance = value; 
            }
        }

        private OleDbConnectionStringBuilder dbConnectionStringBuilder = new OleDbConnectionStringBuilder();
        /// <summary>
        /// Gets or sets the DbConnectionStringBuilder object
        /// </summary>
        public DbConnectionStringBuilder DbConnectionStringBuilder
        {
            get
            {
                return dbConnectionStringBuilder;
            }
            set
            {
                dbConnectionStringBuilder = (OleDbConnectionStringBuilder)value;
            }
        }


        /// <summary>
        /// Sets the preferred database name
        /// </summary>
        public string PreferredDatabaseName
        {
            get
            {
                return dbConnectionStringBuilder.DataSource;
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
        #endregion

        #region Event Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.OnOkClick();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.OnCancelClick();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            this.OnFileNameChanged();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.OnBrowseClick();
        }
        #endregion

    }
}