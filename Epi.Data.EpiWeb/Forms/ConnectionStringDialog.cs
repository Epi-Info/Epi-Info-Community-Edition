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

namespace Epi.Data.EpiWeb.Forms
{
    public partial class ConnectionStringDialog : DialogBase, IConnectionStringGui
    {

        private EpiWebConnectionStringBuilder connBuilder;

        public ConnectionStringDialog()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(txtCertFile.Text))
            {
                btnOk.Enabled = false;
            }
        }

        public bool ShouldIgnoreNonExistance { set => throw new NotImplementedException(); }

        public DbConnectionStringBuilder DbConnectionStringBuilder { get { return connBuilder; } }

        public string PreferredDatabaseName { get; }

        public string ConnectionStringDescription { get { return "Epi Info Web & Cloud Services"; } }

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

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Epi Info Web Certificate Files (*.wcert)|*.wcert";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtCertFile.Text = dialog.FileName;
            }
        }

        private void txtCertFile_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCertFile.Text))
            {
                btnOk.Enabled = false;
            }
            else
            {
                this.connBuilder = new EpiWebConnectionStringBuilder("epiweb://" + txtCertFile.Text);
                btnOk.Enabled = true;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
    }
}
