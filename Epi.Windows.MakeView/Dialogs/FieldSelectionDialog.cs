using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using System.Data;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
	public partial class FieldSelectionDialog : DialogBase
	{
		private string columnName = string.Empty;

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public FieldSelectionDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Field Selection Dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="project">The project</param>
        /// <param name="tableName">The table name</param>
		public FieldSelectionDialog(MainForm frm, Project project, string tableName) 
            : base(frm)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			DataTable columns = project.CollectedData.GetTableColumnSchema(tableName);

            columns.PrimaryKey = new DataColumn[] {columns.Columns[ColumnNames.COLUMN_NAME]};
            if (columns.Columns.Contains(ColumnNames.UNIQUE_KEY))
            {
                columns.Rows.Remove(columns.Rows.Find(ColumnNames.UNIQUE_KEY));
            }
            
			lbxFields.DataSource = columns;
			lbxFields.DisplayMember = ColumnNames.COLUMN_NAME;
			lbxFields.ValueMember = ColumnNames.COLUMN_NAME;
		}

	
		#region Public Property

        /// <summary>
        /// The column name
        /// </summary>
		public string ColumnName
		{
			get
			{
				return(columnName);
			}
		}
		#endregion Public Property
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			if (lbxFields.SelectedIndex > -1)
			{
				columnName = lbxFields.SelectedValue.ToString();
			}
			this.DialogResult = DialogResult.OK;
			this.Hide();
		
		}

            private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (lbxFields.SelectedIndex > -1)
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

