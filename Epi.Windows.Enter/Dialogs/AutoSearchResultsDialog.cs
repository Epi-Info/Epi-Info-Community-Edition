#region Namespaces	 
	
using Epi.Windows.Enter;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Enter.Dialogs
{
    public partial class AutoSearchResults : Epi.Windows.Dialogs.DialogBase
    {

        private int currentRow = -1;
        private DataTable dataTable = null;
        private new EnterMainForm mainForm;

        #region Constructors

        /// <summary>
        /// The default constructor for the Auto Search Results dialog
        /// </summary>
        public AutoSearchResults()
        {
            InitializeComponent();
        } 

        /// <summary>
        /// Constructor for AutoSearch Results dialog
        /// </summary>
        /// <param name="view">The current view</param>
        /// <param name="mainForm">Enter module's main form</param>
        /// <param name="data">Data table containing auto search results</param>
        public AutoSearchResults(View view, EnterMainForm mainForm, DataTable data, bool showContinueNewMessage = false)
            : base(mainForm)
        {
            #region Input Validation

            if (view == null)
            {
                {
                    throw new ArgumentNullException("view");
                }
            }

            #endregion Input Validation

            InitializeComponent();
            //this.view = view;
            this.mainForm = mainForm;

            // set DataGridView properties
            this.dataGrid1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGrid1.AllowUserToAddRows = false;
            this.dataGrid1.AllowUserToDeleteRows = false;
            this.dataGrid1.AllowUserToOrderColumns = false;
            this.dataGrid1.AllowUserToResizeRows = false;
            this.dataGrid1.AllowUserToResizeColumns = true;
            this.dataGrid1.AllowDrop = false;
            this.dataGrid1.MultiSelect = false;

            // add event handlers
            this.dataGrid1.KeyDown += new KeyEventHandler(OnKeyDown);
            this.btnOK.KeyDown += new KeyEventHandler(OnKeyDown);
            this.btnCancel.KeyDown += new KeyEventHandler(OnKeyDown);
            this.KeyDown += new KeyEventHandler(OnKeyDown);
            base.KeyDown += new KeyEventHandler(OnKeyDown);
            this.dataGrid1.SelectionChanged += new EventHandler(dataGrid1_SelectionChanged);

            this.lblOKInstructions.Visible = !showContinueNewMessage;
            this.lblCancelInstructions.Visible = !showContinueNewMessage;
            this.btnCancel.Visible = !showContinueNewMessage;
            if (showContinueNewMessage)
            {
                btnOK.Location = new Point(230, 270);
            }
            this.lblContinueNewMessage.Visible = showContinueNewMessage;
            this.lblContinueNew2.Visible = showContinueNewMessage;

            DisplayResults(data);
            
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Display auto search results
        /// </summary>
        public void DisplayResults(DataTable data)
        {
            dataTable = new DataTable();
            dataTable = data;

            dataGrid1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            float TotalFillWeight = 0.0f;

            dataGrid1.DataSource = dataTable;

            for (int i = 0; i < dataTable.Columns.Count && TotalFillWeight< 65436; i++)
            {
                TotalFillWeight += 100;
                //hide the "Unique Key" and "RecStatus" columns                    
                if (dataGrid1.Columns[i].Name.Equals("UniqueKey", StringComparison.OrdinalIgnoreCase) || dataGrid1.Columns[i].Name.Equals("RecStatus", StringComparison.OrdinalIgnoreCase))
                {
                    dataGrid1.Columns[i].Visible = false;
                }
            }
          
            if (this.dataTable.Rows.Count > 0)
            {
                GetCurrentRecord();
            }
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void GetCurrentRecord()
        {
            if (dataGrid1.Rows.Count > 0)
            {
                if (dataGrid1.CurrentRow == null)
                {
                    dataGrid1.CurrentCell = dataGrid1.Rows[0].Cells[0];
                }
                currentRow = dataGrid1.CurrentRow.Index;
                string record = dataGrid1.CurrentRow.Cells["UniqueKey"].Value.ToString();
                mainForm.RecordId = record;
            }
            else
            {

            }
        }


        /// <summary>
        /// Handles the Double Click event of the datagrid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void dataGrid1_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Handles the SelectionChanged event for the DataGridView control
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">EventArgs value</param>
        private void dataGrid1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGrid1.SelectedRows.Count > 0)
            {
                GetCurrentRecord();
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the KeyDown event for all controls to capture Esc and Enter keys
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">KeyEventArgs value</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.DialogResult = DialogResult.Cancel;
                    break;
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }

        #endregion    

    }
}

