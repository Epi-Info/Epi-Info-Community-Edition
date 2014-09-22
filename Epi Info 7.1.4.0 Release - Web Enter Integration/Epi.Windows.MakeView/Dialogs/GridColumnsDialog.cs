#region Namespaces
using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Epi.Windows.Dialogs;
using Epi.Fields;
using Epi;
using Epi.Windows;
#endregion

namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// The Grid Columns dialog
    /// </summary>
    public partial class GridColumnsDialog : DialogBase
    {
        #region Private Members

        private const int iColumnOffset = 4;

        private int currentGridColumn = -1;
        private DataTable fieldTypes = null;
        private MainForm form;
        private Epi.Fields.GridField grid = null;
        private DataTable gridColumnsTable = null;
        private FormMode mode;
        private string originalColumnName = string.Empty;
        private Page page = null;
        private DataView patterns = null;
        private DataGridTableStyle tableStyle = null;
        private List<GridColumnBase> gridColumns = null;

        #region DDLColumns Class Members

        private bool isExclusiveTable = false;
        private bool shouldSort = false;
        private string sourceTableName = string.Empty;
        private string textColumnName = string.Empty;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public GridColumnsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Grid Columns Dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="fieldPage">The page containing the field</param>
        /// <param name="gridField">The grid's field</param>
        /// <param name="patternsDataView">The data view</param>
        /// <param name="formMode">The form mode</param>
        /// <param name="tempColumns">Temporary Columns</param>
        public GridColumnsDialog(MainForm frm, Page fieldPage, Epi.Fields.GridField gridField, DataView patternsDataView, FormMode formMode, List<GridColumnBase> tempColumns)
            : base(frm)
        {
            InitializeComponent();
            page = fieldPage;
            grid = gridField;
            patterns = patternsDataView;
            mode = formMode;
            form = frm;
            gridColumns = tempColumns;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Property for the grid data collection
        /// </summary>
        public GridField Grid
        {
            get
            {
                return grid;
            }
        }

        #endregion

        /// <summary>
        /// Property to access the dialog Grid Columns collection
        /// </summary>
        public List<GridColumnBase> GridColumns
        {
            get
            {
                return gridColumns;
            }
        }

        #region Private Methods

        /// <summary>
        /// Enables and disables fields
        /// </summary>
        private void EnableDisableFields()
        {
            if (cmbFieldType.SelectedIndex == -1)
            {
                return;
            }

            DataRow fieldType = fieldTypes.Rows[cmbFieldType.SelectedIndex];
            cbxRange.Enabled = (bool)(fieldType["HasRange"]);
            cbxReadOnly.Enabled = (bool)(fieldType["HasReadOnly"]);
            cbxRequired.Enabled = (bool)(fieldType["HasRequired"]);

            cmbSize.SelectedIndex = -1;

            btnDataSource.Enabled = false;
            btnSaveColumn.Enabled = true;

            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal) || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues))
            {
                bool hideDataSourceButton = txtColumnName.Text == "" || txtFieldName.Text == "";
                bool hideSaveColumnButton = txtDataSource.Text == "" || hideDataSourceButton;

                btnDataSource.Enabled = hideDataSourceButton ? false : true;
                btnSaveColumn.Enabled = hideSaveColumnButton ? false : true;

                cbxUniqueField.Enabled = true;
            }
            else
            {
                cbxUniqueField.Enabled = false;
            }

            bool isTypeTime = false;

            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Date) 
                || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Time)
                || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.DateTime)
                )
            {
                isTypeTime = true;
            }

            if (isTypeTime == false && ((bool)fieldType["HasPattern"]) || ((bool)fieldType["HasSize"]))
            {
                lblPattern.Visible = (bool)(fieldType["HasPattern"]);
                cmbPattern.Visible = (bool)(fieldType["HasPattern"]);

                if (patterns != null)
                {
                    patterns.RowFilter = ColumnNames.DATATYPEID + " = " + fieldType[ColumnNames.DATATYPEID].ToString();
                }
                
                if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
                {
                    cmbPattern.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
                }

                if (!cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Multiline))
                {
                    lblSize.Visible = (bool)(fieldType["HasSize"]);
                    cmbSize.Visible = (bool)(fieldType["HasSize"]);
                }
                else
                {
                    lblSize.Visible = false;
                    cmbSize.Visible = false;
                }
            }
            else
            {
                lblPattern.Visible = false;
                cmbPattern.Visible = false;
                lblSize.Visible = false;
                cmbSize.Visible = false;
            }
        }

        /// <summary>
        /// Loads the columns for the data grid based on the grid field that was passed to this form
        /// </summary>
        private void LoadGridData()
        {
            gridColumnsTable = new DataTable("GridColumns");
            tableStyle = new DataGridTableStyle();
            tableStyle.MappingName = gridColumnsTable.TableName;
            tableStyle.RowHeadersVisible = false;
            tableStyle.ReadOnly = true;

            gridColumns.Sort(Util.SortByPosition);
            if (gridColumns.Count != 0)
            {
                foreach (GridColumnBase gridColumn in gridColumns)
                {
                    if (!(gridColumn is PredefinedColumn))
                    {
                        DataColumn dc = new DataColumn(gridColumn.Name);
                        gridColumnsTable.Columns.Add(dc);

                        DataGridTextBoxColumn textBoxColumn = new DataGridTextBoxColumn();

                        textBoxColumn.MappingName = gridColumn.Name;
                        textBoxColumn.HeaderText = gridColumn.Text;
                        textBoxColumn.Width = ((gridColumn.Width > 0) ? gridColumn.Width : 75);
                        textBoxColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);

                        tableStyle.GridColumnStyles.Add(textBoxColumn);
                    }
                }
            }
            dgColumns.TableStyles.Clear();
            dgColumns.TableStyles.Add(tableStyle);
            dgColumns.DataSource = gridColumnsTable;
            dgColumns.RowHeadersVisible = false;
        }

        /// <summary>
        /// Sets the values for the combo boxes on this form
        /// </summary>
        private void LoadSettings()
        {
            PopulateGridColumnTypes();
            PopulatePatterns();
        }

        /// <summary>
        /// Populates grid column types
        /// </summary>
        private void PopulateGridColumnTypes()
        {
            fieldTypes = Grid.GetMetadata().GetGridFieldTypes();
            cmbFieldType.DataSource = fieldTypes;
            cmbFieldType.DisplayMember = ColumnNames.NAME;
            cmbFieldType.ValueMember = ColumnNames.FIELD_TYPE_ID;
        }

        /// <summary>
        /// Populates patterns for datatypes
        /// </summary>
        private void PopulatePatterns()
        {
            cmbPattern.DataSource = patterns;
            cmbPattern.DisplayMember = ColumnNames.EXPRESSION;
            cmbPattern.ValueMember = ColumnNames.PATTERN_ID;
        }

        private string CreateGridColumnFieldName(string promptText)
        {
            promptText = Util.Squeeze(promptText);

            if (promptText == "")
            {
                promptText = "field";
            }

            //Remove any special characters in the prompt name
            string fieldName = Util.RemoveNonAlphaNumericCharacters(promptText);

            //If first character is not a letter then prefix field name with "N"
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (!Util.IsFirstCharacterALetter(fieldName))
                {
                    fieldName = "N" + fieldName;
                }
            }

            // Check if this column already exists. If so, increment a suffix counter.
            string newFieldName = fieldName;
            int count = 0;
            Regex colRegex = new Regex("^" + fieldName + "{1}[1-9]*", RegexOptions.IgnoreCase);

            GridField grid = this.Grid;
            foreach (GridColumnBase cols in gridColumns)
            {
                Match colMatch = colRegex.Match(cols.Name);
                if (colMatch.Success)
                {
                    count++;
                }

            }
            newFieldName = fieldName + ((count > 0) ? count.ToString():string.Empty);
            return newFieldName;
        }

        /// <summary>
        /// Restores the default settings for the grid columns dialog
        /// </summary>
        private void RestoreDefaultProperties()
        {
            cmbFieldType.Text = "Text";
            cmbSize.Text = string.Empty;
            cbxRepeatLast.Checked = false;
            cbxRequired.Checked = false;
            cbxReadOnly.Checked = false;
            cbxUniqueField.Checked = false;
            cbxRange.Checked = false;
            cmbPattern.Text = string.Empty;
            txtUpper.Text = "99";
            txtLower.Text = "0";
            cbxRange.Checked = false;
            hti = null;
        }

        /// <summary>
        /// Save column data 
        /// </summary>
        private void SaveColumnData()
        {
            switch ((MetaFieldType)cmbFieldType.SelectedValue)
            {
                case MetaFieldType.Text:
                    TextColumn textColumn = new TextColumn(grid);
                    textColumn.Name = txtFieldName.Text;
                    textColumn.Text = txtColumnName.Text;
                    textColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    textColumn.IsRequired = cbxRequired.Checked;
                    textColumn.IsReadOnly = cbxReadOnly.Checked;
                    textColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;//gridColumnsTable.Columns.Count;
                    textColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    if (!string.IsNullOrEmpty(cmbSize.Text))
                    {
                        textColumn.Size = int.Parse(cmbSize.Text);
                    }
                    textColumn.Grid = grid;
                    gridColumns.Add(textColumn);
                    SetColumnStyle(textColumn);
                    break;
                case MetaFieldType.Number:
                    NumberColumn numberColumn = new NumberColumn(grid);
                    numberColumn.Name = txtFieldName.Text;
                    numberColumn.Text = txtColumnName.Text;
                    numberColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    numberColumn.IsRequired = cbxRequired.Checked;
                    numberColumn.IsReadOnly = cbxReadOnly.Checked;
                    numberColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    numberColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    numberColumn.Pattern = cmbPattern.Text;
                    if (cbxRange.Checked)
                    {
                        numberColumn.Lower = txtLower.Text;
                        numberColumn.Upper = txtUpper.Text;
                    }
                    gridColumns.Add(numberColumn);
                    SetColumnStyle(numberColumn);
                    break;
                case MetaFieldType.PhoneNumber:
                    PhoneNumberColumn phoneNumberColumn = new PhoneNumberColumn(grid);
                    phoneNumberColumn.Name = txtFieldName.Text;
                    phoneNumberColumn.Text = txtColumnName.Text;
                    phoneNumberColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    phoneNumberColumn.IsRequired = cbxRequired.Checked;
                    phoneNumberColumn.IsReadOnly = cbxReadOnly.Checked;
                    phoneNumberColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    phoneNumberColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    phoneNumberColumn.Pattern = cmbPattern.Text;
                    gridColumns.Add(phoneNumberColumn);
                    SetColumnStyle(phoneNumberColumn);
                    break;
                case MetaFieldType.Date:
                    SetContiguousColumnProperties(new DateColumn(grid));
                    break;
                case MetaFieldType.Time:
                    SetContiguousColumnProperties(new TimeColumn(grid));
                    break;
                case MetaFieldType.DateTime:
                    SetContiguousColumnProperties(new DateTimeColumn(grid));
                    break;
                case MetaFieldType.CommentLegal:
                    DDLColumnOfCommentLegal commentLegalColumn = new DDLColumnOfCommentLegal(grid);
                    commentLegalColumn.ShouldSort = shouldSort;
                    commentLegalColumn.SourceTableName = sourceTableName;
                    commentLegalColumn.TextColumnName = textColumnName;
                    commentLegalColumn.IsExclusiveTable = isExclusiveTable;
                    commentLegalColumn.Name = txtFieldName.Text;
                    commentLegalColumn.Text = txtColumnName.Text;
                    commentLegalColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    commentLegalColumn.IsRequired = cbxRequired.Checked;
                    commentLegalColumn.IsReadOnly = cbxReadOnly.Checked;
                    commentLegalColumn.IsUniqueField = cbxUniqueField.Checked;
                    commentLegalColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    commentLegalColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    commentLegalColumn.Grid = grid;
                    gridColumns.Add(commentLegalColumn);
                    SetColumnStyle(commentLegalColumn);
                    break;
                case MetaFieldType.LegalValues:
                    DDLColumnOfLegalValues legalValuesColumn = new DDLColumnOfLegalValues(grid);
                    legalValuesColumn.ShouldSort = shouldSort;
                    legalValuesColumn.SourceTableName = sourceTableName;
                    legalValuesColumn.TextColumnName = textColumnName;
                    legalValuesColumn.IsExclusiveTable = isExclusiveTable;
                    legalValuesColumn.Name = txtFieldName.Text;
                    legalValuesColumn.Text = txtColumnName.Text;
                    legalValuesColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    legalValuesColumn.IsRequired = cbxRequired.Checked;
                    legalValuesColumn.IsReadOnly = cbxReadOnly.Checked;
                    legalValuesColumn.IsUniqueField = cbxUniqueField.Checked;
                    legalValuesColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    legalValuesColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth; ;
                    legalValuesColumn.Grid = grid;
                    gridColumns.Add(legalValuesColumn);
                    SetColumnStyle(legalValuesColumn);
                    break;
                case MetaFieldType.Checkbox:
                    CheckboxColumn checkboxColumn = new CheckboxColumn(grid);
                    checkboxColumn.Name = txtFieldName.Text;
                    checkboxColumn.Text = txtColumnName.Text;
                    checkboxColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    checkboxColumn.IsRequired = cbxRequired.Checked;
                    checkboxColumn.IsReadOnly = cbxReadOnly.Checked;
                    checkboxColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;//gridColumnsTable.Columns.Count;
                    checkboxColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    checkboxColumn.Grid = grid;
                    gridColumns.Add(checkboxColumn);
                    SetColumnStyle(checkboxColumn);
                    break;
                case MetaFieldType.YesNo:
                    YesNoColumn yesNoColumn = new YesNoColumn(grid);
                    yesNoColumn.ShouldSort = shouldSort;
                    yesNoColumn.SourceTableName = sourceTableName;
                    yesNoColumn.TextColumnName = textColumnName;
                    yesNoColumn.IsExclusiveTable = isExclusiveTable;
                    yesNoColumn.Name = txtFieldName.Text;
                    yesNoColumn.Text = txtColumnName.Text;
                    yesNoColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    yesNoColumn.IsRequired = cbxRequired.Checked;
                    yesNoColumn.IsReadOnly = cbxReadOnly.Checked;
                    yesNoColumn.IsUniqueField = cbxUniqueField.Checked;
                    yesNoColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    yesNoColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth; ;
                    yesNoColumn.Grid = grid;
                    gridColumns.Add(yesNoColumn);
                    SetColumnStyle(yesNoColumn);
                    break;
            }
        }

        private void SetContiguousColumnProperties(ContiguousColumn contiguousColumn)
        {
            contiguousColumn.Name = txtFieldName.Text;
            contiguousColumn.Text = txtColumnName.Text;
            contiguousColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
            contiguousColumn.IsRequired = cbxRequired.Checked;
            contiguousColumn.IsReadOnly = cbxReadOnly.Checked;
            contiguousColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
            contiguousColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
            contiguousColumn.Pattern = cmbPattern.Text;

            if (cbxRange.Checked)
            {
                DateTime upper;
                DateTime lower;
                bool isValid = true;
                if (!DateTime.TryParse(txtLower.Text, out lower))
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.INVALID_LOWER_DATE_RANGE);
                }
                if (!DateTime.TryParse(txtUpper.Text, out upper))
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.INVALID_UPPER_DATE_RANGE);
                }
                if (isValid)
                {
                    if (lower.CompareTo(upper) > 0)
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.UPPER_DATE_RANGE_IS_LOWER);
                    }
                }

                if (isValid)
                {
                    contiguousColumn.Lower = txtLower.Text;
                    contiguousColumn.Upper = txtUpper.Text;
                }
                else
                {
                    ShowErrorMessages();
                    //break;
                }
            }
            gridColumns.Add(contiguousColumn);
            SetColumnStyle(contiguousColumn);
        }

        /// <summary>
        /// Adds the GridColumnStyle for a column to the collection
        /// </summary>
        /// <param name="column">Column to add</param>
        private void SetColumnStyle(GridColumnBase column)
        {
            bool columnExists = false;
            DataGridTextBoxColumn textColumn = new DataGridTextBoxColumn();
            textColumn.MappingName = column.Name;
            textColumn.HeaderText = column.Text;
            textColumn.Width = (column.Width > 0 ? column.Width:75);
            textColumn.NullText = string.Empty;
            textColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);

            foreach (DataGridColumnStyle col in dgColumns.TableStyles["GridColumns"].GridColumnStyles)
            {
                if (col.MappingName.Equals(textColumn.MappingName))
                {
                    columnExists = true;
                }
            }

            if (!columnExists)
            {
                this.dgColumns.TableStyles["GridColumns"].GridColumnStyles.Add(textColumn);
            }
        }

        /// <summary>
        /// Updates the style of an existing column
        /// </summary>
        /// <param name="position">Position of the original column</param>
        /// <param name="column">Updated column</param>
        private void UpdateColumnStyle(int position, GridColumnBase column)
        {
            position = position - iColumnOffset;
            DataGridColumnStyle[] temp = new DataGridColumnStyle[dgColumns.TableStyles["GridColumns"].GridColumnStyles.Count];
            dgColumns.TableStyles["GridColumns"].GridColumnStyles.CopyTo(temp, 0);
            List<DataGridColumnStyle> styles = new List<DataGridColumnStyle>(temp);

            DataGridTextBoxColumn textColumn = new DataGridTextBoxColumn();
            textColumn.MappingName = column.Name;
            textColumn.HeaderText = column.Text;
            textColumn.Width = (column.Width > 0 ? column.Width : 75);
            textColumn.NullText = string.Empty;
            textColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);

            styles.RemoveAt(position);
            styles.Insert(position, textColumn);

            dgColumns.TableStyles["GridColumns"].GridColumnStyles.Clear();
            dgColumns.TableStyles["GridColumns"].GridColumnStyles.AddRange(styles.ToArray());
        }

        /// <summary>
        /// Adds the GridColumnStyle of a column to the collection
        /// </summary>
        /// <param name="column">name of the column to add</param>
        private void SetColumnStyle(string column)
        {
            DataGridTextBoxColumn textColumn = new DataGridTextBoxColumn();
            textColumn.MappingName = column;
            textColumn.HeaderText = column;
            textColumn.NullText = string.Empty;
            textColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);
            this.dgColumns.TableStyles["GridColumns"].GridColumnStyles.Add(textColumn);
        }

        /// <summary>
        /// Displays the properties of a desired column
        /// </summary>
        /// <param name="columnNumber">Index of the column in the datagrid</param>
        private void ShowColumnProperties(int columnNumber)
        {
            txtFieldName.Text = gridColumns[columnNumber].Name;
            txtColumnName.Text = gridColumns[columnNumber].Text;

            bool hasDataTable = ((Epi.Windows.MakeView.Forms.MakeViewMainForm)form).mediator.Project.CollectedData.TableExists(page.TableName);

            if (hasDataTable)
            {
                txtFieldName.Enabled = false;
            }
            else
            {
                txtFieldName.Enabled = true;
            }

            cbxRepeatLast.Checked = gridColumns[columnNumber].ShouldRepeatLast;
            cbxRequired.Checked = gridColumns[columnNumber].IsRequired;
            cbxReadOnly.Checked = gridColumns[columnNumber].IsReadOnly;
            cbxUniqueField.Checked = gridColumns[columnNumber].IsUniqueField;
            txtDataSource.Text = string.Empty; // Clear data source text box before attempting to assign the current datasource text.
            btnDataSource.Enabled = false;
            
            if (gridColumns[columnNumber] is DDLColumnOfCommentLegal)
            {
                txtDataSource.Text = ((DDLColumnOfCommentLegal)gridColumns[columnNumber]).SourceTableName + " :: " + ((DDLColumnOfCommentLegal)gridColumns[columnNumber]).TextColumnName;
                btnDataSource.Enabled = true;
            }
            
            if (gridColumns[columnNumber] is DDLColumnOfLegalValues)
            {
                txtDataSource.Text = ((DDLColumnOfLegalValues)gridColumns[columnNumber]).SourceTableName + " :: " + ((DDLColumnOfLegalValues)gridColumns[columnNumber]).TextColumnName;
                btnDataSource.Enabled = true;
            }
            
            cmbFieldType.SelectedValue = gridColumns[columnNumber].GridColumnType;
            
            if (gridColumns[columnNumber] is TextColumn)
            {
                cmbSize.Text = ((TextColumn)gridColumns[columnNumber]).Size.ToString();
            }
            else
            {
                cmbSize.Text = string.Empty;
            }
            
            if (gridColumns[columnNumber] is NumberColumn)
            {
                cmbPattern.SelectedIndex = cmbPattern.FindStringExact(((NumberColumn)gridColumns[columnNumber]).Pattern);
                txtUpper.Text = ((NumberColumn)gridColumns[columnNumber]).Upper;
                txtLower.Text = ((NumberColumn)gridColumns[columnNumber]).Lower;
                cbxRange.Checked = ((NumberColumn)gridColumns[columnNumber]).Lower.Length > 0;
            }
            
            if (gridColumns[columnNumber] is PhoneNumberColumn)
            {
                cmbPattern.Text = ((PhoneNumberColumn)gridColumns[columnNumber]).Pattern;

            }
            
            if (gridColumns[columnNumber] is DateColumn || gridColumns[columnNumber] is TimeColumn || gridColumns[columnNumber] is DateTimeColumn)
            {
                cmbPattern.Text = ((ContiguousColumn)gridColumns[columnNumber]).Pattern;
                txtUpper.Text = ((ContiguousColumn)gridColumns[columnNumber]).Upper;
                txtLower.Text = ((ContiguousColumn)gridColumns[columnNumber]).Lower;
                cbxRange.Checked = ((ContiguousColumn)gridColumns[columnNumber]).Lower.Length > 0;
            }
        }

        /// <summary>
        /// Display the DDLField*Dialog that creates/uses the code table.
        /// </summary>
        /// <param name="legalDialog">DDField*Dialog</param>
        private void ShowDDLFieldDialog(LegalValuesDialog legalDialog)
        {
            DialogResult result;
            if (legalDialog is CommentLegalDialog)
            {
                result = ((CommentLegalDialog)legalDialog).ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(((CommentLegalDialog)legalDialog).SourceTableName) && !string.IsNullOrEmpty(((CommentLegalDialog)legalDialog).TextColumnName))
                    {
                        txtDataSource.Text = ((CommentLegalDialog)legalDialog).SourceTableName + " :: " + ((CommentLegalDialog)legalDialog).TextColumnName;
                        shouldSort = ((CommentLegalDialog)legalDialog).ShouldSort;
                        sourceTableName = ((CommentLegalDialog)legalDialog).SourceTableName;
                        textColumnName = ((CommentLegalDialog)legalDialog).TextColumnName;
                        isExclusiveTable = ((CommentLegalDialog)legalDialog).IsExclusiveTable;
                    }
                    else 
                    {
                        txtDataSource.Text = string.Empty;
                        sourceTableName = string.Empty;
                        textColumnName = string.Empty;
                    }
                }
            }
            else
            {
                result = legalDialog.ShowDialog();
                if (!string.IsNullOrEmpty(legalDialog.SourceTableName) && !string.IsNullOrEmpty(legalDialog.TextColumnName))
                {
                    txtDataSource.Text = legalDialog.SourceTableName + " :: " + legalDialog.TextColumnName;
                    shouldSort = legalDialog.ShouldSort;
                    sourceTableName = legalDialog.SourceTableName;
                    textColumnName = legalDialog.TextColumnName;
                    isExclusiveTable = legalDialog.IsExclusiveTable;
                }
                else
                {
                    txtDataSource.Text = string.Empty;
                    sourceTableName = string.Empty;
                    textColumnName = string.Empty;
                }
            }
        }

        /// <summary>
        /// Updates column data 
        /// </summary>
        private void UpdateColumnData()
        {
            currentGridColumn = hti.Column + iColumnOffset;
            MetaFieldType metaFieldTypeEnum = (MetaFieldType)cmbFieldType.SelectedValue;

            switch (metaFieldTypeEnum)
            {
                case MetaFieldType.Text:
                    TextColumn textColumn;
                    textColumn = new TextColumn(grid);
                    textColumn.Name = txtFieldName.Text;
                    textColumn.Text = txtColumnName.Text;
                    textColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    textColumn.IsRequired = cbxRequired.Checked;
                    textColumn.IsReadOnly = cbxReadOnly.Checked;
                    textColumn.Position = hti.Column;
                    textColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width;

                    if (!string.IsNullOrEmpty(cmbSize.Text))
                    {
                        textColumn.Size = int.Parse(cmbSize.Text);
                    }
                    textColumn.Grid = grid;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, textColumn);
                    UpdateColumnStyle(currentGridColumn, textColumn);
                    break;

                case MetaFieldType.Number:
                    NumberColumn numberColumn;
                    numberColumn = new NumberColumn(grid);
                    numberColumn.Name = txtFieldName.Text;
                    numberColumn.Text = txtColumnName.Text;
                    numberColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    numberColumn.IsRequired = cbxRequired.Checked;
                    numberColumn.IsReadOnly = cbxReadOnly.Checked;
                    numberColumn.Position = hti.Column;
                    numberColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width; // 500;
                    numberColumn.Pattern = cmbPattern.Text;
                    if (cbxRange.Checked)
                    {
                        numberColumn.Lower = txtLower.Text;
                        numberColumn.Upper = txtUpper.Text;
                    }

                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, numberColumn);
                    UpdateColumnStyle(currentGridColumn, numberColumn);
                    break;

                case MetaFieldType.PhoneNumber:
                    PhoneNumberColumn phoneNumberColumn;
                    phoneNumberColumn = new PhoneNumberColumn(grid);
                    phoneNumberColumn.Name = txtFieldName.Text;
                    phoneNumberColumn.Text = txtColumnName.Text;
                    phoneNumberColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    phoneNumberColumn.IsRequired = cbxRequired.Checked;
                    phoneNumberColumn.IsReadOnly = cbxReadOnly.Checked;
                    phoneNumberColumn.Position = hti.Column;
                    phoneNumberColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width;
                    phoneNumberColumn.Pattern = cmbPattern.Text;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, phoneNumberColumn);
                    UpdateColumnStyle(currentGridColumn, phoneNumberColumn);
                    break;

                case MetaFieldType.Date:
                case MetaFieldType.Time:
                case MetaFieldType.DateTime:

                    ContiguousColumn column;
                    switch (metaFieldTypeEnum)
                    {
                        case MetaFieldType.Date:
                            column = new DateColumn(grid);
                            break;
                        case MetaFieldType.Time:
                            column = new TimeColumn(grid);
                            break;
                        case MetaFieldType.DateTime:
                            column = new DateTimeColumn(grid);
                            break;
                        default:
                            column = new DateColumn(grid);
                            break;
                    }

                    column.Name = txtFieldName.Text;
                    column.Text = txtColumnName.Text;
                    column.ShouldRepeatLast = cbxRepeatLast.Checked;
                    column.IsRequired = cbxRequired.Checked;
                    column.IsReadOnly = cbxReadOnly.Checked;
                    column.Position = hti.Column;
                    column.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width; // 500;
                    column.Pattern = cmbPattern.Text;
                    if (cbxRange.Checked)
                    {
                        column.Lower = txtLower.Text;
                        column.Upper = txtUpper.Text;
                    }
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, column);
                    UpdateColumnStyle(currentGridColumn, column);
                    break;

                case MetaFieldType.CommentLegal:
                    DDLColumnOfCommentLegal commentLegalColumn;
                    commentLegalColumn = new DDLColumnOfCommentLegal(grid);

                    commentLegalColumn.Name = txtFieldName.Text;
                    commentLegalColumn.Text = txtColumnName.Text;
                    commentLegalColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    commentLegalColumn.IsRequired = cbxRequired.Checked;
                    commentLegalColumn.IsReadOnly = cbxReadOnly.Checked;
                    commentLegalColumn.IsUniqueField = cbxUniqueField.Checked;
                    commentLegalColumn.Position = hti.Column;
                    commentLegalColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width;
                    if (txtDataSource.Text.Length > 4)
                    {
                        commentLegalColumn.SourceTableName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        commentLegalColumn.TextColumnName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    }
                    commentLegalColumn.Grid = grid;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, commentLegalColumn);
                    UpdateColumnStyle(currentGridColumn, commentLegalColumn);
                    btnDataSource.Enabled = true;
                    cbxUniqueField.Enabled = true;
                    break;

                case MetaFieldType.LegalValues:
                    DDLColumnOfLegalValues legalValuesColumn;
                    legalValuesColumn = new DDLColumnOfLegalValues(grid);
                    legalValuesColumn.Name = txtFieldName.Text;
                    legalValuesColumn.Text = txtColumnName.Text;
                    legalValuesColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    legalValuesColumn.IsRequired = cbxRequired.Checked;
                    legalValuesColumn.IsReadOnly = cbxReadOnly.Checked;
                    legalValuesColumn.IsUniqueField = cbxUniqueField.Checked;
                    legalValuesColumn.Position = hti.Column;
                    legalValuesColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width;
                    if (txtDataSource.Text.Length > 4)
                    {
                        legalValuesColumn.SourceTableName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        legalValuesColumn.TextColumnName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    }
                    legalValuesColumn.Grid = grid;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, legalValuesColumn);
                    UpdateColumnStyle(currentGridColumn, legalValuesColumn);
                    btnDataSource.Enabled = true;
                    cbxUniqueField.Enabled = true;
                    break;

                case MetaFieldType.Checkbox:
                    CheckboxColumn checkboxColumn = new CheckboxColumn(grid);
                    checkboxColumn.Name = txtFieldName.Text;
                    checkboxColumn.Text = txtColumnName.Text;
                    checkboxColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    checkboxColumn.IsRequired = cbxRequired.Checked;
                    checkboxColumn.IsReadOnly = cbxReadOnly.Checked;
                    checkboxColumn.Position = gridColumnsTable.Columns[txtFieldName.Text].Ordinal;
                    checkboxColumn.Width = this.dgColumns.TableStyles["GridColumns"].PreferredColumnWidth;
                    checkboxColumn.Grid = grid;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, checkboxColumn);
                    UpdateColumnStyle(currentGridColumn, checkboxColumn);
                    btnDataSource.Enabled = true;
                    cbxUniqueField.Enabled = true;
                    break;

                case MetaFieldType.YesNo:
                    YesNoColumn yesNoColumn;
                    yesNoColumn = new YesNoColumn(grid);
                    yesNoColumn.Name = txtFieldName.Text;
                    yesNoColumn.Text = txtColumnName.Text;
                    yesNoColumn.ShouldRepeatLast = cbxRepeatLast.Checked;
                    yesNoColumn.IsRequired = cbxRequired.Checked;
                    yesNoColumn.IsReadOnly = cbxReadOnly.Checked;
                    yesNoColumn.IsUniqueField = cbxUniqueField.Checked;
                    yesNoColumn.Position = hti.Column;
                    yesNoColumn.Width = this.dgColumns.TableStyles["GridColumns"].GridColumnStyles[hti.Column].Width;
                    
                    if (txtDataSource.Text.Length > 4)
                    {
                        yesNoColumn.SourceTableName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        yesNoColumn.TextColumnName = txtDataSource.Text.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    }

                    yesNoColumn.Grid = grid;
                    gridColumns.RemoveAt(currentGridColumn);
                    gridColumns.Insert(currentGridColumn, yesNoColumn);
                    UpdateColumnStyle(currentGridColumn, yesNoColumn);
                    btnDataSource.Enabled = true;
                    cbxUniqueField.Enabled = true;
                    break;
            }
        }
        #endregion	

        #region Event Handlers

        /// <summary>
        /// Opens Dialog for Legal Values or Comment Legal column.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void btnDataSource_Click(object sender, EventArgs e)
        {
            if (hti != null)
            {
                if (hti.Type == DataGrid.HitTestType.ColumnHeader)
                {
                    object currentColumn = gridColumns[currentGridColumn];
                    currentGridColumn = hti.Column + iColumnOffset;

                    LegalValuesDialog sourceTableDialog = null;

                    switch ((int)cmbFieldType.SelectedValue)
                    {
                        case ((int)MetaFieldType.CommentLegal):
                            DDLColumnOfCommentLegal dDLColumnOfCommentLegal = new Epi.Fields.DDLColumnOfCommentLegal((GridField)Grid);
                            if (currentColumn is DDLColumnOfCommentLegal)  dDLColumnOfCommentLegal = (DDLColumnOfCommentLegal)currentColumn;
                            sourceTableDialog = new CommentLegalDialog(dDLColumnOfCommentLegal, form, txtFieldName.Text, page);

                            break;

                        case ((int)MetaFieldType.LegalValues):
                            DDLColumnOfLegalValues dDLColumnOfLegalValues = new Epi.Fields.DDLColumnOfLegalValues((GridField)Grid);
                            if (currentColumn is DDLColumnOfLegalValues) dDLColumnOfLegalValues = (DDLColumnOfLegalValues)currentColumn;
                            sourceTableDialog = new LegalValuesDialog(dDLColumnOfLegalValues, form, txtFieldName.Text, page);
                            break;
                    }

                    if (sourceTableDialog != null)
                    {
                        ShowDDLFieldDialog(sourceTableDialog);
                        currentColumn = ((LegalValuesDialog)sourceTableDialog).DdlColumn;
                    }
                }
                else
                {
                    if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal))
                    {
                        TableBasedDropDownColumn column = new DDLColumnOfCommentLegal(grid);
                        CommentLegalDialog commentLegal = new CommentLegalDialog(column, form, txtFieldName.Text, page);
                        ShowDDLFieldDialog(commentLegal);
                    }

                    if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues))
                    {
                        TableBasedDropDownColumn column = new DDLColumnOfLegalValues(grid);
                        LegalValuesDialog legalValues = new LegalValuesDialog(column, form, txtFieldName.Text, page);
                        ShowDDLFieldDialog(legalValues);
                    }
                }
            }
            else
            {
                if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal))
                {
                    TableBasedDropDownColumn column = new DDLColumnOfCommentLegal(grid);
                    CommentLegalDialog commentLegal = new CommentLegalDialog(column, form, txtFieldName.Text, page);
                    ShowDDLFieldDialog(commentLegal);
                }

                if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues))
                {
                    TableBasedDropDownColumn column = new DDLColumnOfLegalValues(grid);
                    LegalValuesDialog legalValues = new LegalValuesDialog(column, form, txtFieldName.Text, page);
                    ShowDDLFieldDialog(legalValues);
                }
            }
            btnSaveColumn.Enabled = (!String.IsNullOrEmpty(txtDataSource.Text));
        }

        /// <summary>
        /// Handles the click event of the Ok button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtFieldName.Text))
            {
                //Defect 1444

                MsgBox.Show("The column has not been saved. Click the Save Column button.", "MakeView");
                return;
            }
            else
            {

                this.Hide();
                // DEFECT# 319
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Handles the click event of the Save button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void btnSaveColumn_Click(object sender, System.EventArgs e)
        {
          
            if (string.IsNullOrEmpty(txtColumnName.Text.Trim()))
            {
                MsgBox.ShowError(SharedStrings.ENTER_COLUMN_NAME_FOR_GRID);
                return;
            }

            if (string.IsNullOrEmpty(txtFieldName.Text))
            {
                MsgBox.ShowError(SharedStrings.ENTER_FIELD_NAME);
                return;
            }

            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues) || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal))
            {
                if (string.IsNullOrEmpty(txtDataSource.Text.Trim()))
                {
                    MsgBox.ShowError(SharedStrings.SELECT_DATA_SOURCE);
                    return;
                }
            }
            if (cbxRange.Checked)
            {
                bool isValid = true;
                if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Date))
                {
                    DateTime upper;
                    DateTime lower;
                    if (!DateTime.TryParse(txtLower.Text, out lower))
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.INVALID_LOWER_DATE_RANGE);
                    }
                    if (!DateTime.TryParse(txtUpper.Text, out upper))
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.INVALID_UPPER_DATE_RANGE);
                    }
                    if (isValid)
                    {
                        if (lower.CompareTo(upper) > 0)
                        {
                            isValid = false;
                            ErrorMessages.Add(SharedStrings.UPPER_DATE_RANGE_IS_LOWER);
                        }
                    }

                }
                else if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
                {
                    double lower = double.Parse(txtLower.Text);
                    double upper = double.Parse(txtUpper.Text);

                    if (lower >= upper)
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.UPPER_NUMBER_RANGE_IS_LOWER);
                    }
                }
                if (!isValid)
                {
                    ShowErrorMessages();
                    return;
                }
            }

            if (!string.IsNullOrEmpty(originalColumnName))
            {
                if (!originalColumnName.Equals(txtFieldName.Text.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    gridColumnsTable.Columns[originalColumnName].ColumnName = txtFieldName.Text.Trim();
                }
                UpdateColumnData();
            }
            else
            {
                gridColumnsTable.Columns.Add(txtFieldName.Text);
                SaveColumnData();
            }

            //if ((cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal) || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues)) && !string.IsNullOrEmpty(txtDataSource.Text))
            //{
            //    btnOk.Enabled = true;
            //}

            //if (!cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal) || !cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues))
            //{
            //    btnOk.Enabled = true;
            //}

            RestoreDefaultProperties();
            txtFieldName.Text = string.Empty;
            txtColumnName.Text = string.Empty;
            txtFieldName.Enabled = true;
            txtDataSource.Text = string.Empty;
            txtColumnName.Focus();
            originalColumnName = string.Empty;


            //LoadGridData();
        }

        /// <summary>
        /// Handles the change event of the Range selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void cbxRange_CheckedChanged(object sender, System.EventArgs e)
        {
            lblLower.Visible = cbxRange.Checked;
            txtLower.Visible = cbxRange.Checked;
            lblUpper.Visible = cbxRange.Checked;
            txtUpper.Visible = cbxRange.Checked;
            if (!cbxRange.Checked)
            {
                txtLower.Text = string.Empty;
                txtUpper.Text = string.Empty;
            }
        }

        /// <summary>
        /// Occurs when the value of the Checked property changes.
        /// <remarks>Ensures that only cbxRequired or cbxReadOnly are checked at one time.</remarks>
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void cbxReadOnly_CheckedChanged(object sender, EventArgs e)
        {
            // DEFECT# 332
            cbxRequired.Enabled = !cbxReadOnly.Checked;
        }

        /// <summary>
        /// Occurs when the value of the Checked property changes.
        /// <remarks>Ensures that only cbxRequired or cbxReadOnly are checked at one time.</remarks>
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void cbxRequired_CheckedChanged(object sender, EventArgs e)
        {
            // DEFECT# 332
            cbxReadOnly.Enabled = !cbxRequired.Checked;

        }

        /// <summary>
        /// Handles the change event of the Field Type selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void cmbFieldType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            EnableDisableFields();
        }

        /// <summary>
        /// Handles the leave event of the columns grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void dgColumns_Leave(object sender, System.EventArgs e)
        {
            this.AcceptButton = btnOk;
        }

        /// <summary>
        /// Handles the mouse down event of the columns grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void dgColumns_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DataGrid gvCols = (DataGrid)sender;
            hti = gvCols.HitTest(e.X, e.Y);

            switch (hti.Type)
            {
                case DataGrid.HitTestType.ColumnHeader:
                    txtColumnName.Text = gridColumnsTable.Columns[hti.Column].ColumnName;
                    currentGridColumn = hti.Column + iColumnOffset;
                    btnDataSource.Enabled = false;
                    ShowColumnProperties(currentGridColumn);
                    originalColumnName = txtFieldName.Text.Trim();
                                    
                    if (e.Button.Equals(MouseButtons.Right))
                    {
                        ContextMenu dgMenu = new ContextMenu();
                        MenuItem delete = new MenuItem(SharedStrings.DELETE);
                        delete.Click += new EventHandler(delete_Click);

                        dgMenu.MenuItems.Add(delete);

                        dgMenu.Show((Control)sender, e.Location);
                    }
                    break;
                case DataGrid.HitTestType.ColumnResize:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Initialize the state of the form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void GridColumns_Load(object sender, System.EventArgs e)
        {
            LoadSettings();
            LoadGridData();
            EnableDisableFields();

            btnOk.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void textColumn_WidthChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gridColumns.Count; i++)
            {
                if (((DataGridTextBoxColumn)sender).MappingName == gridColumns[i].Name)
                {
                    gridColumns[i].Width = ((DataGridTextBoxColumn)sender).Width;
                    break;
                }
            }
        }

        /// <summary>
        /// Occurs when the control is double-clicked. Creates selected text to be use as a field name for the current/new grid column.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void txtColumnName_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtColumnName.Text))
            {
                if (txtColumnName.Text.Length > 64)
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(txtColumnName.SelectedText.Substring(0, 64));
                }
                else
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(txtColumnName.SelectedText);
                }
            }
        }

        /// <summary>
        /// Handles the change event of the column name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void txtColumnName_Leave(object sender, System.EventArgs e)
        {
            if (!String.IsNullOrEmpty(originalColumnName))
            {
                if (txtFieldName.Text.Equals(originalColumnName, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }
            if (!string.IsNullOrEmpty(txtColumnName.Text) )
            {
                if (txtColumnName.Text.Length > 64)
                {
                    txtFieldName.Text = this.CreateGridColumnFieldName(txtColumnName.Text.Substring(0, 64));
                }
                else
                {
                    txtFieldName.Text = this.CreateGridColumnFieldName(txtColumnName.Text);
                }
            }
        }

        /// <summary>
        /// Event used to set the state of the form controls
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void txtColumnName_TextChanged(object sender, EventArgs e)
        {
            if (txtColumnName.Text.Length > 64)
            {
                txtColumnName.Text = txtColumnName.Text.Substring(0, 64);
            }
            
            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnSaveColumn.Enabled = true;

                if (cmbFieldType.SelectedValue != null)
                {
                    if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.CommentLegal) || cmbFieldType.SelectedValue.Equals((int)MetaFieldType.LegalValues))
                    {
                        if (string.IsNullOrEmpty(txtDataSource.Text))
                        {
                            btnSaveColumn.Enabled = false;
                        }
                    }
                }
            }
            else
            {
                btnOk.Enabled = true;
            }
        }

        /// <summary>
        /// Event used to set the state of the form controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDataSource_TextChanged(object sender, EventArgs e)
        {
           //btnSaveColumn.Enabled = !string.IsNullOrEmpty(txtDataSource.Text);
       }

        // the following two sections of code fix defect #284
        /// <summary>
        /// Prevent numeric keypresses in txtUpper when we have selected 
        /// dropdown for Number when entering a Range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUpper_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
            {
                if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
                {
                    if (e.KeyChar != '.')
                    {
                        int isNumber = 0;
                        e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Event to format the range values when the text box is left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RangeValue_Leave(object sender, EventArgs e)
        {
            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
            {
                if (!String.IsNullOrEmpty(((Control)sender).Text))
                {
                    string mask = cmbPattern.Text;
                    string numberInput = ((Control)sender).Text;
                    if (mask.Contains("."))
                    {
                        mask = "{0:" + mask.Replace("#", "0") + "}";
                        Match spaceMatch = Regex.Match(numberInput, @"\.*[\s][0-9]");
                        if (spaceMatch.Success)
                        {
                            numberInput = Regex.Replace(numberInput, @"[\s]", "0");
                        }
                        else
                        {
                            numberInput = Regex.Replace(numberInput, @"[\s]", "");
                        }
                        numberInput = String.Format(mask, float.Parse(numberInput));
                    }
                    else
                    {
                        int count = 0;
                        foreach (char c in mask) //{0:00} fails to produce leading zeros
                        {
                            if (c.Equals('#')) count++;
                        }
                        numberInput = ((int)Math.Round(double.Parse(numberInput), 0)).ToString(string.Format("D{0}", count));
                    }
                    ((Control)sender).Text = numberInput;
                }
            }
        }


        /// <summary>
        /// Prevent numeric keypresses in txtLower when we have selected 
        /// dropdown for Number when entering a Range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLower_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (cmbFieldType.SelectedValue.Equals((int)MetaFieldType.Number))
            {
                if (e.KeyChar != '.')
                {
                    int isNumber = 0;
                    e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                }
            }
        }

        /// <summary>
        /// Validates the field name when the txtFieldName textbox is left.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFieldName_Leave(object sender, EventArgs e)
        {

            if (!String.IsNullOrEmpty(originalColumnName))
            {
                if (txtFieldName.Text.Equals(originalColumnName, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }
            
            if (!string.IsNullOrEmpty(((TextBox)sender).Text) && !((TextBox)sender).Text.Equals(originalColumnName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (txtColumnName.Text.Length > 64)
                {
                    txtFieldName.Text = this.CreateGridColumnFieldName(((TextBox)sender).Text.Substring(0, 64));
                }
                else
                {
                    txtFieldName.Text = this.CreateGridColumnFieldName(((TextBox)sender).Text);
                }
            }
        }

        void txtFieldName_TextChanged(object sender, System.EventArgs e)
        {
            EnableDisableFields();
        }	

        void delete_Click(object sender, EventArgs e)
        {
            DialogResult confirmDelete = MsgBox.ShowQuestion(SharedStrings.CONFIRM_DELETE_GRID_COLUMN, MessageBoxButtons.YesNo);
            if (confirmDelete.Equals(DialogResult.Yes))
            {
                string columnName = string.Empty;
                List<GridColumnBase> tempGrid = null;

                if (!Util.IsEmpty(dgColumns.DataSource))
                {
                    columnName = ((DataTable)dgColumns.DataSource).Columns[hti.Column].ColumnName;
                    tempGrid = new List<GridColumnBase>(gridColumns);

                    foreach (GridColumnBase col in tempGrid)
                    {
                        if (col.Name.Equals(columnName))
                        {
                            gridColumns.Remove(col);
                            break;
                        }
                    }

                    DataGridColumnStyle[] temp = new DataGridColumnStyle[dgColumns.TableStyles["GridColumns"].GridColumnStyles.Count];
                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.CopyTo(temp, 0);
                    List<DataGridColumnStyle> styles = new List<DataGridColumnStyle>(temp);
                    styles.RemoveAt(hti.Column);

                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.Clear();
                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.AddRange(styles.ToArray());

                    tempGrid = null;

                    RestoreDefaultProperties();
                    txtColumnName.Text = string.Empty;
                    txtFieldName.Text = string.Empty;
                    txtFieldName.Enabled = true;
                    txtDataSource.Text = string.Empty;
                    txtColumnName.Focus();
                    originalColumnName = string.Empty;
                    currentGridColumn = -1;

                    if (dgColumns.DataSource is DataTable)
                    {
                        if (dgColumns.TableStyles["GridColumns"].GridColumnStyles.Count.Equals(0))
                        {
                            btnOk.Enabled = false;
                        }
                        else
                        {
                            btnOk.Enabled = true;
                        }
                    }
                }
            }
        }
        #endregion


   }
}
