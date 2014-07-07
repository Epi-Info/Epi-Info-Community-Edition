using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Data;
using Epi.Windows;

namespace Epi.Windows.Dialogs
{
    public partial class AdvancedReadDialog : DialogBase
    {
        private string sqlQuery;
        private IDbDriver db;

        public AdvancedReadDialog()
        {
            InitializeComponent();
            sqlQuery = string.Empty;
        }

        public AdvancedReadDialog(IDbDriver db)
        {
            InitializeComponent();
            this.sqlQuery = string.Empty;
            this.db = db;
        }

        public string SQLQuery
        {
            get
            {
                return sqlQuery;
            }
            set
            {
                this.sqlQuery = value;
                txtQuery.Text = SQLQuery;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool valid = ValidateQuery();

            if (!valid)
            {
                sqlQuery = string.Empty;
                MsgBox.ShowInformation("Query is not valid. Please try again.");
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
            else
            {
                sqlQuery = txtQuery.Text.Trim();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }       

        private bool ValidateQuery()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(txtQuery.Text))
            {
                return false;
            }

            string query = txtQuery.Text.Trim().ToLower();

            if (!query.StartsWith("select"))
            {
                valid = false;
            }

            if (query.Contains(";") && !query.EndsWith(";"))
            {
                valid = false;
            }

            if (query.Contains("--"))
            {
                valid = false;
            }

            if (query.Contains("update ") || query.Contains("insert ") || query.Contains("drop table ") || query.Contains("delete ") || query.Contains("goto ") || query.Contains("alter ") || query.Contains("reconfigure ") || query.Contains("rollback ") || query.Contains("create ") || query.Contains("database "))
            {
                valid = false;
            }

            return valid;
        }

        private void AdvancedReadDialog_Load(object sender, EventArgs e)
        {
            if (db == null)
            {
                throw new ApplicationException("Db cannot be null.");
            }
        }
    }
}
