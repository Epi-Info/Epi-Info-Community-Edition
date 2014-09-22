using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog for Quit command
	/// </summary>
    public partial class NewColumnNameDialog : Form
	{
        public string ColumnName = string.Empty;
        
        public NewColumnNameDialog()
		{
			InitializeComponent();
            Construct();
		}

        private void Construct()
        {
            this.btnOK.Click += new EventHandler(btnOK_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            ColumnName = textBoxColumnName.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        public bool ValidateColumnName(string columnName)
        {
            bool isValid = true;

            for (int i = 0; i < columnName.Length; i++)
            {
                string columnNameChar = columnName.Substring(i, 1);
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(columnNameChar, "[A-Za-z0-9_]");
                if (!m.Success)
                {
                    return false;
                }
            }

            return isValid;
        }

        void textBoxColumnName_KeyUp(object sender, KeyEventArgs e)
        {
            if (ValidateColumnName(((TextBox)sender).Text))
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }
    }
}