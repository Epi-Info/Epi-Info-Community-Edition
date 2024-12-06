using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using Epi;
using Epi.Data;
using Epi.Windows;
using Epi.Fields;
using Epi.Windows.Dialogs;


namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// The Legal Values dialog
    /// </summary>
    public partial class LegalValuesDialog : DialogBase
    {
        #region Constants
        private double COLUMN_WIDTH_MULTIPLE = .87;
        #endregion

        #region Private Class Members
        private string fieldName = string.Empty;
        private Page page;
        private int newCurrentRow = -1;
        private int oldCurrentRow;
        private string newTableName = string.Empty;
        private ContextMenu _rightClickColumnMenu = new ContextMenu();
        private MenuItem insertColumnMenuItem = new MenuItem("Insert Column");
        private MenuItem removeColumnMenuItem = new MenuItem("Remove Column");
        private ContextMenu _rightClickRowMenu = new ContextMenu();
        private MenuItem insertRowMenuItem = new MenuItem("Insert Row");
        private Point clickPosition = new Point(0, 0);
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        #endregion Private Class Members

        #region Protected Class Members
        protected CreationMode creationMode = CreationMode.CreateNew;
        protected DataTable codeTable;
        protected DataTable newCodeTable;
        protected string sourceTableName = string.Empty;
        protected string textColumnName = string.Empty;
        protected bool isExclusiveTable;
        #endregion

        private TableBasedDropDownField ddlField;

        public TableBasedDropDownField DdlField
        {
            get { return ddlField; }
            set { ddlField = value; }
        }

        private TableBasedDropDownColumn ddlColumn;

        public TableBasedDropDownColumn DdlColumn
        {
            get { return ddlColumn; }
            set { ddlColumn = value; }
        }

        #region Constructors
        [Obsolete("Use of default constructor not allowed", true)]
        public LegalValuesDialog()
        {
            InitializeComponent();
        }

        public LegalValuesDialog(MainForm frm, string name, Page currentPage) : base(frm)
		{
            InitializeComponent();
            InitContextMenu();
			fieldName = name;
			page = currentPage;
            ddlField = new DDLFieldOfLegalValues(page);
			ddlField.Name = name;
            //dgCodes.CaptionText += "  " + name;
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * COLUMN_WIDTH_MULTIPLE);

            if (!string.IsNullOrEmpty(ddlField.SourceTableName))
            {
                codeTable = ddlField.GetSourceData();
                sourceTableName = ddlField.SourceTableName;
                textColumnName = ddlField.TextColumnName;
            }
		}

		public LegalValuesDialog(MainForm frm, RenderableField field, Page currentPage) : base(frm)
		{
            InitializeComponent();
			page = currentPage;
            ddlField = (DDLFieldOfLegalValues)field;
            codeTable = ddlField.GetSourceData();
			cbxSort.Checked = ddlField.ShouldSort;
			fieldName = ddlField.Name;
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * COLUMN_WIDTH_MULTIPLE);
		}

        public LegalValuesDialog(TableBasedDropDownField field, MainForm frm, string name, Page currentPage) : base(frm)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            creationMode = CreationMode.Edit;
            //dgCodes.CaptionText += "  " + name;
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * COLUMN_WIDTH_MULTIPLE);
            ddlField = field;

            if (!string.IsNullOrEmpty(field.SourceTableName))
            {
                codeTable = field.GetSourceData();
                sourceTableName = field.SourceTableName;
                textColumnName = field.TextColumnName;
            }

            InitContextMenu();
        }

        public LegalValuesDialog(TableBasedDropDownColumn column, MainForm frm, string name, Page currentPage) : base(frm)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            //dgCodes.CaptionText += "  " + name;
            //dgCodes.PreferredColumnWidth = Convert.ToInt32(dgCodes.Width * COLUMN_WIDTH_MULTIPLE);
            ddlColumn = column;

            if (!string.IsNullOrEmpty(column.SourceTableName))
            {
                codeTable = column.GetSourceData();
                sourceTableName = column.SourceTableName;
                textColumnName = column.TextColumnName;
            }
        }

        public LegalValuesDialog(RenderableField field, Page currentPage)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(field.ToString()))
            {
                throw new ArgumentNullException("field");
            }
            if (string.IsNullOrEmpty(currentPage.ToString()))
            {
                throw new ArgumentNullException("currentPage");
            }

            ddlField = (DDLFieldOfLegalValues)field;
            page = currentPage;
            codeTable = ddlField.GetSourceData();
            cbxSort.Checked = ddlField.ShouldSort;
            fieldName = ddlField.Name;
            sourceTableName = ddlField.SourceTableName;
            textColumnName = ddlField.TextColumnName;
        }

        private void InitContextMenu()
        {
            insertColumnMenuItem.Click += new EventHandler(insertColumnMenuItem_Click);
            _rightClickColumnMenu.MenuItems.Add(insertColumnMenuItem);

            removeColumnMenuItem.Click += new EventHandler(removeColumnMenuItem_Click);
            _rightClickColumnMenu.MenuItems.Add(removeColumnMenuItem);

            insertRowMenuItem.Click += new EventHandler(insertRowMenuItem_Click);
            _rightClickRowMenu.MenuItems.Add(insertRowMenuItem);
        }

        #endregion Constructors

        #region Public Properties

        public CreationMode Mode
        {
            get
            {
                return creationMode;
            }
            
            set
            {
                creationMode = value;
            }
        }

        public string FieldName
        {
            get
            {
                return fieldName;
            }
        }

        public void DisableUseExisting()
        {
            btnUseExisting.Enabled = false;
        }

        /// <summary>
        ///  Returns boolean of Sort checkbox checked state
        /// </summary>
        public bool ShouldSort
        {
            get
            {
                return (!cbxSort.Checked);
            }
        }

        /// <summary>
        /// Gets the source table name.
        /// </summary>
        public string SourceTableName
        {
            get
            {
                return (sourceTableName);
            }
        }

        /// <summary>
        /// Gets the text column name.
        /// </summary>
        public string TextColumnName
        {
            get
            {
                return (textColumnName);
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
        #endregion Public Properties

        #region Event Handlers

        /// <summary>
        /// Cancel button closes this dialog 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Delete button deletes the link to the code table
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void btnDelete_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MsgBox.ShowQuestion(SharedStrings.DELETE_LINK_TO_CODE_TABLE);
            if (result == DialogResult.Yes)
            {
                DeleteCodeTableLink();
            }
        }

        /// <summary>
        /// Display the data for LegalValues
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void Dialog_Load(object sender, System.EventArgs e)
        {
            DisplayData();
            dgCodes.Focus();
        }

        /// <summary>
        /// OK button saves and closes this dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void btnOK_Click(object sender, System.EventArgs e)
        {
            if (DdlField != null)
            {
                DdlField.ShouldSort = ShouldSort;
            }

            if (DdlColumn != null)
            {
                DdlColumn.ShouldSort = ShouldSort;
            }

            SaveCodeTableToField();
           
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Create New button creates a new code table
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void btnCreate_Click(object sender, System.EventArgs e)
        {
            CreateLegalValues();
            btnCreate.Enabled = false;
            btnFromExisting.Enabled = false;
            btnUseExisting.Enabled = false;
        }

        protected virtual void btnFromExisting_Click(object sender, System.EventArgs e)
        {
            creationMode = CreationMode.CreateNewFromExisting;
            CallMatchFieldDialogs();
        }

        protected virtual void btnUseExisting_Click(object sender, System.EventArgs e)
        {
            creationMode = CreationMode.UseExisting;
            CallMatchFieldDialogs();
            //this.DialogResult = DialogResult.OK;
            //this.Hide();
        }

        protected void CallMatchFieldDialogs()
        {
            ViewSelectionDialog dialog = new ViewSelectionDialog(MainForm, page.GetProject());
            DialogResult result = dialog.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                sourceTableName = dialog.TableName;
                dialog.Close();
                ShowFieldSelection(sourceTableName);
                btnOK.Enabled = true;
            }
        }

        
        /// <summary>
        /// Event from when the Sort check box is selected/deselected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void cbxSort_CheckedChanged(object sender, System.EventArgs e)
        {
            if (cbxSort.Checked)
            {
                //dgCodes.AllowSorting = false;
            }
            else
            {
                //dgCodes.AllowSorting = true;
            }
        }

        private void dgCodes_DataSourceChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = true;
        }

        void dgCodes_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = dgCodes.DoDragDrop(dgCodes.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        void dgCodes_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            Point clientPoint = dgCodes.PointToClient(new Point(e.X, e.Y));

            rowIndexOfItemUnderMouseToDrop = dgCodes.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (codeTable != null)
            { 
                if (e.Effect == DragDropEffects.Move && rowIndexFromMouseDown < codeTable.Rows.Count)
                {
                    DataRow cutRow = codeTable.NewRow();
                    cutRow.ItemArray = codeTable.Rows[rowIndexFromMouseDown].ItemArray;
                    codeTable.Rows.Remove(codeTable.Rows[rowIndexFromMouseDown]);
                    codeTable.Rows.InsertAt(cutRow, rowIndexOfItemUnderMouseToDrop);
                }
            }
        }

        void dgCodes_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        void dgCodes_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rowIndexFromMouseDown = dgCodes.HitTest(e.X, e.Y).RowIndex;
            
            if (rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            { 
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }


        void dgCodes_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int columnIndex = dgCodes.HitTest(e.X, e.Y).ColumnIndex;
                int rowIndex = dgCodes.HitTest(e.X, e.Y).RowIndex;
                clickPosition = e.Location;

                if (columnIndex >= 0 && rowIndex == -1)
                {
                    _rightClickColumnMenu.Show(this, clickPosition);
                }
                else if (columnIndex == -1 && rowIndex >= 0)
                {
                    _rightClickRowMenu.Show(this, clickPosition);
                }
            }
        }

        void insertColumnMenuItem_Click(object sender, EventArgs e)
        {
            if (dgCodes.DataSource is DataTable)
            {
                DataTable table = (DataTable)dgCodes.DataSource;
                NewColumnNameDialog dialog = new NewColumnNameDialog();
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    table.Columns.Add(dialog.ColumnName);
                    table.AcceptChanges();
                }
                
                dialog.Dispose();
            }
        }

        void removeColumnMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DataGridView.HitTestInfo hti = dgCodes.HitTest(clickPosition.X, clickPosition.Y);

            if (dgCodes.DataSource is DataTable)
            {
                DataTable table = (DataTable)dgCodes.DataSource;
                int columnCount = table.Columns.Count;

                if (hti.ColumnIndex >= 0 && hti.ColumnIndex < columnCount)
                {
                    DialogResult result = MessageBox.Show(
                        string.Format("Select OK to delete the column named '{0}' from this table.", table.Columns[hti.ColumnIndex].ColumnName), 
                        "Delete Column", 
                        MessageBoxButtons.OKCancel);

                    if (result == DialogResult.OK)
                    {
                        table.Columns.RemoveAt(hti.ColumnIndex);
                        table.AcceptChanges();
                    }
                }
            }
        }

        void insertRowMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DataGridView.HitTestInfo hti = dgCodes.HitTest(clickPosition.X, clickPosition.Y);
            
            if (dgCodes.DataSource is DataTable)
            {
                try
                {
                    DataTable table = (DataTable)dgCodes.DataSource;
                    table.Rows.InsertAt(table.NewRow(), hti.RowIndex);
                    table.AcceptChanges();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("nulls"))
                    {
                        DataTable table = (DataTable)dgCodes.DataSource;
                        DataRow blankRow = table.NewRow();
                        foreach (DataColumn c  in table.Columns)
                        {
                            blankRow[c.ColumnName] = " ";
                        }
                        table.Rows.InsertAt(blankRow, hti.RowIndex);
                        table.AcceptChanges();
                    }
                }
            }
        }

        #endregion Event Handlers

        #region Private/Protected Methods

        /// <summary>
        /// Delete the link to the code table, it does not delete the code table itself
        /// </summary>
        protected void DeleteCodeTableLink()
        {
            codeTable = null;
            dgCodes.DataSource = null;
            this.sourceTableName = string.Empty;
            this.textColumnName = string.Empty;

            btnCreate.Enabled = true;
            btnFromExisting.Enabled = true;
            btnUseExisting.Enabled = true;
            
            btnDelete.Enabled = false;
        }

        /// <summary>
        /// Create the legal values by creating the code table and linking to field
        /// </summary>
        private void CreateLegalValues()
        {
            dgCodes.Visible = true;
            btnOK.Visible = true;
            btnOK.Enabled = true;

            DataTable bindingTable = page.GetProject().CodeData.GetCodeTableNamesForProject(page.GetProject());
            DataView dataView = bindingTable.DefaultView;
            //if (dgCodes.AllowSorting)
            //{
            //    if (DdlField != null)
            //    {
            //        DdlField.ShouldSort = true;
            //    }

            //    if (DdlColumn != null)
            //    {
            //        DdlColumn.ShouldSort = true;
            //    }

            //    dataView.Sort = GetDisplayString(page);
            //}
            string cleanedCodeTableName = CleanCodeTableName(fieldName, dataView);

            page.GetProject().CreateCodeTable(cleanedCodeTableName, fieldName.ToLowerInvariant());
            codeTable = page.GetProject().GetTableData(cleanedCodeTableName);
            codeTable.TableName = cleanedCodeTableName;

            this.dataGridTableStyle1.MappingName = cleanedCodeTableName;
            this.dataGridTextBoxColumn1.MappingName = fieldName;
            this.dataGridTextBoxColumn1.HeaderText = fieldName;
            //this.dataGridTextBoxColumn1.Width = this.dgCodes.PreferredColumnWidth;

            dgCodes.DataSource = codeTable;

            if (dgCodes.CurrentCell != null)
            {
                dgCodes.BeginEdit(true);
            }
            
            sourceTableName = codeTable.TableName;
            textColumnName = fieldName;

            //dgCodes.AllowSorting = true;
        }

        /// <summary>
        /// Determine sorting column display based on database type
        /// </summary>
        /// <param name="page">Page</param>
        /// <returns></returns>
        protected string GetDisplayString(Page page)
        {
            string display = String.Empty;
            string kindOfDatabase = page.GetProject().CodeData.IdentifyDatabase();

            //TODO: Will need to rework this section not to be dependant on strings and develop a 
            // more OO solution
            switch (kindOfDatabase)
            {
                case "ACCESS":
                    display = ColumnNames.NAME;
                    break;
                case "SQLite":
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
            return display;
        }

        
        protected string GetNewCodeTableName(string fieldName)
        {
            DataTable bindingTable = page.GetProject().CodeData.GetCodeTableNamesForProject(page.GetProject());
            DataView dataView = bindingTable.DefaultView;
            return CleanCodeTableName(fieldName, dataView);
        }
        
        /// <summary>
        /// Create an acceptable tablename from a fieldname
        /// </summary>
        /// <param name="fieldName">Field name to turn into a tablename</param>
        /// <param name="dataView">DataView to search to make sure this table does not already exist</param>
        /// <returns></returns>
        protected string CleanCodeTableName(string fieldName, DataView dataView)
        {
            fieldName = "code" + fieldName;

            fieldName = fieldName.Replace("-", string.Empty);
            fieldName = fieldName.ToLowerInvariant();

            string tableName = Util.Squeeze(fieldName);
            string newTableName = tableName;
         
            tableName = newTableName;
            string columnName = Util.Squeeze(fieldName);

            int count = 0;
            bool uniqueNameFound = false;

            do
            {
                string displayName = GetDisplayString(page); 
                DataRow[] rows = dataView.Table.Select(string.Format("{0} = '{1}'", displayName, newTableName));
                if (rows.Length == 0) uniqueNameFound = true;
                count++;
                newTableName = tableName + count;
            }
            while (uniqueNameFound == false);

            if (!Util.IsFirstCharacterALetter(columnName))
            {
                columnName = "col" + columnName;
            }
            
            return newTableName;
        }

        protected virtual void DisplayData()
        {
            if (codeTable != null)
            {
                dgCodes.Visible = true;
                codeTable.TableName = sourceTableName;
                this.dataGridTableStyle1.MappingName = sourceTableName;
                this.dataGridTextBoxColumn1.MappingName = textColumnName;
                this.dataGridTextBoxColumn1.HeaderText = textColumnName;
                //this.dataGridTextBoxColumn1.Width = this.dgCodes.PreferredColumnWidth;
                dgCodes.DataSource = codeTable;

                if (DdlField != null)  cbxSort.Checked = !DdlField.ShouldSort;
                if (DdlColumn != null) cbxSort.Checked = !DdlColumn.ShouldSort;

                btnCreate.Enabled = false;
                btnFromExisting.Enabled = false; 
                btnUseExisting.Enabled = false;
                btnDelete.Enabled = true;
            }
            else
            {
                btnCreate.Enabled = true;
                btnFromExisting.Enabled = true;
                btnUseExisting.Enabled = true;
                btnDelete.Enabled = false;
            }

            btnOK.Visible = true;
            btnDelete.Visible = true;
        }

        /// <summary>
        /// Checks to see if a cell value is valid
        /// </summary>
        /// <param name="newText">New text</param>
        /// <returns>True/False;Depending upon the validation of text value</returns>
        private bool IsValidCellValue(string newText)
        {
            try
            {
                DataTable data = (DataTable)dgCodes.DataSource;
                DataRow[] rows = data.Select(data.Columns[0].ColumnName + " = '" + newText + "'");
                return (rows.Length == 1);
            }
            catch (Exception)
            {
                return (false);
            }
        }

        /// <summary>
        /// Display the fields
        /// </summary>
        /// <param name="tableName">Table name</param>
        protected virtual void ShowFieldSelection(string tableName)
        {
            string separator = " - ";
            if (!tableName.Contains(separator))
            {
                FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName);
                DialogResult result = fieldSelection.ShowDialog();
                if (result == DialogResult.OK)
                {
                    textColumnName = fieldSelection.ColumnName;
                    sourceTableName = tableName;
                    codeTable = page.GetProject().GetTableData(tableName, textColumnName, string.Empty);
                    fieldSelection.Close();
                    DisplayData();
                    isExclusiveTable = true;
                }
            }
            else if (DdlField != null)// using Form table as Datasource
            {
                string[] view_page = sourceTableName.Replace(separator, "^").Split('^');
                string viewName = view_page[0].ToString();
                string pageName = view_page[1].ToString();
                string filterExpression = string.Empty;
                string tableName1 = null;
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
                        tableName1 = viewName + pageId;
                    }
                }
                if (page.GetProject().CollectedData.TableExists(tableName1))
                {
                    FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName1);
                    DialogResult result = fieldSelection.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        textColumnName = fieldSelection.ColumnName;
                        codeTable = page.GetProject().GetTableData(tableName1, textColumnName, string.Empty);
                        fieldSelection.Close();
                        DisplayData();
                        isExclusiveTable = true;
                    }
                }

            }
            else
            {
                FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName);
                DialogResult result = fieldSelection.ShowDialog();
                if (result == DialogResult.OK)
                {
                    textColumnName = fieldSelection.ColumnName;
                    sourceTableName = tableName;
                    codeTable = page.GetProject().GetTableData(tableName, textColumnName, string.Empty);
                    fieldSelection.Close();
                    DisplayData();
                    isExclusiveTable = true;
                }
            }
        }

        /// <summary>
        /// Link the code table up to the field and save
        /// </summary>
        protected void SaveCodeTableToField()
        {
            DataTable dataTable = (DataTable)dgCodes.DataSource;

            if (dataTable != null)
            {
                if (!string.IsNullOrEmpty(dataTable.TableName))
                {
                    Project project = page.GetProject();

                    if (project != null)
                    {
                        if ((DataTable)dgCodes.DataSource != null)
                        {
                            if (Mode == CreationMode.CreateNewFromExisting || Mode == CreationMode.CreateNew || Mode == CreationMode.Edit)
                            {
                                string[] finalTableColumnNames = new string[dataTable.Columns.Count];

                                if (Mode != CreationMode.Edit)
                                {
                                    dataTable.TableName = GetNewCodeTableName(fieldName);
                                }

                                int i = 0;
                                foreach (DataColumn finalTableColumn in dataTable.Columns)
                                {
                                    finalTableColumnNames[i++] = finalTableColumn.ColumnName;
                                }

                                if(project.Metadata.TableExists(dataTable.TableName))
                                {
                                    project.Metadata.DeleteCodeTable("["+dataTable.TableName+"]");
                                }

                                dataTable.AcceptChanges();

                                project.CreateCodeTable(dataTable.TableName, finalTableColumnNames);
                                project.InsertCodeTableData(dataTable, dataTable.TableName, finalTableColumnNames);
                            }
                        }
                    }
                }

                this.sourceTableName = dataTable.TableName;
            }
        }

        #endregion Private Methods
    }

    public enum CreationMode
    {
        CreateNew, CreateNewFromExisting, UseExisting, Edit
    }
}

