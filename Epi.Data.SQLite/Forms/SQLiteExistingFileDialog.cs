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

namespace Epi.Data.SQLite.Forms
{

    /// <summary>
    /// Connection string dialog for a database that already exists
    /// </summary>
    public partial class SQLiteExistingFileDialog : System.Windows.Forms.Form, IConnectionStringGui
    {

        private bool shouldIgnoreNonExistance;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public SQLiteExistingFileDialog()
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

            if (this.txtFileName.Text.EndsWith(".db", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                this.dbConnectionStringBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";
            }
            this.dbConnectionStringBuilder.DataSource = this.txtFileName.Text;
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
            dialog.Filter = "SQLite Files (*.db)|*.db";
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
                return "SQLite File: " + txtFileName.Text;
            }
        }

        public bool ShouldIgnoreNonExistance
        {
            set 
            {
                shouldIgnoreNonExistance = value;
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