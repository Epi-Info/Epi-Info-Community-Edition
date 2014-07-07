using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Epi.Data;
using Epi.Fields;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{

    /// <summary>
    /// Dialog for selecting the data source of a dropdown field
    /// </summary>
    public partial class DataSourceSelector : DialogBase //Form
    {
        #region Private Class Members
        private string fieldName = string.Empty;
        private DataTable codeTable;
        private Page page;
        string tableName;
        string columnName;
        private bool isExclusiveTable;
        private TableBasedDropDownField field;
        private string sourceTableName = string.Empty;
        private string textColumnName = string.Empty;
        private bool isCommentLegal = false;

        #endregion Private Class Members

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public DataSourceSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="field">field</param>
        /// <param name="currentFieldName">The name of the field</param>
        /// <param name="frm">Form</param>
        /// <param name="currentPage">Page</param>
        public DataSourceSelector(TableBasedDropDownField field, string currentFieldName, MainForm frm, Page currentPage)
            : base(frm)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(currentFieldName))
            {
                throw new ArgumentNullException("currentFieldName");
            }
            #endregion Input validation

            InitializeComponent();
            page = currentPage;
            TableBasedDropDownField ddlField = (TableBasedDropDownField)field;

            if (!string.IsNullOrEmpty(field.SourceTableName))
            {
                codeTable = ddlField.GetSourceData();
            }

            this.field = field;

            if (field.FieldType == MetaFieldType.CommentLegal)
            {
                isCommentLegal = true;
            }
            else
            {
                isCommentLegal = false;
            }

            this.fieldName = currentFieldName;
            dgCodeTable.CaptionText += currentFieldName;

            try
            {
                if (!string.IsNullOrEmpty(field.SourceTableName))
                {
                    tableName = field.SourceTableName;
                    columnName = field.TextColumnName;
                    dgCodeTable.PreferredColumnWidth = Convert.ToInt32(dgCodeTable.Width * .87); 
                    dgCodeTable.DataSource = field.GetSourceData();
                    btnCreate.Enabled = false;
                    btnExisting.Enabled = false;
                }
                else
                {
                    btnCreate.Enabled = true;
                    btnExisting.Enabled = true;
                }
            }
            catch
            {
                //TODO: move string to sharedstrings.resx
                throw new SystemException("Error loading data.");
            }

        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="currentFieldName">The name of the field</param>
        public DataSourceSelector(DDLFieldOfLegalValues field, string currentFieldName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(currentFieldName))
            {
                throw new ArgumentNullException("currentFieldName");
            }
            #endregion Input validation

            InitializeComponent();
            this.field = field;
            this.fieldName = currentFieldName;
            dgCodeTable.CaptionText += currentFieldName;

            try
            {
                if (!string.IsNullOrEmpty(field.SourceTableName))
                {
                    tableName = field.SourceTableName;
                    columnName = field.TextColumnName;
                    dgCodeTable.PreferredColumnWidth = Convert.ToInt32(dgCodeTable.Width * .87); 
                    dgCodeTable.DataSource = field.GetSourceData();
                    btnCreate.Enabled = false;
                    btnExisting.Enabled = false;
                }
            }
            catch
            {
                //TODO: move string to sharedstrings.resx
                throw new SystemException("Error loading data.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            CodeTableDefinition codeTableDialog = new CodeTableDefinition();
            DialogResult result = codeTableDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                DataTable bindingTable = GetProject().CodeData.GetCodeTableNamesForProject(GetProject());
                DataView dataView = bindingTable.DefaultView;
                string display = String.Empty;
                string kindOfDatabase = GetProject().CodeData.IdentifyDatabase();

                //TODO: Will need to rework this section not to be dependant on strings and develop a 
                // more OO solution
                switch (kindOfDatabase)
                {
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

                // Check if this table already exists. If so, increment a suffix counter.
                tableName = Util.Squeeze(codeTableDialog.CodeTableName);
                string newTableName = tableName;
                int count = 0;
                while (dataView.Find(newTableName)!=-1)
                {
                    count++;
                    newTableName = tableName + count;
                }
                tableName = newTableName;
                columnName = Util.Squeeze(codeTableDialog.CodeTableRootName);
                if(!Util.IsFirstCharacterALetter(columnName)) columnName = "col" + columnName;
                DataTable table = new DataTable(tableName);
                table.Columns.Add(columnName);
                dgCodeTable.PreferredColumnWidth = Convert.ToInt32(dgCodeTable.Width * .87);
                dgCodeTable.DataSource = table;
                codeTableDialog.Close();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TableBasedDropDownField ddlField = (TableBasedDropDownField)field;

            //if this is a new code table, use tableName
            //otherwise, use sourceTableName
            if (!string.IsNullOrEmpty(tableName))
            {
                field.SourceTableName = tableName;
            }
            else if (!string.IsNullOrEmpty(sourceTableName))
            {
                field.SourceTableName = sourceTableName;
            }
            else
            {
                throw new ArgumentNullException("Table name for code table");
            }

            //if this is a new code table, use columnName
            //otherwise, use textColumnName
            if (!string.IsNullOrEmpty(columnName))
            {
                field.TextColumnName = columnName;
            }
            else if (!string.IsNullOrEmpty(textColumnName))
            {
                field.TextColumnName = textColumnName;
            }
            else
            {
                throw new ArgumentNullException("Column name for code table");
            }

            if ((!string.IsNullOrEmpty(tableName)) && (!string.IsNullOrEmpty(columnName)))
            {
                field.GetMetadata().CreateCodeTable(tableName, columnName);
                field.GetMetadata().SaveCodeTableData((DataTable)dgCodeTable.DataSource, tableName, columnName);
            }
            if (!string.IsNullOrEmpty(field.SourceTableName))
            {
                codeTable = ddlField.GetSourceData();
            }

            if (codeTable != null)
            {
                foreach (DataRow row in codeTable.Rows)
                {
                    if (isCommentLegal)
                    {
                        if (((row.ItemArray[0].ToString().IndexOf("-") == -1)))
                        {
                            string msg = SharedStrings.SEPARATE_COMMENT_LEGAL_WITH_HYPEN + ": \n" + row.ItemArray[0].ToString();
                            MsgBox.ShowError(msg);
                            return;
                        }
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Gets the table name
        /// </summary>
        public string TableName
        {
            get
            {
                return this.tableName;
            }
        }

        /// <summary>
        /// Gets the column name
        /// </summary>
        public string ColumnName
        {
            get
            {
                return this.columnName;
            }
        }

        /// <summary>
        /// Returns boolean to specify whether or not it is an exclusive table
        /// </summary>
        public bool IsExclusiveTable
        {
            get
            {
                return (!isExclusiveTable);
            }
        }

        private void DataSourceSelector_Load(object sender, System.EventArgs e)
        {
            DisplayData();
            dgCodeTable.Focus();

        }

        private void btnExisting_Click(object sender, EventArgs e)
        {
            ViewSelectionDialog dialog = new ViewSelectionDialog(this.MainForm, page.GetProject());
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                sourceTableName = dialog.TableName;
                dialog.Close();
                ShowFieldSelection(sourceTableName);
            }
        }
        private void ShowFieldSelection(string tableName)
        {
            FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName);
            DialogResult result = fieldSelection.ShowDialog();
            if (result == DialogResult.OK)
            {
                textColumnName = fieldSelection.ColumnName;
                codeTable = GetProject().GetTableData(tableName, textColumnName, string.Empty);
                fieldSelection.Close();
                DisplayData();
                isExclusiveTable = true;
            }

        }
        private Project GetProject()
        {
            return page.GetProject();
        }

        private void DisplayData()
        {
            if (codeTable != null)
            {
                dgCodeTable.Visible = true;
                btnOk.Enabled = true;
                btnCreate.Enabled = false;
                btnExisting.Enabled = false;
                btnDelete.Enabled = true;
                codeTable.TableName = sourceTableName;
                dgCodeTable.PreferredColumnWidth =Convert.ToInt32( dgCodeTable.Width * .87);
                dgCodeTable.DataSource = codeTable;
                dgCodeTable.CaptionText = sourceTableName;
            }
        }
    }
}
