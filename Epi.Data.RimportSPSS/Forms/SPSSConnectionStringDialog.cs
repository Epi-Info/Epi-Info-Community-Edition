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

        private DbConnectionStringBuilder connBuilder;

        public SPSSConnectionStringDialog()
        {
            InitializeComponent();


            if (string.IsNullOrEmpty(txtCertFile.Text))
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
            if (string.IsNullOrEmpty(txtCertFile.Text) || string.IsNullOrEmpty(txtOrgKey.Text))
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
				//var red_cap_api = new RedcapApi(txtCertFile.Text);

				//var ExportInstrumentsAsync = await red_cap_api.ExportInstrumentsAsync(txtOrgKey.Text, Content.Instrument, ReturnFormat.json);
				//var ExportInstrumentsAsyncData = JsonConvert.DeserializeObject(ExportInstrumentsAsync);
				//System.Data.DataTable instrumentTable =
				//	JsonConvert.DeserializeObject<System.Data.DataTable>(ExportInstrumentsAsyncData.ToString());
				//string tableName = (string)instrumentTable.Rows[0]["instrument_name"];

				//this.connBuilder = new REDCapConnectionStringBuilder(txtCertFile.Text + "@" +
				//	txtOrgKey.Text + "@" +
				//	tableName);
				//this.DialogResult = DialogResult.OK;
				//this.Hide();
			}
			catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowError(ex.ToString());
                return;
            }
        }

        private void txtOrgKey_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCertFile.Text) || string.IsNullOrEmpty(txtOrgKey.Text))
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
