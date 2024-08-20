#region Namespaces

using Epi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;  
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

#endregion

namespace Epi.Data.SQLite.Forms
{

    /// <summary>
    /// Connection string dialog for a database that does not exist yet
    /// </summary>
    public partial class SQLiteNewFileDialog : System.Windows.Forms.Form, IConnectionStringGui 
    {

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public SQLiteNewFileDialog()
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
            this.dbConnectionStringBuilder.DataSource = this.txtFileName.Text;
            this.dbConnectionStringBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";
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
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "SQLite Files (*.db)|*.db";

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
                return txtFileName.Text;   // Path.Combine(Configuration.Directories.Project, txtFileName.Text + ".mdb");
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

        #region Event Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            bool valid = ValidateDatabaseName();
            if (valid)
            {
                this.OnOkClick();
            }
            else
            {
                txtFileName.Focus();
            }
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
            OnBrowseClick();
        }
        #endregion

        /// <summary>
        /// Validates the database name
        /// </summary>
        private bool ValidateDatabaseName()
        {
            string filename;
            string dir;
            bool valid = false;
            if (!string.IsNullOrEmpty(txtFileName.Text.Trim()))
            {
                dir = Path.GetDirectoryName(txtFileName.Text.Trim());
                filename = Path.GetFileName(txtFileName.Text.Trim());
                Regex dirRegex = new Regex(@"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^#$/:*?<>""|]*))+)$"); 
                //Regex dirRegex = new Regex(@"[a-zA-Z]:(\\w+)*\\");
                if (!dirRegex.IsMatch(dir))
                {
                    MessageBox.Show(this, "Invalid path. Please enter a valid path.", "Invalid path", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    valid = false;
                    return valid;
                }

                if (string.IsNullOrEmpty(filename))
                {
                    MessageBox.Show(this, "Blank database name. Please enter a database name with full path.", "Input Database Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Path.HasExtension(filename))
                {
                    if (!(Path.GetExtension(filename).ToLowerInvariant() == ".mdb"))
                    {
                        MessageBox.Show(this, "Invalid database type. Please enter correct database extension name .mdb.", "Input Correct Extension Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        valid = true;
                        txtFileName.Text = Path.Combine(dir, filename); 
                    }
                }
                else 
                {
                    filename = filename + ".mdb";
                    txtFileName.Text = Path.Combine(dir, filename);
                    //txtFileName.Text = txtFileName.Text + ".mdb";
                    valid = true;
                }

                if (valid == true)
                {
                    Regex filenameRegex = new Regex(@"^[$#%A-Za-z0-9_-]*?\.mdb$");
                    if (!filenameRegex.IsMatch(filename.ToLowerInvariant()))
                    {
                        MessageBox.Show(this, "Invalid database name. Please enter a valid database name.", "Invalid database name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        valid = false;
                    }
                }

                if (valid == true)
                {
                    if (string.IsNullOrEmpty(dir))
                    {
                        MessageBox.Show(this, "Please enter full path of database.", "Input Full Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        valid = false;
                    }
                    else
                    {
                        if (!(Directory.Exists(dir)))
                        {
                            DialogResult dr = MessageBox.Show(this, "The directory does not exist, do you want to create it : " + dir, "Create Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question );
                            
                            if (dr == DialogResult.Yes )
                            {
                                try
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show (ex.Message , "Create directory Error", MessageBoxButtons.OK );
                                    valid = false;
                                }
                            }
                            else
                            {
                             valid = false;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(this ,"The database name cannot be blank.  Please enter a database name with full path.", "Enter database name", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return valid;

        }
    }
}