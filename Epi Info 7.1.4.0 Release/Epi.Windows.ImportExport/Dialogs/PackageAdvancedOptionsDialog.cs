using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.ImportExport;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class PackageAdvancedOptionsDialog : Form
    {
        private string initVector;
        private string salt;
        int iterations;
        private bool includeCodeTables = false;
        private bool includeGridData = true;
        private FormInclusionType formInclusionType;

        public PackageAdvancedOptionsDialog()
        {
            InitializeComponent();
        }

        public PackageAdvancedOptionsDialog(bool includeCodeTables, bool includeGridData, FormInclusionType formInclusionType)
        {
            InitializeComponent();
            checkboxIncludeCodeTables.Checked = includeCodeTables;
            checkboxIncludeGrids.Checked = includeGridData;
            switch (formInclusionType)
            {
                case FormInclusionType.AllDescendants:
                    cmbFormData.SelectedIndex = 0;
                    break;
                case FormInclusionType.DirectDescendants:
                    cmbFormData.SelectedIndex = 1;
                    break;
                case FormInclusionType.CurrentFormOnly:
                    cmbFormData.SelectedIndex = 2;
                    break;                
            }
        }

        public PackageAdvancedOptionsDialog(bool includeCodeTables, bool includeGridData, FormInclusionType formInclusionType, string s_initVector, string s_salt, int i_iterations)
        {
            InitializeComponent();
            checkboxIncludeCodeTables.Checked = includeCodeTables;
            checkboxIncludeGrids.Checked = includeGridData;
            switch (formInclusionType)
            {
                case FormInclusionType.AllDescendants:
                    cmbFormData.SelectedIndex = 0;
                    break;
                case FormInclusionType.DirectDescendants:
                    cmbFormData.SelectedIndex = 1;
                    break;
                case FormInclusionType.CurrentFormOnly:
                    cmbFormData.SelectedIndex = 2;
                    break;
            }

            if (s_initVector.Length == 16 && s_salt.Length == 32 && i_iterations <= nudIterations.Maximum && i_iterations >= nudIterations.Minimum)
            {
                checkboxCustomEncryption.Checked = true;
                txtInitVector.Text = s_initVector;
                txtSalt.Text = s_salt;
                nudIterations.Value = i_iterations;
            }
            else
            {
                checkboxCustomEncryption.Checked = false;
                txtInitVector.Text = string.Empty;
                txtSalt.Text = string.Empty;
                nudIterations.Value = 4;
                MsgBox.ShowError("Encryption parameters were passed into the dialog that were invalid. Custom encryption has been disabled.");
            }
        }

        public string InitVector
        {
            get
            {
                return this.initVector;
            }
        }

        public string Salt
        {
            get
            {
                return this.salt;
            }
        }

        public int Iterations
        {
            get
            {
                return this.iterations;
            }
        }

        public FormInclusionType FormInclusionType
        {
            get
            {
                return this.formInclusionType;
            }
        }

        public bool IncludeGridData
        {
            get
            {
                return this.includeGridData;
            }
        }

        public bool IncludeCodeTables
        {
            get
            {
                return this.includeCodeTables;
            }
        }

        private bool ValidateInput()
        {
            if (checkboxCustomEncryption.Checked)
            {
                if (txtInitVector.Text.Length != 16)
                {
                    MsgBox.ShowError("The initialization vector must be 16 characters.");
                    return false;
                }
                if (txtSalt.Text.Length != 32)
                {
                    MsgBox.ShowError("The salt value must be 32 characters.");
                    return false;
                }
            }

            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                if (checkboxCustomEncryption.Checked)
                {
                    this.salt = txtSalt.Text;
                    this.initVector = txtInitVector.Text;
                    this.iterations = (int)nudIterations.Value;
                }
                else
                {
                    this.salt = string.Empty;
                    this.initVector = string.Empty;
                }

                this.includeCodeTables = checkboxIncludeCodeTables.Checked;
                this.includeGridData = checkboxIncludeGrids.Checked;

                switch (cmbFormData.SelectedIndex)
                {
                    case 1:
                        formInclusionType = FormInclusionType.DirectDescendants;
                        break;
                    case 2:
                        formInclusionType = FormInclusionType.CurrentFormOnly;
                        break;
                    default:
                        formInclusionType = FormInclusionType.AllDescendants;
                        break;
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void checkboxCustomEncryption_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxCustomEncryption.Checked)
            {
                txtInitVector.Enabled = true;
                txtSalt.Enabled = true;
                nudIterations.Enabled = true;
            }
            else
            {
                txtInitVector.Enabled = false;
                txtSalt.Enabled = false;
                nudIterations.Enabled = false;

                txtInitVector.Text = string.Empty;
                txtSalt.Text = string.Empty;
                nudIterations.Value = 4;
            }
        }
    }
}
