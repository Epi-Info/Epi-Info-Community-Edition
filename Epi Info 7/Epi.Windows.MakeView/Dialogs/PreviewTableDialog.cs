using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Epi;
using Epi.Fields;
using Epi.Collections;
using Epi.Windows.Dialogs;


namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// The Create New Table dialog
    /// </summary>
    public partial class PreviewTableDialog : DialogBase
    {

        #region Private Class Members
        private DataTable codeTable = null;
        private string fieldName = string.Empty;
        //private NamedObjectCollection<Field> fields;
        private ComboBox.ObjectCollection fields;
        private Project currentProject;
        private string currentTableName;
        private string columnNames = string.Empty;
        private List<string> columnCollection;
        private DataTable previewTable;
        /// <summary>
        /// Table adapter
        /// </summary>
        public DataAdapter tableAdapter;

        #endregion Private Class Members

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public PreviewTableDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Preview Table Dialog dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="project">The current project</param>
        /// <param name="tableFields">Selected fields collection</param>
        /// <param name="tableName">The table name</param>
        public PreviewTableDialog(MainForm frm, Project project, ComboBox.ObjectCollection tableFields, string tableName)
            : base(frm)
        {
            InitializeComponent();
            currentProject = project;
            currentTableName = tableName;
            fields = tableFields;
            columnCollection = new List<string>();
            btnOK.Visible = false;
            lblInstruction.Text = "The grid below displays the fields from the selected table.  Click Back to go to the Match Fields screen.";
        }

        /// <summary>
        /// Handles the Load event of Preview Table Dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewTableDialog_Load(object sender, System.EventArgs e)
        {
            CreateTableForPreview();
        }

        /// <summary>
        /// Set the styling for the data grid
        /// </summary>
        /// <param name="colNames">The column names to style</param>
        private void SetTableStyles(string[] colNames)
        {
            this.dataGridTableStyle.MappingName = "code" + fieldName;
            foreach (string column in colNames)
            {
                DataGridTextBoxColumn textColumn = new DataGridTextBoxColumn();
                textColumn.MappingName = column;
                textColumn.HeaderText = column;
                textColumn.NullText = string.Empty;
                this.dataGridTableStyle.GridColumnStyles.Add(textColumn);
            }
        }

        /// <summary>
        /// Handles the Click event for the OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            string[] columnNames = new string[codeTable.Columns.Count];
            for (int x = 0; x < codeTable.Columns.Count; x++)
            {
                columnNames[x] = codeTable.Columns[x].ColumnName;
            }
            if (codeTable.TableName != string.Empty)
            {
                //        currentProject.SaveCodeTableData(codeTable, codeTable.TableName, columnNames);
            }
        }

        /// <summary>
        /// Handles the Click event for the Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Create the table to preview
        /// </summary>
        private void CreateTableForPreview()
        {
            if (codeTable == null)
            {
                EstablishColumns();
                GetSelectedFieldsData();
                DisplayData();
            }
        }

        /// <summary>
        /// Build the fields that need to exist in the grid
        /// </summary>
        private void EstablishColumns()
        {
            string[] colNames = new string[fields.Count + 1];

            int index = 0;
            if (fields.Count > 0)
            {
                foreach (string field in fields)
                {
                    columnNames += "[" + field + "]";
                    colNames[index] = field;
                    if (fields.Count > 0 && index < fields.Count)//1
                    {
                        if (!(String.IsNullOrEmpty(columnNames)))
                        {
                            columnNames += StringLiterals.COMMA;
                            columnCollection.Add(field);
                        }
                    }
                    index++;
                }

                columnNames = columnNames.Substring(0, (columnNames.Length - 1));
            }

            SetTableStyles(colNames);
        }

        /// <summary>
        /// Get the data that will populate the fields that were just established
        /// </summary>
        private void GetSelectedFieldsData()
        {
            DataTable previewDataTable = new DataTable("previewDataTable");

            foreach (string column in columnCollection)
            {
                DataColumn tableColumn = new DataColumn(column, Type.GetType("System.String"));
                previewDataTable.Columns.Add(tableColumn);
            }

            if (currentProject.CollectedData.TableExists(currentTableName))
            {
                previewTable = currentProject.GetTableData(currentTableName, columnNames.ToString(), string.Empty);
            }
            else
            {


            }

            DataView readOnlyView = new DataView(previewTable);
            readOnlyView.AllowNew = false;
            readOnlyView.AllowEdit = false;
            readOnlyView.AllowDelete = false;

            dgCodes.DataSource = readOnlyView;    
        }

        /// <summary>
        /// Show the data grid with the data
        /// </summary>
        private void DisplayData()
        {
            if (codeTable != null)
            {
                dgCodes.DataSource = codeTable.DefaultView;
                btnOK.Visible = true;
            }
        }
    }
}
