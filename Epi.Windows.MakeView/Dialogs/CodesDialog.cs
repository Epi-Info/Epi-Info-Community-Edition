using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;

using Epi.Data;
using Epi.Fields;
using Epi.Collections;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class CodesDialog : LegalValuesDialog
    {
        private double MULTICOLUMN_WIDTH_MULTIPLE = .4;
        private string fieldName = string.Empty;

        private DataTable fieldSetupTable;
        private DataTable fieldValueSetupTable;

        private Page page;
        private NamedObjectCollection<Field> selectedFields;
        private new DDLFieldOfCodes ddlField;

        public KeyValuePairCollection kvPairs;
        public string relateCondition = string.Empty;
        
        #region Constructors
        /// <summary>
        /// Default Constructor - Design mode only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CodesDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Contructor of the CodesDialog
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="frm">The main form</param>
        /// <param name="name">The field name</param>
        /// <param name="currentPage">The current page</param>
        /// <param name="selectedItems">The names of the fields from the Code Field Definition dialog</param>
        public CodesDialog(TableBasedDropDownField field, MainForm frm, string name, Page currentPage, NamedObjectCollection<Field> selectedItems)
            : base(field, frm, name, currentPage)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            ddlField = (DDLFieldOfCodes)field;
            selectedFields = selectedItems;
            SetDataSource(ddlField);
            SetDgCodes(dgCodes, fieldName);
            dgCodes.Visible = true;
            relateCondition = ddlField.RelateConditionString;
        }
        #endregion Constructors

        #region Public Properties
        public new string SourceTableName
        {
            get
            {
                return (sourceTableName);
            }
            set
            {
                sourceTableName = value;
            }
        }

        public new string TextColumnName
        {
            get
            {
                return (textColumnName);
            }
        }

        public NamedObjectCollection<Field> SelectedFields
        {
            get
            {
                return (selectedFields);
            }
        }
        #endregion Public Properties

        #region Protected Methods
        protected override void ShowFieldSelection(string tableName)
        {
            ShowMatchFieldsDialog(tableName);
        }
        #endregion Protected Methods

        #region Protected Events
        
        // NEW 
        protected override void btnCreate_Click(object sender, System.EventArgs e)
        {
            creationMode = CreationMode.CreateNew;
            CreateCodes();

            btnCreate.Enabled = false;
            btnFromExisting.Enabled = false;
            btnUseExisting.Enabled = false;

            dgCodes.Visible = true;
            btnOK.Enabled = true;
            this.btnMatchFields.Enabled = true;
        }

        // NEW FROM EXISTING
        protected override void btnFromExisting_Click(object sender, System.EventArgs e)
        {
            creationMode = CreationMode.CreateNewFromExisting;
            ViewSelectionDialog dialog = new ViewSelectionDialog(MainForm, page.GetProject());
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                sourceTableName = dialog.TableName;
                dialog.Close();
                if (page.GetProject().CollectedData.TableExists(sourceTableName))
                {
                    codeTable = page.GetProject().GetTableData(sourceTableName);
                }
                else
                {
                    string separator = " - ";

                    if (sourceTableName.Contains(separator))
                    {
                        string[] view_page = sourceTableName.Replace(separator, "^").Split('^');
                        string viewName = view_page[0].ToString();
                        string pageName = view_page[1].ToString();
                        string filterExpression = string.Empty;
                        string tableName = null;
                        View targetView = page.GetProject().Metadata.GetViewByFullName(viewName);

                        if (targetView != null)
                        {
                            DataTable targetPages = page.GetProject().Metadata.GetPagesForView(targetView.Id);
                            DataView dataView = new DataView(targetPages);

                            filterExpression = string.Format("Name = '{0}'", pageName);

                            DataRow[] pageArray = targetPages.Select(filterExpression);

                            if (pageArray.Length > 0)
                            {
                                int pageId = (int)pageArray[0]["PageId"];
                                tableName = viewName + pageId;
                            }
                        }
                        if (page.GetProject().CollectedData.TableExists(tableName))
                        {
                            codeTable = page.GetProject().GetTableData(tableName);
                        }
                    }
                }
                

                if (codeTable != null)
                {
                    dgCodes.Visible = true;
                    codeTable.TableName = sourceTableName;
                    dgCodes.DataSource = codeTable;

                    if (DdlField != null) cbxSort.Checked = !DdlField.ShouldSort;
                    if (DdlColumn != null) cbxSort.Checked = !DdlColumn.ShouldSort;

                    btnCreate.Enabled = false;
                    btnFromExisting.Enabled = false;
                    btnUseExisting.Enabled = false;
                    btnDelete.Enabled = true;

                    btnMatchFields.Enabled = true;
                }
                else
                {
                    btnCreate.Enabled = true;
                    btnFromExisting.Enabled = true;
                    btnUseExisting.Enabled = true;
                    btnDelete.Enabled = false;

                    btnMatchFields.Enabled = false;
                }

                isExclusiveTable = true;
            }
        }

        // USE EXISTING
        protected override void btnUseExisting_Click(object sender, System.EventArgs e)
        {
            creationMode = CreationMode.UseExisting;

            ViewSelectionDialog dialog = new ViewSelectionDialog(MainForm, page.GetProject());
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                sourceTableName = dialog.TableName;
                dialog.Close();
                if (page.GetProject().CollectedData.TableExists(sourceTableName))
                {
                    codeTable = page.GetProject().GetTableData(sourceTableName);
                }
                else
                {
                    string separator = " - ";

                    if (sourceTableName.Contains(separator))
                    {
                        string[] view_page = sourceTableName.Replace(separator, "^").Split('^');
                        string viewName = view_page[0].ToString();
                        string pageName = view_page[1].ToString();
                        string filterExpression = string.Empty;
                        string tableName = null;
                        View targetView = page.GetProject().Metadata.GetViewByFullName(viewName);

                        if (targetView != null)
                        {
                            DataTable targetPages = page.GetProject().Metadata.GetPagesForView(targetView.Id);
                            DataView dataView = new DataView(targetPages);

                            filterExpression = string.Format("Name = '{0}'", pageName);

                            DataRow[] pageArray = targetPages.Select(filterExpression);

                            if (pageArray.Length > 0)
                            {
                                int pageId = (int)pageArray[0]["PageId"];
                                tableName = viewName + pageId;
                            }
                        }
                        if (page.GetProject().CollectedData.TableExists(tableName))
                        {
                            codeTable = page.GetProject().GetTableData(tableName);
                        }
                    }
                }

                if (codeTable != null)
                {
                    dgCodes.Visible = true;
                    codeTable.TableName = sourceTableName;
                    dgCodes.DataSource = codeTable;

                    if (DdlField != null) cbxSort.Checked = !DdlField.ShouldSort;
                    if (DdlColumn != null) cbxSort.Checked = !DdlColumn.ShouldSort;

                    btnCreate.Enabled = false;
                    btnFromExisting.Enabled = false;
                    btnUseExisting.Enabled = false;
                    btnDelete.Enabled = true;

                    btnMatchFields.Enabled = true;
                }
                else
                {
                    btnCreate.Enabled = true;
                    btnFromExisting.Enabled = true;
                    btnUseExisting.Enabled = true;
                    btnDelete.Enabled = false;
                    
                    btnMatchFields.Enabled = false;
                }

                isExclusiveTable = true;
            }
        }

        protected void CallMatchFieldDialogs()
        {
            ViewSelectionDialog dialog = new ViewSelectionDialog(MainForm, page.GetProject());
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                sourceTableName = dialog.TableName;
                dialog.Close();
                btnOK.Enabled = true;
            }
        }

        private void btnMatchFields_Click(object sender, EventArgs e)
        {
            if (this.sourceTableName != "")
            {
                SaveCodeTableToField();
                ShowMatchFieldsDialog(this.sourceTableName);
            }
        }

        #endregion Protected Events

        #region Private Methods
        private void SetDgCodes(DataGridView dgCodes, string fieldName)
        {
            //dgCodes.CaptionText = "Codes for: " + fieldName.ToLowerInvariant();
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * MULTICOLUMN_WIDTH_MULTIPLE);
        }

        private void SetDataSource(DDLFieldOfCodes ddlField)
        {
            if (!string.IsNullOrEmpty(ddlField.SourceTableName))
            {
                codeTable = ddlField.GetSourceData();
                sourceTableName = ddlField.SourceTableName;
                textColumnName = ddlField.TextColumnName;
            }
        }

        protected override void DisplayData()
        {
            bool relateFunctional = RelateConditionFunctional();

            if (codeTable == null)
            {
                btnCreate.Enabled = true;
                btnFromExisting.Enabled = true;
                btnUseExisting.Enabled = true;
                btnDelete.Enabled = false;

                btnOK.Enabled = false;
                btnMatchFields.Enabled = false;
            }
            else
            {
                codeTable.TableName = sourceTableName;
                dgCodes.DataSource = codeTable;
                //dgCodes.CaptionText = sourceTableName;
                dgCodes.Visible = true;

                btnCreate.Enabled = false;
                btnFromExisting.Enabled = false;
                btnUseExisting.Enabled = false;

                btnDelete.Enabled = true;
                cbxSort.Checked = !ddlField.ShouldSort;

                btnMatchFields.Enabled = true;

                if (relateFunctional)
                {
                    btnOK.Enabled = true;
                }
                else
                {
                    btnOK.Enabled = false;
                }
            }

            btnOK.Visible = true;
            btnDelete.Visible = true;
        }

        private bool RelateConditionFunctional()
        {
            if (string.IsNullOrEmpty(relateCondition))
            {
                return false;
            }

            return true;
        }

        private void CreateCodes()
        {
            dgCodes.Visible = true;

            Project project = page.GetProject();

            DataTable bindingTable = project.CodeData.GetCodeTableNamesForProject(project);
            DataView dataView = bindingTable.DefaultView;
            
            //if (dgCodes.AllowSorting)
            {
                dataView.Sort = GetDisplayString(page);
            }
            
            string cleanedCodeTableName = CleanCodeTableName(fieldName, dataView);

            if (SelectedFields.Count >= 1)
            {
                int i = 1;

                string[] selectedFieldsForCodeColumns = new string[SelectedFields.Count + 1];
                selectedFieldsForCodeColumns[0] = fieldName;

                foreach (Field field in SelectedFields)
                {
                    selectedFieldsForCodeColumns[i] = field.Name;
                    i += 1;
                }

                //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * MULTICOLUMN_WIDTH_MULTIPLE);

                project.CreateCodeTable(cleanedCodeTableName, selectedFieldsForCodeColumns);
            }
            else
            {
                project.CreateCodeTable(cleanedCodeTableName, fieldName.ToLowerInvariant());
            }

            codeTable = project.GetTableData(cleanedCodeTableName);
            codeTable.TableName = cleanedCodeTableName;
            dgCodes.DataSource = codeTable;

            sourceTableName = codeTable.TableName;
            textColumnName = fieldName;
            newCodeTable = codeTable;
        }

        private void ShowMatchFieldsDialog(string tableName)
        {
            Project project = page.GetProject();

            List<string> selectedFields = new List<string>();
            foreach(Field field in SelectedFields)
            {
                selectedFields.Add(field.Name);
            }

            Dictionary<string, string> fieldColumnNamePairs = new Dictionary<string, string>();

            foreach (KeyValuePair<string, int> kvp in ddlField.PairAssociated)
            {
                Field fieldById = page.view.GetFieldById(kvp.Value);
                fieldColumnNamePairs.Add(fieldById.Name, kvp.Key);
            }

            MatchFieldsDialog dialog = new MatchFieldsDialog
                (
                MainForm, 
                project, 
                tableName, 
                selectedFields,
                ddlField.TextColumnName,
                fieldColumnNamePairs);
            
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string valueTableName = tableName.ToString() + "value";

                fieldSetupTable = new DataTable(tableName);
                fieldSetupTable.Columns.Add(fieldName, dialog.PrimaryColumnData.GetType());
                fieldValueSetupTable = new DataTable(valueTableName);

                if (project.CollectedData.TableExists(tableName))
                {
                    fieldValueSetupTable = project.GetTableData(tableName, dialog.PrimaryColumnData.ToString());
                }

                newCodeTable = new DataTable();
                newCodeTable = fieldValueSetupTable.Clone();
                newCodeTable.Clear();

                if (newCodeTable.Columns.Count > 0)
                {
                    newCodeTable.Columns.RemoveAt(0);
                }
                
                newCodeTable.Columns.Add(fieldName, dialog.PrimaryColumnData.GetType());

                foreach (DataRow row in fieldValueSetupTable.Rows)
                {
                    DataRow rowToAdd = newCodeTable.NewRow();
                    rowToAdd[0] = row[0];
                    newCodeTable.Rows.Add(rowToAdd);
                }

                kvPairs = dialog.Codes;

                System.Collections.ArrayList codeColumns = new System.Collections.ArrayList();
                string columnNames = string.Empty;

                foreach (KeyValuePair kvPair in kvPairs)
                {
                    codeColumns.Add(kvPair.Value);
                    columnNames += kvPair.Value.ToString() + StringLiterals.COMMA;
                }

                if (columnNames.Length > 1)
                {
                    columnNames = columnNames.Substring(0, (columnNames.Length - 1));
                }

                sourceTableName = tableName;
                textColumnName = dialog.PrimaryColumnData;

                if (Mode == CreationMode.CreateNewFromExisting)
                {
                    textColumnName = fieldName;

                    string[] comma = { "," };
                    string column = string.Empty;

                    foreach (KeyValuePair kvPair in kvPairs)
                    {
                        column += kvPair.Key.ToString() + StringLiterals.COMMA;
                    }

                    if (column.Length > 1)
                    {
                        column = column.Substring(0, (column.Length - 1));
                    }

                    string[] columns = column.Split(comma, StringSplitOptions.None);

                    for (int i = 0; i < columns.Length; i++)
                    {
                        newCodeTable.Columns.Add(columns[i]);
                    }

                    string columnNamesForValues = string.Empty;

                    foreach (KeyValuePair kvPair in kvPairs)
                    {
                        columnNamesForValues += kvPair.Value.ToString() + StringLiterals.COMMA;
                    }

                    if (columnNamesForValues.Length > 1)
                    {
                        columnNamesForValues = columnNamesForValues.Substring(0, (columnNamesForValues.Length - 1));
                    }

                    string newTableColumnName = string.Empty;
                    string existingColumnName = string.Empty;

                    foreach (DataColumn dataColumnOfNewTable in newCodeTable.Columns)
                    {
                        newTableColumnName = dataColumnOfNewTable.ColumnName;

                        foreach (KeyValuePair kvPair in kvPairs)
                        {
                            if(kvPair.Key == newTableColumnName)
                            {
                                existingColumnName = kvPair.Value;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(existingColumnName) == false)
                        { 
                            for (int i = 0; i < codeTable.Rows.Count; i++)
                            {
                                newCodeTable.Rows[i][newTableColumnName] = codeTable.Rows[i][existingColumnName];
                            }
                        }
                    }

                    sourceTableName = GetNewCodeTableName(FieldName);
                    newCodeTable.TableName = sourceTableName;
                    codeTable = newCodeTable;
                }
                else
                {
                    relateCondition = string.Empty;
                    
                    foreach (KeyValuePair kvPair in kvPairs)
                    {
                        relateCondition = relateCondition.Length > 0 ? relateCondition + "," : relateCondition;
                        int fieldId = this.page.Fields[kvPair.Key].Id;
                        relateCondition = string.Format("{0}{1}:{2}", relateCondition, kvPair.Value, fieldId);
                    }
                }

                dialog.Close();
                DisplayData();
            }
        }

        private void SaveShouldSort()
        {
            if (ddlField != null)
            {
                ddlField.ShouldSort = !cbxSort.Checked;
            }
        }

        #endregion Private Methods
    }
}

