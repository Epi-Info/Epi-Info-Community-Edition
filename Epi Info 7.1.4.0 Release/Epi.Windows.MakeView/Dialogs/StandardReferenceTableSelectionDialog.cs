using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Fields;
using Epi.Windows.Dialogs;


namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// The Standardard Reference Table Selection dialog
    /// </summary>
    public partial class StandardReferenceTableSelectionDialog : DialogBase
	{

		/// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public StandardReferenceTableSelectionDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Standard Reference Table Selection dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public StandardReferenceTableSelectionDialog(MainForm frm): base(frm)
        {
            InitializeComponent();
        }


		private void rdbOther_CheckedChanged(object sender, System.EventArgs e)
		{
			lblDataSource.Enabled = rdbOther.Checked;
			txtFileName.Enabled = rdbOther.Checked;
			btnBrowse.Enabled = rdbOther.Checked;
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			btnOk.Enabled = true;
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		/// <summary>
		/// Gets the field name
		/// </summary>
		public string FieldName
		{
			get
			{
				return Util.Squeeze(lbxFields.SelectedItem.ToString().Split('-')[0]);
			}
		}

	}
}
