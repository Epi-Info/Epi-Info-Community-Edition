using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class FieldSelectionDialog : Form
    {
        public string SelectedField
        {
            get
            {
                if (cmbColumnNames.SelectedIndex >= 0)
                {
                    return cmbColumnNames.SelectedItem.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public FieldSelectionDialog(List<string> columnNames)
        {
            InitializeComponent();

            cmbColumnNames.DataSource = null;
            cmbColumnNames.Items.Clear();

            cmbColumnNames.DataSource = columnNames;
        }

        private void cmbColumnNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbColumnNames.SelectedIndex >= 0)
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
