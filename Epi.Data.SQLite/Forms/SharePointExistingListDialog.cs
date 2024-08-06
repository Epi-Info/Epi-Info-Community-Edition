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

namespace Epi.Data.Office.Forms
{
    /// <summary>
    /// Excel Existing File Dialog
    /// </summary>
    public partial class SharePointExistingListDialog : Form, IConnectionStringGui 
    {

        public string ServerPart { get; set; }
        public string ListPart { get; set; }

        /// <summary>
        /// Excel Existing File Dialog
        /// </summary>
        public SharePointExistingListDialog()
        {
            InitializeComponent();
        }

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
                return "SharePoint List: " + txtUrl.Text;
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
                return "";//txtFileName.Text = Path.Combine(Configuration.Directories.Project, value + ".mdb");
            }
        }

        /// <summary>
        /// Gets whether or not the user entered a password
        /// </summary>
        public bool UsesPassword
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Ok click for UI inheritance
        /// </summary>        
        protected void OnOkClick()
        {
            string parsedUri = Uri.UnescapeDataString(txtUrl.Text);
            string serverPart = parsedUri.Substring(0,parsedUri.IndexOf("_layouts"));
            string listPart = parsedUri.Substring(parsedUri.IndexOf("List="), 43);

            string connString = "Provider=Microsoft.ACE.OLEDB.12.0;WSS;IMEX=1;RetrieveIds=Yes;DATABASE=" + serverPart + ";" + listPart + ";";

            this.dbConnectionStringBuilder = new OleDbConnectionStringBuilder(connString);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Occurs when the file name has changed
        /// </summary>
        protected virtual void OnFileNameChanged()
        {
            btnOK.Enabled = !string.IsNullOrEmpty(txtUrl.Text);
        }

        /// <summary>
        /// Occurs when the Cancel button is clicked
        /// </summary>
        protected virtual void OnCancelClick()
        {
            //this.connectionString = null;
            this.dbConnectionStringBuilder.ConnectionString = null;  
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Occurs when the Browse button is clicked
        /// </summary>
        protected virtual void OnBrowseClick()
        {

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



        #region IConnectionStringGui Members

        public bool ShouldIgnoreNonExistance
        {
            set {  }
        }

        #endregion
    }
}