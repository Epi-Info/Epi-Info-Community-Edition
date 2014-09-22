using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
    /// <summary>
    /// The Code Dialog
    /// </summary>
    public partial class ListDialog : LegalValuesDialog
    {

        #region Public Interface
        #region Constructors
        /// <summary>
        /// Default Constructor - Design mode only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public ListDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor of the Codes dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="name">The field's name</param>
        /// <param name="currentPage">The current page</param>
        public ListDialog(MainForm frm, string name, Page currentPage)
            : base(frm, name, currentPage)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            ddlField = new DDListField(page);
            ddlField.Name = fieldName;
            selectedFields = new NamedObjectCollection<Field>();
            SetDataSource(ddlField);
            SetDgCodes(dgCodes, fieldName);
        }

        /// <summary>
        /// Constructor of the Codes dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The field</param>
        /// <param name="currentPage">The current page</param>
        public ListDialog(MainForm frm, RenderableField field, Page currentPage)
            : base(frm, field, currentPage)
        {
            InitializeComponent();
            page = currentPage;
            ddlField = (DDListField)field;
            codeTable = ddlField.GetSourceData();
            this.Text = "List Field";
            //cbxSort.Checked = ddlField.ShouldSort;
            fieldName = ddlField.Name;
            SetDataSource(ddlField);
            SetDgCodes(dgCodes, fieldName);
        }

        /// <summary>
        /// Contructor of the CodesDialog
        /// </summary>
        /// <param name="field">The field</param>
        /// <param name="frm">The main form</param>
        /// <param name="name">The field name</param>
        /// <param name="currentPage">The current page</param>
        /// <param name="selectedItems">The names of the fields from the Code Field Definition dialog</param>
        public ListDialog(TableBasedDropDownField field, MainForm frm, string name, Page currentPage, NamedObjectCollection<Field> selectedItems)
            : base(frm, name, currentPage)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            ddlField = (DDListField)field;
            this.Text = "List Field";
            //if (!(string.IsNullOrEmpty(sourceTableName)))
            //{
            //    codeTable = ddlField.GetSourceData();
            //}
            selectedFields = selectedItems;
            SetDataSource(ddlField);
            SetDgCodes(dgCodes, fieldName);

        }
        #endregion Constructors

        #region Public Enums and Constants
        #endregion Public Enums and Constants

        #region Public Properties
        /// <summary>
        /// The source table name
        /// </summary>
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

        /// <summary>
        /// Text Column Name
        /// </summary>
        public new string TextColumnName
        {
            get
            {
                return (textColumnName);
            }
        }

        /// <summary>
        /// A collection of selected fields
        /// </summary>
        public NamedObjectCollection<Field> SelectedFields
        {
            get
            {
                return (selectedFields);
            }

        }
        #endregion Public Properties

        #region Public Methods

        #endregion Public Methods
        #endregion Public Interface

        #region Protected Interface

        #region Protected Properties

        #endregion Protected Properties

        #region Protected Methods

        #endregion Protected Methods

        #region Protected Events
        /// <summary>
        /// Handles click event of Create button
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnCreate_Click(object sender, System.EventArgs e)
        {
            CreateCodes();
            // don't necessarily want to do this for the user
            //foreach (Field field in SelectedFields)
            //{
            //    ((SingleLineTextField)field).IsReadOnly = true; 
            //    field.SaveToDb();
            //}
            btnCreate.Enabled = false;
            btnUseExisting.Enabled = false;
            dgCodes.Visible = true;
            btnOK.Enabled = true;
        }

        /// <summary>
        /// Handles click event of OK button
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnOK_Click(object sender, System.EventArgs e)
        {
            SaveShouldSort();
            if (codeTable == null)
            {
                //new code table
                SaveCodeTableToField();
            }
            else
            {
                //else, you are using an existing table 
                if ((DataTable)dgCodes.DataSource != null)
                {
                    DataTable dataTables = page.GetProject().CodeData.GetCodeTableNamesForProject(page.GetProject());
                    // def 1471 -- commented the foreach block
                    //    bool doesExist = false;
                    //foreach (DataRow row in dataTables.Rows)
                    //{
                    //    if ((codeTable.TableName).Equals(row[0]))
                    //    {
                    //        doesExist = true;
                    //    }
                    //}

                    // Added this condition -- Def 1471
                    //if (page.GetProject().CollectedData.TableExists(codeTable.TableName))
                    //{
                    //    if (!(page.GetProject().CollectedData.TableExists(codeTable.TableName + fieldName.ToLower())))
                    //    {
                    //        //to handle use existing, and the new column
                    //        codeTable.TableName = codeTable.TableName + fieldName.ToLower();
                    //        dgCodes.DataSource = codeTable;
                    //    }
                    //}
                    this.sourceTableName = codeTable.TableName;

                    int index = 0;
                    string[] columnsToSave = new string[codeTable.Columns.Count];
                    foreach (DataColumn column in codeTable.Columns)
                    {
                        columnsToSave[index] = codeTable.Columns[index].ColumnName;
                        index = index + 1;
                    }

                    page.GetProject().CreateCodeTable(codeTable.TableName, columnsToSave);                    
                    //page.GetProject().SaveCodeTableData(codeTable, codeTable.TableName, columnsToSave);
                    page.GetProject().InsertCodeTableData(codeTable, codeTable.TableName, columnsToSave);
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
        #endregion Protected Events

        #endregion Protected Interface

        #region Private Members

        #region Private Enums and Constants
        private double MULTICOLUMN_WIDTH_MULTIPLE = .4;
        #endregion Private Enums and Constants

        #region Private Properties
        private string fieldName = string.Empty;
        private DataTable codeTable;
        private DataTable valueTable;
        private DataTable fieldSetupTable;
        private DataTable fieldValueSetupTable;
        private DataTable finalTable;
        private Page page;
        private string sourceTableName = string.Empty;
        private string textColumnName = string.Empty;
        private NamedObjectCollection<Field> selectedFields;
        private new DDListField ddlField;

        #endregion Private Properties

        #region Private Methods
        private void SetDgCodes(DataGridView dgCodes, string fieldName)
        {
            //dgCodes.CaptionText = "List for: " + fieldName.ToLower();
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * MULTICOLUMN_WIDTH_MULTIPLE);
        }

        private void SetDataSource(DDListField ddlField)
        {
            if (!string.IsNullOrEmpty(ddlField.SourceTableName))
            {
                codeTable = ddlField.GetSourceData();
                sourceTableName = ddlField.SourceTableName;
                textColumnName = ddlField.TextColumnName;
            }
        }

        private void DisplayData()
        {
            if (codeTable != null)
            {
                dgCodes.Visible = true;
                btnOK.Enabled = true;
                btnCreate.Enabled = false;
                btnUseExisting.Enabled = false;
                btnDelete.Enabled = false;

                codeTable.TableName = sourceTableName;
                dgCodes.DataSource = codeTable;
                cbxSort.Checked = !ddlField.ShouldSort;

                btnOK.Visible = true;
                btnCreate.Enabled = false;
                btnUseExisting.Enabled = false;
                btnDelete.Visible = true;
            }

        }

        private void CreateCodes()
        {
            dgCodes.Visible = true;
            DataTable bindingTable = page.GetProject().CodeData.GetCodeTableNamesForProject(page.GetProject());
            DataView dataView = bindingTable.DefaultView;
            //if (dgCodes.AllowSorting)
            //{
            //    dataView.Sort = GetDisplayString(page);
            //}
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
                
                page.GetProject().CreateCodeTable(cleanedCodeTableName, selectedFieldsForCodeColumns);
            }
            else
            {
                page.GetProject().CreateCodeTable(cleanedCodeTableName, fieldName.ToLower());
            }

            codeTable = page.GetProject().GetTableData(cleanedCodeTableName);
            codeTable.TableName = cleanedCodeTableName;
            dgCodes.DataSource = codeTable;

            sourceTableName = codeTable.TableName;
            textColumnName = fieldName;
        }

        /// <summary>
        /// Link the code table up to the field and save
        /// </summary>
        private void SaveCodeTableToField()
        {
            DataTable dataTable = (DataTable)dgCodes.DataSource;
            if (dataTable != null)
            {
                if (dataTable.Columns.Count > 1)
                {
                    int index = 0;
                    string[] columnsToSave = new string[dataTable.Columns.Count];
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        columnsToSave[index] = dataTable.Columns[index].ColumnName;
                        index = index + 1;
                    }
                    page.GetProject().SaveCodeTableData(dataTable, dataTable.TableName, columnsToSave);
                }
                else
                {
                    page.GetProject().SaveCodeTableData(dataTable, dataTable.TableName, dataTable.Columns[0].ColumnName);
                }
                this.sourceTableName = dataTable.TableName;
                this.textColumnName = dataTable.Columns[0].ColumnName;
            }
        }

        private NamedObjectCollection<Field> ConvertToLower(NamedObjectCollection<Field> columnNames)
        {
            if (columnNames != null)
            {
                NamedObjectCollection<Field> columnNamesInLower = new NamedObjectCollection<Field>();
                columnNamesInLower = columnNames;

                int selectedIndex = 1;
                DataRowView item;
                string[] selectedViewFields = new string[lbxFields.SelectedItems.Count + 1];
                selectedViewFields[0] = fieldName;
                for (int i = 0; i < lbxFields.Items.Count; i++)
                {
                    item = (DataRowView)lbxFields.Items[i];
                    if (lbxFields.GetSelected(i))
                    {
                        selectedViewFields[selectedIndex] = item[lbxFields.DisplayMember].ToString();
                        DataRow selectRow = item.Row;
                        selectedFields.Add(page.GetView().GetFieldById(int.Parse((selectRow[ColumnNames.FIELD_ID].ToString()))));
                        selectedIndex++;
                    }
                }
                return columnNamesInLower;
            }
            return columnNames;
        }

        /// <summary>
        /// Set ShouldSort based on if the checkbox is checked
        /// </summary>
        private void SaveShouldSort()
        {
            if (ddlField != null)
            {
                ddlField.ShouldSort = !cbxSort.Checked;
            }
        }

        #endregion Private Methods

        #region Private Events
        /// <summary>
        /// Handles load event of the form
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void Codes_Load(object sender, System.EventArgs e)
        {
            DisplayData();
            if (dgCodes.DataSource != null)
            {
                btnOK.Enabled = true;
            }
            dgCodes.Focus();
        }
        #endregion Private Events

        #endregion Private Members

    }
}

