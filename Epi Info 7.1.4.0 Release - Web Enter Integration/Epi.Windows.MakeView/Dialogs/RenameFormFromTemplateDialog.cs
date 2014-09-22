using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Data.Services;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
    public partial class RenameFormFromTemplatePageDialog : Form
	{
		private string formName = string.Empty;

        public RenameFormFromTemplatePageDialog()
        {
            InitializeComponent();
        }

		private void RenamePage_Load(object sender, System.EventArgs e)
		{
            textBoxFormName.Text = formName;
            textBoxFormName.Focus();
		}

        private bool ValidateFormName()
        {
            bool valid = true;
            return valid;
        }

		public string FormName
		{
			get
			{
				return formName;
			}
			set
			{
                formName = value;
			}
		}

        private void textBoxFormName_TextChanged(object sender, EventArgs e)
        {
        	formName = textBoxFormName.Text.Trim();
            if (textBoxFormName.Text.Length > 0)
            {
                buttonOK.Enabled = true;
            }
            else
            {
                buttonOK.Enabled = false;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (ValidateFormName() == false)
            {
                this.DialogResult = DialogResult.None;
                this.textBoxFormName.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
	}
}

