using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{

    /// <summary>
    /// Form for defining code tables
    /// </summary>
    public partial class CodeTableDefinition : Form
    {
        /// <summary>
        /// Default constructor for exclusive use by the designer
        /// </summary>
        public CodeTableDefinition()
        {
            InitializeComponent();
        }

        private void txtCodeTableName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (txtCodeTableName.Text.Length > 0);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (!txtCodeTableName.Text.StartsWith("code"))
                txtCodeTableName.Text = "code" + txtCodeTableName.Text;
            this.Hide();
        }

        /// <summary>
        /// Gets the code table name
        /// </summary>
        public string CodeTableName
        {
            get
            {
                return txtCodeTableName.Text;
            }
        }
        /// <summary>
        /// Gets the code table name as entered by the user.
        /// </summary>
        public string CodeTableRootName
        {
            get
            {
                return txtCodeTableName.Text.Remove(0, 4);
            }
        }
    }
}