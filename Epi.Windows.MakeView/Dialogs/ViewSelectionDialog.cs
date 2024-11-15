using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Epi;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// The View Selection dialog
	/// </summary>
    public partial class ViewSelectionDialog : DialogBase
	{
		#region Private Class Members
		private Project currentProject = null;
		private string tableName = string.Empty;
		#endregion Private Class Members

		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ViewSelectionDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="project">The current project</param>
        public ViewSelectionDialog(MainForm frm, Project project)
            : base(frm)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			currentProject = project;
		}
		#endregion Constructor

		#region Public Properties
        /// <summary>
        /// The table name
        /// </summary>
		public string TableName
		{
			get
			{
				return (tableName);
			}
		}
		#endregion Public Properties

		#region Event Handlers
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void ViewSelection_Load(object sender, System.EventArgs e)
		{
			LoadTables();

            if (lbxTables.SelectedIndex == -1)
            {
                btnOK.Enabled = false;
            }
            else
            {
                btnOK.Enabled = true;
            }
		}
		
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			if (lbxTables.SelectedIndex > -1)
			{
				tableName = lbxTables.SelectedValue.ToString();
			}
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}
		#endregion Event Handlers

		#region Private Methods
		private void LoadTables()
		{
            DataTable bindingTable = currentProject.CodeData.GetCodeTableNamesForProject(currentProject);

            string  collectedDataTableName = string.Empty;

            DataTable pages;

            Epi.Windows.MakeView.Forms.MakeViewMainForm makeViewMainForm = ((Epi.Windows.MakeView.Forms.MakeViewMainForm)this.mainForm);
            View currentView = makeViewMainForm.projectExplorer.currentPage.view;

            foreach (View view in currentView.GetProject().Views)
            { 
                pages = currentProject.Metadata.GetPagesForView(view.Id);

                foreach (DataRow row in pages.Rows)
                {
                    collectedDataTableName = string.Format("{0} - {1}", view.Name, row["Name"].ToString());

                    if (!string.IsNullOrEmpty(collectedDataTableName))
                    {
                        bindingTable.Rows.Add(new string[] { collectedDataTableName });
                    }
                }
            }

            DataView dataView = bindingTable.DefaultView;

            string display = String.Empty;
            string kindOfDatabase = currentProject.CodeData.IdentifyDatabase();

            //TODO: Will need to rework this section not to be dependant on strings and develop a 
            // more OO solution
            switch (kindOfDatabase)
            {
                case "SQLite":
                case "ACCESS":
                    display = ColumnNames.NAME;
                    break;
                case "SQLSERVER":
                    display = ColumnNames.SCHEMA_TABLE_NAME;
                    break;
                case "MYSQL":
                    display = ColumnNames.SCHEMA_TABLE_NAME;
                    break;
                default:
                    display = ColumnNames.SCHEMA_TABLE_NAME;
                    break;
            }
            dataView.Sort = display;
            lbxTables.DataSource = dataView.ToTable(true, new string[] { display });
            lbxTables.DisplayMember = display;
            lbxTables.ValueMember = display;
		}
		#endregion Private Methods
	}
}

