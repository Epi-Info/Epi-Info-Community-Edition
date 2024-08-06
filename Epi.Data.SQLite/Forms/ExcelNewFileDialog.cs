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
    /// 
    /// </summary>
    public partial class ExcelNewFileDialog : Form, IConnectionStringGui 
    {
        /// <summary>
        /// Excel New File Dialog
        /// </summary>
        public ExcelNewFileDialog()
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
                return "MS Excel File: " + txtFileName.Text;
            }
        }

        public bool ShouldIgnoreNonExistance
        {
            set { }
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
            if (File.Exists(this.txtFileName.Text))
            {
                MessageBox.Show("File already exists.");
                return;
            }
            else
            {
                //this.connectionString = ExcelWorkbook.BuildConnectionString(this.txtFileName.Text, true);
                this.dbConnectionStringBuilder.FileName = this.txtFileName.Text;  
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
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
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Microsoft Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFileName.Text = dialog.FileName;
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