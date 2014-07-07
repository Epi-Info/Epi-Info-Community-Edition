#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Epi;
using Epi.Windows.Docking;

#endregion

namespace Epi.Windows.Analysis.Forms
{
    /// <summary>
    /// A form to display output in a DataGrid
    /// </summary>
    public partial class DataGridForm : DockWindow
    {
        #region Constructors
        /// <summary>
        /// Constructor for the dataGridForm
        /// </summary>
        /// <param name="frm">The parent form</param>
        public DataGridForm(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Current project per requirements for analysis
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return dataGridView1.ReadOnly;
            }
            set
            {
                dataGridView1.ReadOnly = value;
            }
        }

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Displays the output in the DataGrid
        /// </summary>
        /// <param name="results">Command Processor Results</param>
        /// <param name="actionId">Action GridTable or Update</param>
        public void SendToOutput(CommandProcessorResults results, Action actionId)
        {
            if (results != null && results.DsOutput != null)
            {   // DEFECT: 200 renamed xmlDataSet to dsResults to reflect use.
                DataSet dsResults = results.DsOutput;
                //xmlDataSet.ReadXml(new StringReader(results.XmlOutput.OuterXml));
                dataGridView1.DataSource = dsResults;
                dataGridView1.DataMember = dsResults.Tables[0].TableName;
                dataGridView1.ReadOnly = (actionId == Action.GridTable);
                if (dataGridView1.Columns.Contains(ColumnNames.REC_STATUS))
                {
                    dataGridView1.Columns[ColumnNames.REC_STATUS].ReadOnly = true;
                }
                if (dataGridView1.Columns.Contains(ColumnNames.REC_STATUS))
                {
                    dataGridView1.Columns[ColumnNames.UNIQUE_KEY].ReadOnly = true;
                }
                this.dataGridView1.Refresh();
                this.Text = ((actionId == Action.Update) ? SharedStrings.UPDATE_TITLE : 
                                                           SharedStrings.GRIDTABLE_TITLE);

                this.Show(this.Parent);
            }
        }
        #endregion Public Methods

        #region Private Attributes
        //private CommandProcessorResults results;
        //private Action actionId;
        //private DataSet xmlDataSet; // Deprecated to SendToOutput
        #endregion Private Attributes

        #region Event Handlers
        private void DataGridForm_Load(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // TODO: if update, Update DataSourceInfo dcs0
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (dataGridView1.CurrentCell.ReadOnly)  // This cell is read-only
            {
                throw new GeneralException(SharedStrings.MODIFICATION_NOT_PERMITTED);
            }
        }

        #endregion Event Handlers


    }

}

