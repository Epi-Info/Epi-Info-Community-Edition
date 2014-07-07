using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.ImportExport.Dialogs
{
    /// <summary>
    /// Table to View Dialog
    /// </summary>
    public partial class TableToViewDialog : Form
    {
        private string TableName = null;
        private string ViewName = null;
        private Epi.Data.IDbDriver DbDriver = null;
        //private Dictionary<string, string> columnMapping;
        private List<ColumnConversionInfo> columnMapping;
        private DataTable dataDictionary;
        private List<DataTable> codeTables;
        private List<string> codeTableList;

        public List<ColumnConversionInfo> ColumnMappings
        {
            get
            {
                return this.columnMapping;
            }
        }

        /// <summary>
        /// TableToViewDialog
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="pViewName">view name</param>
        /// <param name="pDbDriver">IDBDriver</param>
        /// <param name="pColumnMapping">Column mapping scheme</param>
        public TableToViewDialog(string pTableName, string pViewName, Epi.Data.IDbDriver pDbDriver, /*Dictionary<string, string> pColumnMapping*/List<ColumnConversionInfo> pColumnMapping)
        {
            InitializeComponent();

            TableName = pTableName;
            ViewName = pViewName;
            DbDriver = pDbDriver;
            columnMapping = pColumnMapping;
            ConvertMappingsToFieldTable();
            codeTables = new List<DataTable>();
            codeTableList = new List<string>();

            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Text");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Multiline");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Number");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("YesNo");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Checkbox");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Date");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("DateTime");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("Time");
            ((DataGridViewComboBoxColumn)dgvFormFields.Columns["FieldType"]).Items.Add("LegalValues");
        }

        private void ConvertMappingsToFieldTable()
        {
            dataDictionary = new DataTable("Data Dictionary");
            dataDictionary.Columns.Add(new DataColumn("Import", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("SourceColumnName", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("DestinationColumnName", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("Prompt", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("SourceColumnType", typeof(DbType)));
            dataDictionary.Columns.Add(new DataColumn("FieldType", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("PageNumber", typeof(int)));
            dataDictionary.Columns.Add(new DataColumn("TabIndex", typeof(int)));
            dataDictionary.Columns.Add(new DataColumn("IsTabStop", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("IsReadOnly", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("IsRequired", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("IsRepeatLast", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("HasRange", typeof(bool)));
            dataDictionary.Columns.Add(new DataColumn("LowerBound", typeof(object)));
            dataDictionary.Columns.Add(new DataColumn("UpperBound", typeof(object)));
            dataDictionary.Columns.Add(new DataColumn("ListSourceTableName", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("ListSourceTextColumnName", typeof(string)));
            dataDictionary.Columns.Add(new DataColumn("ListSourceTable", typeof(DataTable)));
            dataDictionary.Columns.Add(new DataColumn("ControlFont", typeof(Font)));
            dataDictionary.Columns.Add(new DataColumn("PromptFont", typeof(Font)));
            dataDictionary.Columns.Add(new DataColumn("ControlLeftPosition", typeof(double)));
            dataDictionary.Columns.Add(new DataColumn("ControlTopPosition", typeof(double)));
            dataDictionary.Columns.Add(new DataColumn("PromptLeftPosition", typeof(double)));
            dataDictionary.Columns.Add(new DataColumn("PromptTopPosition", typeof(double)));

            foreach (ColumnConversionInfo cci in columnMapping)
            {
                string message = string.Empty;
                if (Util.IsValidFieldName(cci.DestinationColumnName, ref message))
                {
                    dataDictionary.Rows.Add(true, cci.SourceColumnName, cci.DestinationColumnName, cci.Prompt, cci.SourceColumnType,
                        cci.FieldType, cci.PageNumber, cci.TabIndex, cci.IsTabStop, cci.IsReadOnly, cci.IsRequired, cci.IsRepeatLast,
                        cci.HasRange, cci.LowerBound, cci.UpperBound, cci.ListSourceTableName, cci.ListSourceTextColumnName,
                        cci.ListSourceTable, cci.ControlFont, cci.PromptFont, cci.ControlLeftPosition, cci.ControlTopPosition,
                        cci.PromptLeftPosition, cci.PromptTopPosition);
                }
            }
        }

        private void ConvertFieldTableToMappings()
        {
            columnMapping = new List<ColumnConversionInfo>();

            foreach (DataRow row in dataDictionary.Rows)
            {
                if ((bool)row[0] == true)
                {
                    ColumnConversionInfo cci = new ColumnConversionInfo();
                    cci.SourceColumnName = row["SourceColumnName"].ToString();
                    cci.DestinationColumnName = row["DestinationColumnName"].ToString();
                    cci.Prompt = row["Prompt"].ToString();
                    cci.SourceColumnType = (DbType)row["SourceColumnType"];
                    switch (row["FieldType"].ToString())
                    {
                        case "Text":
                            cci.FieldType = MetaFieldType.Text;
                            break;
                        case "Multiline":
                            cci.FieldType = MetaFieldType.Multiline;
                            break;
                        case "Number":
                            cci.FieldType = MetaFieldType.Number;
                            break;
                        case "Checkbox":
                            cci.FieldType = MetaFieldType.Checkbox;
                            break;
                        case "YesNo":
                            cci.FieldType = MetaFieldType.YesNo;
                            break;
                        case "LegalValues":
                            cci.FieldType = MetaFieldType.LegalValues;
                            break;
                        case "Date":
                            cci.FieldType = MetaFieldType.Date;
                            break;
                        case "DateTime":
                            cci.FieldType = MetaFieldType.DateTime;
                            break;
                        case "Time":
                            cci.FieldType = MetaFieldType.Time;
                            break;
                        default:
                            throw new ApplicationException("Invalid field type");
                    }                    
                    cci.PageNumber = (int)row["PageNumber"];
                    cci.TabIndex = (int)row["TabIndex"];
                    cci.IsTabStop = (bool)row["IsTabStop"];
                    cci.IsReadOnly = (bool)row["IsReadOnly"];
                    cci.IsRequired = (bool)row["IsRequired"];
                    cci.IsRepeatLast = (bool)row["IsRepeatLast"];
                    cci.HasRange = (bool)row["HasRange"];
                    cci.LowerBound = row["LowerBound"].ToString();
                    cci.UpperBound = row["UpperBound"].ToString();
                    cci.ListSourceTableName = row["ListSourceTableName"].ToString();
                    cci.ListSourceTextColumnName = row["ListSourceTextColumnName"].ToString();
                    if (row["ListSourceTable"] != null && row["ListSourceTable"] != DBNull.Value)
                    {
                        cci.ListSourceTable = (DataTable)row["ListSourceTable"];
                    }
                    if (row["ControlFont"] != null && row["ControlFont"] != DBNull.Value)
                    {
                        cci.ControlFont = (Font)row["ControlFont"];
                    }
                    if (row["PromptFont"] != null && row["PromptFont"] != DBNull.Value)
                    {
                        cci.PromptFont = (Font)row["PromptFont"];
                    }
                    cci.ControlLeftPosition = (double)row["ControlLeftPosition"];
                    cci.ControlTopPosition = (double)row["ControlTopPosition"];
                    cci.PromptLeftPosition = (double)row["PromptLeftPosition"];
                    cci.PromptTopPosition = (double)row["PromptTopPosition"];
                    columnMapping.Add(cci);
                }
            }
        }

        /// <summary>
        /// On load
        /// </summary>
        /// <param name="e">eventargs</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            textTableName.Text = TableName;
            textViewName.Text = ViewName;

            //((DataGridViewComboBoxColumn)dgvFormFields.Columns["ListSourceTableName"]).DataSource = codeTableList;
            //((DataGridViewComboBoxColumn)dgvFormFields.Columns["ListSourceTableName"]).AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            
            dgvFormFields.DataSource = dataDictionary;            
            dgvFormFields.AutoResizeColumns();

            codeTableList.Add(string.Empty);

            foreach (DataGridViewRow dgvRow in dgvFormFields.Rows)
            {
                DataGridViewComboBoxCell dgvc = dgvRow.Cells["FieldType"] as DataGridViewComboBoxCell;                
                DataGridViewTextBoxCell dgvt = dgvRow.Cells["SourceColumnType"] as DataGridViewTextBoxCell;

                dgvc.Items.Clear();
                
                switch(dgvt.FormattedValue.ToString().ToLower()) 
                {
                    case "string":                        
                        dgvc.Items.Add("Text");
                        dgvc.Items.Add("Multiline");
                        dgvc.Items.Add("Number");
                        dgvc.Items.Add("YesNo");
                        dgvc.Items.Add("Checkbox");                        
                        dgvc.Items.Add("LegalValues");
                        break;
                    case "int16":
                    case "int32":
                    case "int64":
                    case "decimal":
                    case "single":
                    case "double":
                        dgvc.Items.Add("Text");
                        dgvc.Items.Add("Multiline");
                        dgvc.Items.Add("Number");
                        break;
                    case "boolean":
                        dgvc.Items.Add("Text");
                        dgvc.Items.Add("YesNo");
                        dgvc.Items.Add("Checkbox");
                        break;
                    case "byte":
                        dgvc.Items.Add("Text");
                        dgvc.Items.Add("Multiline");
                        dgvc.Items.Add("Number");
                        dgvc.Items.Add("YesNo");
                        dgvc.Items.Add("Checkbox");
                        break;
                    case "datetime":
                        dgvc.Items.Add("Text");
                        dgvc.Items.Add("Multiline");                        
                        dgvc.Items.Add("Date");
                        dgvc.Items.Add("DateTime");
                        break;
                    default:
                        dgvc.Items.Add("Text");
                        break;
                }
            }
        }

        private Epi.ImportExport.ColumnConversionInfo GetColumnConversionInfo(string sourceColumnName)
        {
            foreach (Epi.ImportExport.ColumnConversionInfo cci in columnMapping)
            {
                if (cci.SourceColumnName == sourceColumnName)
                {
                    return cci;
                }
            }

            throw new KeyNotFoundException("sourceColumnName");
        }

        private bool Validate(ref string errorMessage)
        {
            foreach (DataRow row in this.dataDictionary.Rows)
            {
                string fieldNameValidationMessage = string.Empty;
                if (string.IsNullOrEmpty(row["DestinationColumnName"].ToString()))
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_FIELD_NAME_BLANK, row["SourceColumnName"].ToString());
                    return false;
                }
                else if (!(Util.IsValidFieldName(row["DestinationColumnName"].ToString(), ref fieldNameValidationMessage)))
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_FIELD_NAME_INVALID, row["SourceColumnName"].ToString(), fieldNameValidationMessage);
                    return false;
                }

                if (string.IsNullOrEmpty(row["Prompt"].ToString()))
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_PROMPT_BLANK, row["SourceColumnName"].ToString());
                    return false;
                }
                if (row["Prompt"].ToString().Length > 255)
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_PROMPT_255, row["SourceColumnName"].ToString());
                    return false;
                }

                if(row["PageNumber"] == null || row["PageNumber"] == DBNull.Value) 
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_PAGE_BLANK, row["SourceColumnName"].ToString());
                    return false;
                }

                int pageNumber = (int)row["PageNumber"];

                if (pageNumber <= 0) 
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_PAGE_LESS_THAN_ZERO, row["SourceColumnName"].ToString());
                    return false;
                }

                if (row["TabIndex"] == null || row["TabIndex"] == DBNull.Value)
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_TAB_BLANK, row["SourceColumnName"].ToString());
                    return false;
                }

                int tabIndex = (int)row["TabIndex"];

                if (tabIndex < 0)
                {
                    errorMessage = string.Format(ImportExportSharedStrings.ERROR_TABLE_TO_FORM_TAB_LESS_THAN_ZERO, row["SourceColumnName"].ToString());
                    return false;
                }
            }
            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //foreach (ListViewItem listViewItem in listViewFields.Items)
            //{
            //    SelectedColumns.Add(listViewItem.Text);
            //}

            if (Validate())
            {
                ConvertFieldTableToMappings();
                this.Close();
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {            
            this.Close();
        }        

        private void btnImportListValues_Click(object sender, EventArgs e)
        {            
            Epi.Windows.Dialogs.BaseReadDialog dlg = new Epi.Windows.Dialogs.BaseReadDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlg.SelectedDataSource is Project)
                {
                    Epi.Windows.MsgBox.ShowInformation(ImportExportSharedStrings.ERROR_CODE_TABLE_PROJECT_IMPORT_NOT_ALLOWED);
                }
                else
                {
                    Epi.Data.IDbDriver db = (Epi.Data.IDbDriver)dlg.SelectedDataSource;
                    if (string.IsNullOrEmpty(dlg.SQLQuery))
                    {
                        DataTable DT = db.GetTopTwoTable(dlg.SelectedDataMember);
                        List<string> columnNames = new List<string>();
                        foreach(DataColumn dc in DT.Columns) 
                        {
                            columnNames.Add(dc.ColumnName);
                        }
                        Epi.Windows.ImportExport.Dialogs.FieldSelectionDialog fsd = new Windows.ImportExport.Dialogs.FieldSelectionDialog(columnNames);
                        DialogResult fieldDialogResult = fsd.ShowDialog();
                        if (fieldDialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            string selectedColumn = fsd.SelectedField;
                            string codeTableName = "code" + dlg.SelectedDataMember + selectedColumn;

                            try
                            {
                                DT = db.GetTableData(dlg.SelectedDataMember);
                                DT.TableName = codeTableName;
                                codeTables.Add(DT);

                                codeTableList.Add(codeTableName);
                                ((DataGridViewComboBoxColumn)dgvFormFields.Columns["ListSourceTableName"]).Items.Clear();

                                foreach (string s in codeTableList)
                                {
                                    ((DataGridViewComboBoxColumn)dgvFormFields.Columns["ListSourceTableName"]).Items.Add(s);
                                }
                            }
                            catch (OutOfMemoryException)
                            {
                                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_OUT_OF_MEMORY_CODE_TABLE);
                                DT.Rows.Clear();
                            }
                        }
                    }
                    else
                    {
                        Epi.Windows.MsgBox.ShowInformation(ImportExportSharedStrings.ERROR_CODE_TABLE_QUERY_IMPORT_NOT_ALLOWED);
                    }
                }
            }
        }

        private void listViewFields_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dgvFormFields_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {            
            if (e.ColumnIndex == 15)
            {
                string codeTableName = dgvFormFields.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                dgvFormFields.Rows[e.RowIndex].Cells["ListSourceTable"].Value = null;
                dgvFormFields.Rows[e.RowIndex].Cells["ListSourceTextColumnName"].Value = string.Empty;

                if(!string.IsNullOrEmpty(codeTableName))
                {
                    foreach (DataTable table in codeTables)
                    {
                        if (table.TableName == codeTableName)
                        {
                            dgvFormFields.Rows[e.RowIndex].Cells["ListSourceTable"].Value = table;
                            dgvFormFields.Rows[e.RowIndex].Cells["ListSourceTextColumnName"].Value = table.Columns[0].ColumnName;
                            break;
                        }
                    }
                }
            }
        }

        private void btnSetPromptFont_Click(object sender, EventArgs e)
        {
            FontDialog fnt = new FontDialog();            
            fnt.ShowEffects = false;
            DialogResult result = fnt.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (DataRow row in dataDictionary.Rows)
                {
                    row["PromptFont"] = fnt.Font;
                }
            }
        }

        private void btnSetControlFont_Click(object sender, EventArgs e)
        {
            FontDialog fnt = new FontDialog();
            fnt.ShowEffects = false;
            DialogResult result = fnt.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (DataRow row in dataDictionary.Rows)
                {
                    row["ControlFont"] = fnt.Font;
                }
            }
        }
    }
}
