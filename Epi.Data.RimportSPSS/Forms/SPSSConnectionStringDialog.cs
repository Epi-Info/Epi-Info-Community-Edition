using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Epi.Windows.Dialogs;


namespace Epi.Data.RimportSPSS.Forms
{
    public partial class SPSSConnectionStringDialog : DialogBase, IConnectionStringGui
    {

        private RimportSPSSConnectionStringBuilder connBuilder;

        public SPSSConnectionStringDialog()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(fileName.Text))
            {
                btnOk.Enabled = true;
            }
        }

        public bool ShouldIgnoreNonExistance { set => throw new NotImplementedException(); }

        public DbConnectionStringBuilder DbConnectionStringBuilder { get { return connBuilder; } }

        public string PreferredDatabaseName { get; }

        public string ConnectionStringDescription { get { return "SPSS"; } }

        public bool UsesPassword { get { return false; } }

        public void SetDatabaseName(string databaseName)
        {
            
        }

        public void SetPassword(string password)
        {
            
        }

        public void SetServerName(string serverName)
        {
            
        }

        public void SetUserName(string userName)
        {

        }

        //private void btnBrowse_Click(object sender, EventArgs e)
        //{

        //}

        //private void txtCertFile_TextChanged(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(fileName.Text))
        //    {
        //        btnOk.Enabled = false;
        //    }
        //    else
        //    {
        //        btnOk.Enabled = true;
        //    }
        //}

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                string pwd = "Password is: " + password.Text;
                string filePath = "File path is: " + fileName.Text;
                MessageBox.Show(pwd.Trim() + filePath.Trim());

			}
			catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowError(ex.ToString());
                return;
            }
        }

        //private void txtOrgKey_TextChanged(object sender, EventArgs e)
        //{

        //}

        private void findFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "SPSS Data Files (*.sav)|*.sav";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                fileName.Text = dialog.FileName;
            }
        }

        private void SPSSConnectionStringDialog_Load(object sender, EventArgs e)
        {
            btnOk.Enabled = false;
        }

        private void fileName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fileName.Text))
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }
    }
}
