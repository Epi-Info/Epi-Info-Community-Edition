using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Data.SqlServer.Forms
{
    /// <summary>
    /// Partial class for Data.SqlServer form ExistingConnectionStringDialog 
    /// </summary>
    public partial class ExistingConnectionStringDialog : Epi.Data.SqlServer.Forms.ConnectionStringDialog
    {   
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExistingConnectionStringDialog()
        {
            InitializeComponent();
        }

        private void ExistingConnectionStringDialog_Load(object sender, EventArgs e)
        {
            this.Text = "Connect to SQL Server database";
            lblInstructions.Text = "Enter information to connect to the SQL Server database.";
        }

        /// <summary>
        /// ValidateInput
        /// </summary>
        /// <returns>bool</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (String.IsNullOrEmpty(cmbDatabaseName.Text))
            {
                ErrorMessages.Add("Database name is required");
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Click event for Test button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        protected override void btnTest_Click(object sender, EventArgs e)
        {
            string database = this.cmbServerName.Text;

            Epi.Data.SqlServer.SqlDatabase db = new SqlDatabase();
            //db.ConnectionString = this.DbConnectionStringBuilder.ToString(); 

            dbConnectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
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
    }
}

