#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Form to change tab index of fields on a page
	/// </summary>
    public partial class TabOrderDialog : DialogBase
	{
		#region Private Class Members
		private DataTable fieldsTabOrder = null;
        private int currentRowIndex;
        private System.Data.DataView dv;
        private DataTable sortedTable;
        private Page currentPage;
        private View currentView;
		#endregion //Private Class Members

		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
		public TabOrderDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public TabOrderDialog(Page page, DataSets.TabOrders.TabOrderDataTable tabOrderForFields)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
            currentPage = page;
            currentView = page.GetView();
			fieldsTabOrder = tabOrderForFields;
		}
		#endregion //Constructor

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

        /// <summary>
        /// Loads fields by tab order
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void TabOrder_Load(object sender, System.EventArgs e)
		{
			System.Data.DataView dataView = fieldsTabOrder.DefaultView;
			dataView.AllowEdit = false;
			dgControls.DataSource = dataView;
			dgControls.TableStyles.Add(dgTableStyle);
            dv = (DataView)dgControls.DataSource;
            btnUp.Enabled = false;      //Disable up button since first row is selected
            btnOK.Enabled = false;     
		}

        /// <summary>
        /// Handles the Click event of the Down button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void btnDown_Click(object sender, System.EventArgs e)
		{
            btnOK.Enabled = true;
            SetGridDataSource();                       
            DataRow currentRow = dv.Table.Rows[currentRowIndex];
            DataRow nextRow = dv.Table.Rows[currentRowIndex + 1];

            currentRow["TabIndex"] = ((int)currentRow["TabIndex"]) + 1;
            nextRow["TabIndex"] = ((int)nextRow["TabIndex"]) - 1;

            currentRowIndex = dv.Table.Rows.IndexOf(currentRow) + 1;
            dv.Sort = "TabIndex";                     
            
            dgControls.CurrentRowIndex = currentRowIndex;
            dgControls.Select(currentRowIndex);
            sortedTable = dv.ToTable();
            DisableEnableNavigationButtons();
		}

        /// <summary>
        /// Handles the Click event of the datagrid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void dgControls_Click(object sender, System.EventArgs e)
		{
			dgControls.Select(dgControls.CurrentRowIndex);
            currentRowIndex = dgControls.CurrentRowIndex;
            DisableEnableNavigationButtons();
            btnOK.Enabled = true;
		}

        /// <summary>
        /// Handles the Click event of the Up button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void btnUp_Click(object sender, System.EventArgs e)
		{
            btnOK.Enabled = true;
            SetGridDataSource();
            DataRow currentRow = dv.Table.Rows[currentRowIndex];
            DataRow nextRow = dv.Table.Rows[currentRowIndex - 1];

            currentRow["TabIndex"] = ((int)currentRow["TabIndex"]) - 1;
            nextRow["TabIndex"] = ((int)nextRow["TabIndex"]) + 1;

            currentRowIndex = dv.Table.Rows.IndexOf(currentRow) - 1;
            dv.Sort = "TabIndex";

            dgControls.CurrentRowIndex = currentRowIndex;
            dgControls.Select(currentRowIndex);
            sortedTable = dv.ToTable();
            DisableEnableNavigationButtons();
		}

        /// <summary>
        /// Saves the tab order of the fields
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            //If user highlights row, then clicks ok, sortedTable would be null and error
            //Set the sortedTable so it will not be null
            if (sortedTable == null)
            {
                dv.Sort = "TabIndex";
                sortedTable = dv.ToTable();
            }
            currentView.Project.Metadata.UpdateTabOrder(sortedTable, currentView, currentPage);
            
            currentView.MustRefreshFieldCollection = true;      //refresh data
            this.Close();
        }

		#endregion //Event Handlers      

        #region Private Methods

        /// <summary>
        /// Sets the grid's data source
        /// </summary>
        private void SetGridDataSource()
        {
            if (sortedTable == null)
            {
                dv = (DataView)dgControls.DataSource;
            }
            else
            {
                dgControls.DataSource = sortedTable.DefaultView;
                dv = (DataView)dgControls.DataSource;
            }
        }

        /// <summary>
        /// Disables or enables up and down arrow buttons
        /// </summary>
        private void DisableEnableNavigationButtons()
        {
            btnDown.Enabled = (dgControls.CurrentRowIndex != fieldsTabOrder.Rows.Count - 1);
            btnUp.Enabled = !dgControls.IsSelected(0);
        }
        #endregion //Private Methods

        private void dgControls_CurrentCellChanged(object sender, EventArgs e)
        {
            dgControls.Select(dgControls.CurrentCell.RowNumber);
            currentRowIndex = dgControls.CurrentRowIndex;
            DisableEnableNavigationButtons();
            btnOK.Enabled = true;
        }
    
    }
}

