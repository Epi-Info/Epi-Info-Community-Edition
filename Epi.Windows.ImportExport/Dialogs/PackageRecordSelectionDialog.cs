using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport;
using Epi.Windows;

namespace Epi.Windows.ImportExport.Dialogs
{
    /// <summary>
    /// Dialog for filtering records in a project package
    /// </summary>
    public partial class PackageRecordSelectionDialog : Form
    {
        #region Private Members
        private Configuration config;
        private Query selectQuery;
        private Project sourceProject;
        private string formName;
        private MetaFieldType selectedFieldType;
        private Field selectedField;
        private int runningParamIndex = 0;
        private List<IRowFilterCondition> rowFilterConditions;
        private IDbDriver db;
        private string fromClause;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public PackageRecordSelectionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceProject">The source project</param>
        /// <param name="formName">The name of the top-level form</param>
        public PackageRecordSelectionDialog(Project sourceProject, string formName)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;
            rowFilterConditions = new List<IRowFilterCondition>();
            FillFields();
            config = Configuration.GetNewInstance();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceProject">The source project</param>
        /// <param name="formName">The name of the top-level form</param>
        /// <param name="rowFilterConditions">The list of row filter conditions to apply</param>
        public PackageRecordSelectionDialog(Project sourceProject, string formName, List<IRowFilterCondition> rowFilterConditions)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;
            
            if (rowFilterConditions != null)
            {
                this.rowFilterConditions = rowFilterConditions;

                foreach (IRowFilterCondition rowFc in this.rowFilterConditions)
                {
                    lbxRecordFilters.Items.Add(rowFc.Description);
                }
            }
            else
            {
                this.rowFilterConditions = new List<IRowFilterCondition>();
            }

            FillFields();
            config = Configuration.GetNewInstance();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the query
        /// </summary>
        public Query SelectQuery
        {
            get
            {
                return this.selectQuery;
            }
        }

        /// <summary>
        /// Gets the list of row filter conditions
        /// </summary>
        internal List<IRowFilterCondition> RowFilterConditions
        {
            get
            {
                return this.rowFilterConditions;
            }
        }
        #endregion // Public Properties

        #region Private Methods
        private void FillFields()
        { 
            Project selectedProject = sourceProject;
            View selectedView = selectedProject.Views[formName];

            List<string> fieldList = new List<string>();

            foreach (Field field in selectedView.Fields)
            {
                if (field is IDataField &&
                    !(
                    field is RecStatusField ||
                    field is UniqueKeyField ||
                    field is GlobalRecordIdField ||
                    field is ForeignKeyField ||
                    field is CommandButtonField ||
                    field is GroupField ||
                    field is ImageField ||
                    field is OptionField ||
                    field is TimeField ||
                    field is GridField
                    ))
                {
                    fieldList.Add(field.Name);                    
                }
            }

            IDbDriver driver = sourceProject.CollectedData.GetDatabase();
            DataTable dt = driver.GetTableData(formName, "*");
            foreach (DataColumn datcol in dt.Columns)
            {
                if (datcol.ColumnName.Equals("FirstSaveLogonName", StringComparison.OrdinalIgnoreCase) ||
                    datcol.ColumnName.Equals("LastSaveLogonName", StringComparison.OrdinalIgnoreCase) ||
                    datcol.ColumnName.Equals("FirstSaveTime", StringComparison.OrdinalIgnoreCase) ||
                    datcol.ColumnName.Equals("LastSaveTime", StringComparison.OrdinalIgnoreCase))
                {
                    fieldList.Add(datcol.ColumnName);
                    //View v = new View((DataRow)datcol, sourceProject);
                }
            }

            fieldList.Sort();

            foreach (string s in fieldList)
            {
                cmbFieldName.Items.Add(s);
            }
        }

        private void EnableDisableAddButton()
        {
            btnAdd.Enabled = false;

            if (cmbFieldName.SelectedIndex >= 0 && cmbFieldOperator.SelectedIndex >= 0)
            {
                btnAdd.Enabled = true;
            }
        }

        private void FillValueCombobox()
        {
            cmbFieldValue.Items.Clear();

            Field field = selectedField;

            if (field is TableBasedDropDownField)
            {
                DataTable dataTable = ((TableBasedDropDownField)field).GetSourceData();
                Dictionary<string, string> fieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (field is DDLFieldOfLegalValues)
                {
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        string codeColumnName = ((TableBasedDropDownField)field).CodeColumnName.Trim();
                        if (!string.IsNullOrEmpty(codeColumnName))
                        {
                            string Key = row[codeColumnName].ToString();
                            if (!fieldValues.ContainsKey(Key))
                            {
                                fieldValues.Add(Key, Key);
                            }
                        }
                    }
                }
                else if (field is DDLFieldOfCodes)
                {
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        string codeColumnName = ((DDLFieldOfCodes)field).TextColumnName.Trim();
                        if (!string.IsNullOrEmpty(codeColumnName))
                        {
                            string Key = row[codeColumnName].ToString();
                            if (!fieldValues.ContainsKey(Key))
                            {
                                fieldValues.Add(Key, Key);
                            }
                        }
                    }
                }
                else
                {
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        string codeColumnName = ((TableBasedDropDownField)field).TextColumnName.Trim();
                        if (!string.IsNullOrEmpty(codeColumnName))
                        {
                            string Key = row[codeColumnName].ToString();
                            int dash = Key.IndexOf('-');
                            Key = Key.Substring(0, dash);
                            if (!fieldValues.ContainsKey(Key))
                            {
                                fieldValues.Add(Key, Key);
                            }
                        }
                    }
                }

                cmbFieldValue.Items.Clear();

                int i = 0;
                foreach (KeyValuePair<string, string> kvp in fieldValues)
                {
                    if (kvp.Key.Length > 0)
                    {
                        cmbFieldValue.Items.Add(kvp.Key);
                    }

                    i++;
                }
            }
            else if (field is YesNoField || field is CheckBoxField)
            {
                cmbFieldValue.Items.Add(config.Settings.RepresentationOfYes);
                cmbFieldValue.Items.Add(config.Settings.RepresentationOfNo);
            }
        }

        private void HideUnhideValueControls()
        {
            txtFieldValue.Visible = true;

            txtFieldValue.Text = string.Empty;
            cmbFieldValue.Text = string.Empty;            
            cmbFieldValue.Items.Clear();
            cmbFieldValue.SelectedIndex = -1;            

            switch (selectedFieldType)
            {
                case MetaFieldType.Text:
                case MetaFieldType.TextUppercase:
                case MetaFieldType.PhoneNumber:
                case MetaFieldType.Multiline:
                case MetaFieldType.Option:
                case MetaFieldType.Number:
                case MetaFieldType.GUID:
                    txtFieldValue.Visible = true;
                    cmbFieldValue.Visible = false;
                    dtpFieldValue.Visible = false;
                    break;
                case MetaFieldType.Date:
                case MetaFieldType.DateTime:
                case MetaFieldType.Time:
                    // special cases
                    if (cmbFieldOperator.SelectedItem.ToString().Equals("is less than N days ago"))
                    {
                        txtFieldValue.Visible = true;
                        cmbFieldValue.Visible = false;
                        dtpFieldValue.Visible = false;
                    }
                    else if (cmbFieldOperator.SelectedItem.ToString().Equals("is today's date"))
                    {
                        txtFieldValue.Visible = false;
                        cmbFieldValue.Visible = false;
                        dtpFieldValue.Visible = false;
                    }
                    else
                    {
                        txtFieldValue.Visible = false;
                        cmbFieldValue.Visible = false;
                        dtpFieldValue.Visible = true;
                    }
                    break;
                case MetaFieldType.LegalValues:
                case MetaFieldType.CommentLegal:
                case MetaFieldType.Codes:
                case MetaFieldType.YesNo:
                case MetaFieldType.Checkbox:
                    txtFieldValue.Visible = false;
                    cmbFieldValue.Visible = true;
                    dtpFieldValue.Visible = false;
                    break;
                default:
                    throw new ApplicationException("Not a valid field type");
            }
        }
        #endregion // Private Methods

        #region Event Handlers

        private void cmbField1Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableAddButton();
            
            cmbFieldOperator.Items.Clear();
            cmbFieldValue.Items.Clear();

            cmbFieldOperator.Text = string.Empty;
            cmbFieldOperator.SelectedIndex = -1;

            if (cmbFieldName.SelectedIndex >= 0)
            {
                View view = sourceProject.Views[formName];
                string fieldName = cmbFieldName.SelectedItem.ToString();
                Field field = null;
                if (view.Fields.Names.Contains(fieldName))
                {
                    field = view.Fields[fieldName];

                    txtFieldValue.Text = string.Empty;
                    cmbFieldValue.Text = string.Empty;
                    cmbFieldValue.Items.Clear();
                    cmbFieldValue.SelectedIndex = -1;

                    switch (field.FieldType)
                    {
                        case MetaFieldType.Checkbox:
                            cmbFieldOperator.Items.Add("is equal to");
                            break;
                        case MetaFieldType.Date:
                        case MetaFieldType.DateTime:
                        case MetaFieldType.Time:
                            //cmbField1Operator.Items.Add("is not missing");
                            //cmbField1Operator.Items.Add("is missing");
                            cmbFieldOperator.Items.Add("is greater than");
                            cmbFieldOperator.Items.Add("is greater than or equal to");
                            cmbFieldOperator.Items.Add("is less than");
                            cmbFieldOperator.Items.Add("is less than or equal to");
                            cmbFieldOperator.Items.Add("is less than N days ago");
                            //cmbFieldOperator.Items.Add("is today's date");
                            break;
                        case MetaFieldType.Number:
                            //cmbField1Operator.Items.Add("is not missing");
                            //cmbField1Operator.Items.Add("is missing");
                            //cmbField1Operator.Items.Add("is not equal to");
                            cmbFieldOperator.Items.Add("is equal to");
                            cmbFieldOperator.Items.Add("is greater than");
                            cmbFieldOperator.Items.Add("is greater than or equal to");
                            cmbFieldOperator.Items.Add("is less than");
                            cmbFieldOperator.Items.Add("is less than or equal to");
                            break;
                        case MetaFieldType.Text:
                        case MetaFieldType.TextUppercase:
                        case MetaFieldType.Multiline:
                        case MetaFieldType.LegalValues:
                        case MetaFieldType.CommentLegal:
                        case MetaFieldType.Codes:
                        case MetaFieldType.GUID:
                        case MetaFieldType.PhoneNumber:
                            //cmbField1Operator.Items.Add("is not missing");
                            //cmbField1Operator.Items.Add("is missing");
                            //cmbField1Operator.Items.Add("is not equal to");
                            cmbFieldOperator.Items.Add("is equal to");
                            break;
                        case MetaFieldType.YesNo:
                            cmbFieldOperator.Items.Add("is equal to");
                            //cmbField1Operator.Items.Add("is not missing");
                            //cmbField1Operator.Items.Add("is missing");
                            break;
                    }

                    selectedFieldType = field.FieldType;
                    selectedField = field;
                }
                else if (fieldName.Equals("FirstSaveLogonName") || fieldName.Equals("LastSaveLogonName"))
                {
                    cmbFieldOperator.Items.Add("is equal to");
                    selectedFieldType = MetaFieldType.Text;
                }
                else if (fieldName.Equals("FirstSaveTime") || fieldName.Equals("LastSaveTime"))
                {
                    cmbFieldOperator.Items.Add("is greater than");
                    cmbFieldOperator.Items.Add("is greater than or equal to");
                    cmbFieldOperator.Items.Add("is less than");
                    cmbFieldOperator.Items.Add("is less than or equal to");
                    cmbFieldOperator.Items.Add("is less than N days ago");
                    selectedFieldType = MetaFieldType.DateTime;
                }
            }
        }

        private void cmbField1Operator_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFieldValue.Text = string.Empty;

            EnableDisableAddButton();

            if (cmbFieldOperator.SelectedIndex >= 0)
            {
                HideUnhideValueControls();
                FillValueCombobox();

                switch (cmbFieldOperator.SelectedItem.ToString())
                {
                    case "is missing":
                    case "is not missing":
                        txtFieldValue.Enabled = false;
                        break;
                    default:
                        txtFieldValue.Enabled = true;
                        break;
                }
            }
        }
        #endregion // Event Handlers        

        private bool ValidateInput()
        {
            if (cmbFieldOperator.SelectedIndex == -1)
            {
                Epi.Windows.MsgBox.ShowInformation("Specify an operator.");
                cmbFieldOperator.Focus();
                return false;
            }

            switch (selectedFieldType)
            {
                case MetaFieldType.Text:
                case MetaFieldType.TextUppercase:
                case MetaFieldType.PhoneNumber:
                case MetaFieldType.Multiline:
                case MetaFieldType.Option:
                case MetaFieldType.Number:
                case MetaFieldType.GUID:
                    // Textbox
                    if (string.IsNullOrEmpty(txtFieldValue.Text))
                    {
                        Epi.Windows.MsgBox.ShowInformation("Specify a value.");
                        cmbFieldOperator.Focus();
                        return false;
                    }
                    break;
                case MetaFieldType.Date:
                case MetaFieldType.DateTime:
                case MetaFieldType.Time:
                    // special cases
                    if (cmbFieldOperator.SelectedItem.ToString().Equals("is less than N days ago"))
                    {
                        if (string.IsNullOrEmpty(txtFieldValue.Text))
                        {
                            Epi.Windows.MsgBox.ShowInformation("Specify a value.");
                            cmbFieldOperator.Focus();
                            return false;
                        }
                    }
                    else if (cmbFieldOperator.SelectedItem.ToString().Equals("is today's date"))
                    {
                        // NONE
                    }
                    else
                    {
                        // DT picker
                        //if (string.IsNullOrEmpty(txtFieldValue.Text))
                        //{
                        //    Epi.Windows.MsgBox.ShowInformation("Specify a value.");
                        //    cmbFieldOperator.Focus();
                        //    return false;
                        //}
                    }
                    break;
                case MetaFieldType.LegalValues:
                case MetaFieldType.CommentLegal:
                case MetaFieldType.Codes:
                case MetaFieldType.YesNo:
                case MetaFieldType.Checkbox:
                    // cmbFieldValue                    
                    if (cmbFieldValue.SelectedIndex == -1 || string.IsNullOrEmpty(cmbFieldValue.Text))
                    {
                        Epi.Windows.MsgBox.ShowInformation("Specify a value.");
                        cmbFieldOperator.Focus();
                        return false;
                    }
                    break;
                default:
                    throw new ApplicationException("Not a valid field type");
            }

            return true;
        }

        private void AddNewCondition()
        {
            if (ValidateInput())
            {
                string sql = string.Empty;
                string condition = string.Empty;

                if (txtFieldValue.Enabled)
                {
                    string columnName = cmbFieldName.SelectedItem.ToString();
                    string paramName = "@" + columnName + runningParamIndex.ToString();
                    sql = columnName;
                    sql = sql + TranslateOperator(cmbFieldOperator.SelectedItem.ToString());
                    sql = sql + paramName;

                    IRowFilterCondition rowFc = GetRowFilterCondition(sql, paramName, columnName);

                    // e.g. LastName = @LastName0
                    // The runningParamIndex is prefixed into the param name in case there are multiple params
                    // using the same field name. For example, LastName = 'Smith' AND LastName = 'Smythe'

                    string friendlyOp = cmbFieldOperator.SelectedItem.ToString();

                    condition = cmbFieldName.SelectedItem.ToString();
                    if (friendlyOp.ToLowerInvariant().Contains("is less than n days"))
                    {
                        condition = condition + StringLiterals.SPACE + friendlyOp.Replace(" N ", " " + txtFieldValue.Text + " ");
                    }
                    else
                    {
                        condition = condition + StringLiterals.SPACE + friendlyOp + StringLiterals.SPACE;
                        condition = condition + rowFc.Value.ToString();
                    }

                    runningParamIndex++;

                    if (!lbxRecordFilters.Items.Contains(condition) && !rowFilterConditions.Contains(rowFc))
                    {
                        rowFc.Description = condition;
                        lbxRecordFilters.Items.Add(condition);
                        rowFilterConditions.Add(rowFc);
                    }
                    else
                    {
                        Epi.Windows.MsgBox.ShowInformation(ImportExportSharedStrings.CONDITION_ALREADY_ADDED);
                    }
                }
                else
                {
                    // do nothing at the moment; not implemented
                }
            }
        }

        private void RemoveSelectedConditions()
        {
            List<string> conditionStringsToRemove = new List<string>();
            List<IRowFilterCondition> conditionsToRemove = new List<IRowFilterCondition>();

            foreach (string condition in this.lbxRecordFilters.SelectedItems)
            {
                conditionStringsToRemove.Add(condition);
            }

            foreach (string condition in conditionStringsToRemove)
            {
                lbxRecordFilters.Items.Remove(condition);
            }

            foreach (IRowFilterCondition rowFc in this.rowFilterConditions)
            {
                if(conditionStringsToRemove.Contains(rowFc.Description)) 
                {
                    conditionsToRemove.Add(rowFc);
                }
            }

            foreach (IRowFilterCondition rowFc in conditionsToRemove)
            {
                rowFilterConditions.Remove(rowFc);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                AddNewCondition();
            }
            catch (Exception)
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_PACKAGER_SELECT_CONDITION_COULD_NOT_BE_ADDED);
            }
        }

        private IRowFilterCondition GetRowFilterCondition(string conditionSql, string conditionParamName, string conditionColumnName)
        {
            IRowFilterCondition rowFc = null;
            object value = GetValue();

            switch (selectedFieldType)
            {
                case MetaFieldType.Text:
                case MetaFieldType.TextUppercase:
                case MetaFieldType.PhoneNumber:
                case MetaFieldType.Multiline:                
                case MetaFieldType.LegalValues:
                case MetaFieldType.CommentLegal:
                case MetaFieldType.Codes:
                case MetaFieldType.GUID:
                    rowFc = new TextRowFilterCondition(conditionSql, conditionColumnName, conditionParamName, value);
                    break;
                case MetaFieldType.Date:
                case MetaFieldType.DateTime:
                case MetaFieldType.Time:
                    rowFc = new DateRowFilterCondition(conditionSql, conditionColumnName, conditionParamName, value);
                    break;
                case MetaFieldType.Option:
                case MetaFieldType.Number:
                    rowFc = new NumberRowFilterCondition(conditionSql, conditionColumnName, conditionParamName, value);
                    break;
                case MetaFieldType.YesNo:
                    rowFc = new YesNoRowFilterCondition(conditionSql, conditionColumnName, conditionParamName, value);
                    break;
                case MetaFieldType.Checkbox:
                    rowFc = new CheckboxRowFilterCondition(conditionSql, conditionColumnName, conditionParamName, value);
                    break;
                default:
                    throw new ApplicationException("Not a valid field type");
            }

            return rowFc;
        }

        private object GetValue()
        {
            object value = null;

            if (txtFieldValue.Visible)
            {
                if (cmbFieldOperator.SelectedItem.ToString().Equals("is less than N days ago"))
                {
                    DateTime today = DateTime.Now;
                    TimeSpan ts = new TimeSpan(int.Parse(txtFieldValue.Text), 0, 0, 0);
                    DateTime nDaysAgo = today - ts;
                    value = nDaysAgo;                    
                }
                else
                {
                    value = txtFieldValue.Text;
                }
            }
            else if (cmbFieldValue.Visible)
            {
                value = cmbFieldValue.Text;

                if (selectedField is YesNoField || selectedField is CheckBoxField)
                {
                    if (value.Equals(config.Settings.RepresentationOfYes))
                    {
                        value = true;
                    }
                    else if (value.Equals(config.Settings.RepresentationOfNo))
                    {
                        value = false;
                    }
                }
            }
            else if (dtpFieldValue.Visible)
            {
                value = dtpFieldValue.Value;
            }
            else
            {
                // do nothing, not implemented yet
            }

            return value;
        }

        private string TranslateOperator(string friendlyOperator)
        {
            string op = " = ";
            switch (friendlyOperator)
            {
                case "is not missing":
                    op = " is not null";
                    break;
                case "is missing":
                    op = " is null";
                    break;
                case "is not equal to":
                    op = " = ";
                    break;
                case "is equal to":
                    op = " = ";
                    break;
                case "is greater than":
                    op = " > ";
                    break;
                case "is greater than or equal to":
                    op = " >= ";
                    break;
                case "is less than":
                    op = " < ";
                    break;
                case "is less than or equal to":
                    op = " <= ";
                    break;
                case "is less than N days ago":
                    op = " >= ";
                    break;
                case "is today's date":
                    op = " >= ";
                    break;
            }

            return op;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (db != null)
            {
                View sourceView = sourceProject.Views[formName];

                string baseTableName = "t";

                string fromClause = sourceView.FromViewSQL;
                string sql = "SELECT [" + baseTableName + "].[GlobalRecordId] " + fromClause + " WHERE ";

                foreach (IRowFilterCondition rowFc in rowFilterConditions)
                {
                    sql = sql + rowFc.Sql;
                }

                selectQuery = db.CreateQuery(sql);

                foreach (IRowFilterCondition rowFc in rowFilterConditions)
                {
                    selectQuery.Parameters.Add(rowFc.Parameter);
                }
            }
        }

        internal void AttachDatabase(IDbDriver database) 
        {
            db = database;
        }

        private void lbxRecordFilters_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && lbxRecordFilters.SelectedItems.Count > 0)
            {
                RemoveSelectedConditions();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lbxRecordFilters.Items.Clear();
            this.rowFilterConditions.Clear();
        }        
    }
}
