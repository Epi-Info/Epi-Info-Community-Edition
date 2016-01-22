using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog for Recode command
    /// </summary>
    public partial class RecodeDialog : CommandDesignDialog
    {
        #region Private Class Members
        private bool fillRanges;
        private int editingRow = -1;
        int visiblecolumnscount = 0;
        #endregion //Private Class Members
        
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public RecodeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the Recode dialog
        /// </summary>
        /// <param name="frm"></param>
        public RecodeDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        #endregion Constructors

        #region Event Handlers
        /// <summary>
        /// Sets the processing mode and generates command 
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnOK_Click(object sender, System.EventArgs e)
        {
            if (fillRanges)
            {
                FillDataGrid();
                ToggleControls(false);  //also toggles fillRanges
                CheckForInputSufficiency();
            }
            else
            {
                base.btnOK_Click(sender, e);
            }
        }

        private void btnFillRanges_Click(object sender, System.EventArgs e)
        {
            fillRanges = true;
            LoadDataGrid();
            ToggleControls(true);
            txtStart.Text = String.Empty;
            txtEnd.Text = String.Empty;
            txtBy.Text = String.Empty;
            cbxReverse.Checked = false;
            CheckForInputSufficiency();  //disables the OK button until required fields are satisfied.
        }

        private void RecodeDialog_Load(object sender, System.EventArgs e)
        {
            LoadDataGrid();
            LoadVariables();
            GetVisibleColumnscount();
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            LoadDataGrid();
            cmbFrom.SelectedIndex = -1;
            cmbTo.SelectedIndex = -1;
            ToggleControls(false);
            //FillDataGrid();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-recode.html");
        }

        #endregion //Event Handlers

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
                this.dataGrid.KeyDown += new KeyEventHandler(dataGrid_KeyDown);
                this.dataGrid.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGrid_EditingControlShowing);
                this.dataGrid.SelectionChanged += new EventHandler(dataGrid_SelectionChanged);
                //these handle recode issue
                this.cmbFrom.KeyDown += new KeyEventHandler(this.cmbFrom_KeyDown);
                this.cmbTo.KeyDown += new KeyEventHandler(this.cmbTo_KeyDown);
                //these handle the numeric entry issue
                this.txtStart.KeyPress += new KeyPressEventHandler(this.txtStart_KeyPress);
                this.txtEnd.KeyPress += new KeyPressEventHandler(this.txtEnd_KeyPress);
                this.txtBy.KeyPress += new KeyPressEventHandler(this.txtBy_KeyPress);
            }
        }

        //defects 1173 and 1174
        
        void txtBy_KeyPress(object sender, KeyPressEventArgs e)
        {
         
            //Allow only digits, negative sign, and decimal separators.
            //May need to make this culturally aware using the following.
            //   |-> may need a reference to using Microsoft.WindowsCE.Forms or some equivalent.
            //
            //NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            //string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            //string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            //string negativeSign = numberFormatInfo.NegativeSign;

            string keyInput = e.KeyChar.ToString();
            
            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar.Equals ('\b'))
            {
                // Backspace key is OK
            }
            else if (e.KeyChar.Equals(Keys.Delete))
            {
                // Delete key is OK
            }
            else if (e.KeyChar.Equals(Keys.Enter))
            {
                // Enter key is OK
            }
            else if (keyInput.Equals(StringLiterals.HYPHEN))
            {
                if (txtBy.TextLength == 1)
                {
                    // Dash (for negative sign) is OK as long as it is the first character.
                    //ToDo: Implement a culturally aware way to denote the negative sign.
                }
                else
                {
                    e.Handled = true;
                }
            }
            else if (keyInput.Equals(StringLiterals.PERIOD))
            {
                // Period (for decimal sign) is OK  
                //ToDo: Implement a culturally aware way to denote the decimal separator.
            }
            else
            {
                //int isNumber = 0;
                //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                e.Handled = true;
            }
        }

        void txtEnd_KeyPress(object sender, KeyPressEventArgs e)
        {
            //int isNumber = 0;
            //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
            
            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar.Equals('\b'))
            {
                // Backspace key is OK
            }
            else if (e.KeyChar.Equals(Keys.Delete))
            {
                // Delete key is OK
            }
            else if (e.KeyChar.Equals(Keys.Enter))
            {
                // Enter key is OK
            }
            else if (keyInput.Equals(StringLiterals.HYPHEN))
            {
                if (txtBy.TextLength == 1)
                {
                    // Dash (for negative sign) is OK as long as it is the first character.
                    //ToDo: Implement a culturally aware way to denote the negative sign.
                }
                else
                {
                    e.Handled = true;
                }
            }
            else if (keyInput.Equals(StringLiterals.PERIOD))
            {
                // Period (for decimal sign) is OK  
                //ToDo: Implement a culturally aware way to denote the decimal separator.
            }
            else
            {
                //int isNumber = 0;
                //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                e.Handled = true;
            }
        }

        void txtStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            //int isNumber = 0;
            //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar.Equals('\b'))
            {
                // Backspace key is OK
            }
            else if (e.KeyChar.Equals(Keys.Delete))
            {
                // Delete key is OK
            }
            else if (keyInput.Equals(StringLiterals.HYPHEN))
            {
                if (txtBy.TextLength == 0)
                {
                    // Dash (for negative sign) is OK as long as it is the first character.
                    //ToDo: Implement a culturally aware way to denote the negative sign.
                }
                else
                {
                    e.Handled = true;
                }
            }
            else if (keyInput.Equals(StringLiterals.PERIOD))
            {
                // Period (for decimal sign) is OK  
                //ToDo: Implement a culturally aware way to denote the decimal separator.
            }
            else
            {
                //int isNumber = 0;
                //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                e.Handled = true;
            }
        }

        //these two procedures should make the behavior mimic Epi 3.x
        private void cmbTo_KeyDown(object sender, KeyEventArgs e)
        {
            //e.SuppressKeyPress = true;
            //e.Handled = true;
        }

        private void cmbFrom_KeyDown(object sender, KeyEventArgs e)
        {
            //e.SuppressKeyPress = true;
            //e.Handled = true;
        }

        void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (editingRow >= 0)
            {
                int newrow = editingRow;
                DataGridViewCell currentCell = dataGrid.CurrentCell;
                if (currentCell != null)
                {
                    if (dataGrid.ColumnCount == currentCell.ColumnIndex + 1) newrow += 1;
                    editingRow = -1;
                    currentCell = dataGrid.Rows[newrow].Cells[currentCell.ColumnIndex];
                }
            }
            CheckForInputSufficiency();
        }

        void dataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            editingRow = dataGrid.CurrentRow.Index;
        }

        void dataGrid_KeyDown(object sender, KeyEventArgs e)
        
        {
            if (e.KeyCode == Keys.Return)
            {
                DataGridViewCell currentCell = dataGrid.CurrentCell;
                if (currentCell != null)
                {
                    int col = currentCell.ColumnIndex;
                    int row = currentCell.RowIndex;
                    col = (col + 1) % visiblecolumnscount;
                    currentCell = dataGrid.CurrentRow.Cells[col];
                    dataGrid.CurrentCell = currentCell;
                }
                e.Handled = true;
            }
        }

        private void ToggleControls(bool visible)
        {
            //When visible == true, Fill Ranges is shown and the Grid is hidden and btnFillRanges is DISabled
            //            == false, the Grid is shown and btnFillRanges is ENabled
            dataGrid.Visible = !(visible);
            btnFillRanges.Enabled = !(visible);

            txtStart.Visible = visible;
            lblStart.Visible = visible;
            txtEnd.Visible = visible;
            lblEnd.Visible = visible;
            txtBy.Visible = visible;
            lblBy.Visible = visible;
            cbxReverse.Visible = visible;
            fillRanges = visible;
        }

        private void LoadDataGrid()
        {
            DataTable dataTable = new DataTable("Recode");
            dataTable.Columns.Add("Value");
            dataTable.Columns.Add("To Value");
            dataTable.Columns.Add("Recoded Value");
            dataTable.Columns.Add("SortID", typeof(Int16));
            //dataGrid.TableStyles.Add(dataGridTableStyle1);
            dataGrid.DataSource = dataTable.DefaultView;
            dataGrid.Columns["SortID"].Visible = false;
            dataGrid.Columns["Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGrid.Columns["To Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGrid.Columns["Recoded Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void FillDataGrid()
        {
            const string SpcHyphenSpc = StringLiterals.SPACE + StringLiterals.HYPHEN + StringLiterals.SPACE;
            DataView dataView = (DataView)dataGrid.DataSource;
            DataRowView dataRow = dataView.AddNew();
            int sortIndex = 0;
            decimal increment, startValue, endValue, byValue;
            decimal.TryParse(txtStart.Text, out startValue);
            decimal.TryParse(txtEnd.Text, out endValue);
            decimal.TryParse(txtBy.Text, out byValue);
            //startValue = System.Convert.ToDecimal(txtStart.Text);
            //endValue = System.Convert.ToDecimal(txtEnd.Text);
            //byValue = System.Convert.ToDecimal(txtBy.Text);
            increment = startValue + byValue;
            if (increment == 0)
            {
                return;
            }
            if (cbxReverse.Checked)
            {
                dataRow["Value"] = CommandNames.LOVALUE;
                dataRow["To Value"] = startValue;
                dataRow["Recoded Value"] = StringLiterals.LESS_THAN + startValue;
                dataRow["SortID"] = sortIndex;
                dataRow.EndEdit();
                //dataView.Rows.Add(dataRow);

                while (increment <= endValue)
                {
                    sortIndex++;
                    dataRow = dataView.AddNew();
                    dataRow["Value"] = increment - byValue;
                    dataRow["To Value"] = increment;
                    dataRow["Recoded Value"] = (increment - byValue) + SpcHyphenSpc + StringLiterals.LESS_THAN + increment;
                    dataRow["SortID"] = sortIndex;
                    //dataView.Rows.Add(dataRow);
                    dataRow.EndEdit();
                    increment += byValue;
                }
                DataRow lastrow = (DataRow)dataView.Table.Rows[dataView.Count - 1];
                if (!lastrow["To Value"].Equals(endValue.ToString()))
                {
                    sortIndex++;
                    dataRow = dataView.AddNew();
                    dataRow["Value"] = lastrow["To Value"];
                    dataRow["To Value"] = endValue;
                    dataRow["Recoded Value"] = lastrow["To Value"] + SpcHyphenSpc + StringLiterals.LESS_THAN + endValue;
                    dataRow["SortID"] = sortIndex;
                    //dataView.Rows.Add(dataRow);
                    dataRow.EndEdit();
                }
                sortIndex++;
                dataRow = dataView.AddNew();
                dataRow["Value"] = endValue;
                dataRow["To Value"] = CommandNames.HIVALUE;
                dataRow["Recoded Value"] = StringLiterals.GREATER_THAN + StringLiterals.EQUAL + endValue;
                dataRow["SortID"] = sortIndex;
                dataRow.EndEdit();
                //dataView.Rows.Add(dataRow);	

                //	DataView dataView = new DataView(dataView);
                dataView.Sort = "SortID desc";
                dataGrid.DataSource = dataView;

            }
            else
            {
                dataRow["Value"] = CommandNames.LOVALUE;
                dataRow["To Value"] = startValue;
                dataRow["Recoded Value"] = StringLiterals.LESS_THAN + StringLiterals.EQUAL + startValue;
                //dataTable.Rows.Add(dataRow);
                dataRow.EndEdit();

                while (increment <= endValue)
                {
                    dataRow = dataView.AddNew();
                    dataRow["Value"] = increment - byValue;
                    dataRow["To Value"] = increment;
                    dataRow["Recoded Value"] = StringLiterals.GREATER_THAN + (increment - byValue) + SpcHyphenSpc + increment;
                    //dataTable.Rows.Add(dataRow);
                    dataRow.EndEdit();
                    increment += byValue;
                }
                DataRow lastrow = (DataRow)dataView.Table.Rows[dataView.Count - 1];
                if (!lastrow["To Value"].Equals(endValue.ToString()))
                {
                    dataRow = dataView.AddNew();
                    dataRow["Value"] = lastrow["To Value"];
                    dataRow["To Value"] = endValue;
                    dataRow["Recoded Value"] = StringLiterals.GREATER_THAN + lastrow["To Value"] + SpcHyphenSpc + endValue;
                    //dataTable.Rows.Add(dataRow);
                    dataRow.EndEdit();
                }
                dataRow = dataView.AddNew();
                dataRow["Value"] = endValue;
                dataRow["To Value"] = CommandNames.HIVALUE;
                dataRow["Recoded Value"] = StringLiterals.GREATER_THAN + endValue;
                //dataTable.Rows.Add(dataRow);
                dataRow.EndEdit();
                dataGrid.DataSource = dataView;
            }
        }
        private void LoadVariables()
        {
            ////Get the list of all variables except global and permanent \\zack 2\20\08
            //// DataTable variables = GetAllVariablesAsDataTable(true, true, true, false);
            //DataTable variables = GetMemoryRegion().GetVariablesAsDataTable(
            //                                        VariableType.DataSource |
            //                                        VariableType.Standard |
            //                                        VariableType.DataSourceRedefined);

            ////Binding the same datatable to two combo boxes does not work because when the 
            ////selectedvalue changes in one combo box, it will change for the second one as well.
            ////This binds data to different instances of dataviews
            //cmbFrom.DataSource = new DataView(variables,string.Empty,ColumnNames.NAME, DataViewRowState.CurrentRows);
            //cmbFrom.DisplayMember = ColumnNames.NAME;
            //cmbFrom.ValueMember = ColumnNames.NAME;

            VariableType scopeWord = VariableType.DataSource | VariableType.Standard |
                                     VariableType.DataSourceRedefined;
            //cmbTo.DataSource = new DataView(variables,string.Empty,ColumnNames.NAME, DataViewRowState.CurrentRows);;
            //cmbTo.DisplayMember = ColumnNames.NAME;
            //cmbTo.ValueMember = ColumnNames.NAME;
            FillVariableCombo(cmbTo, scopeWord);
            cmbTo.SelectedIndex = -1;
            cmbTo.Sorted = true;

            FillVariableCombo(cmbFrom, scopeWord);
            cmbFrom.SelectedIndex = -1;
            cmbFrom.Sorted = true;
        }

        //2181
        void GetVisibleColumnscount()
        {
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                if (column.Visible)
                    visiblecolumnscount = visiblecolumnscount + 1;
            }
        }
        #endregion //Private Methods

        #region Protected Methods
        /// <summary>
        /// Validate user input
        /// </summary>
        /// <returns>True/False depending upon errors stemmed from validation</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (fillRanges)
            {
                //fillRanges = false;
                //ToggleControls(false);
                if ((string.IsNullOrEmpty(txtStart.Text.Trim())) || (string.IsNullOrEmpty(txtEnd.Text.Trim())) || (string.IsNullOrEmpty(txtBy.Text.Trim())))
                {
                    ErrorMessages.Add(SharedStrings.START_END_BY_EMPTY);
                }
            }
            else
            {
                if ((string.IsNullOrEmpty(cmbFrom.Text)) || (string.IsNullOrEmpty(cmbTo.Text)))
                {
                    ErrorMessages.Add(SharedStrings.NO_RECODE_VARIABLE);
                }

                if (((DataView)dataGrid.DataSource).Table.Rows.Count == 0)
                {
                    ErrorMessages.Add(SharedStrings.NO_RECODINGS_DESIGNATED);
                }
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generate the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.RECODE);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbFrom.Text) ? Util.InsertInSquareBrackets(cmbFrom.Text) : cmbFrom.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(CommandNames.TO);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbTo.Text) ? Util.InsertInSquareBrackets(cmbTo.Text) : cmbTo.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(Environment.NewLine);
            foreach (DataRowView row in ((DataView)dataGrid.DataSource))
            {
                double double_compare;
                DateTime dateValue;

                if (string.IsNullOrEmpty(row["Value"].ToString()) && string.IsNullOrEmpty(row["To Value"].ToString()))
                {
                    sb.Append(StringLiterals.TAB);
                    sb.Append("ELSE");
                    sb.Append(StringLiterals.SPACE);
                }
                else
                if (row["Value"].ToString() == "LOVALUE" || row["Value"].ToString() == "HIVALUE")
                {
                    sb.Append(StringLiterals.TAB);
                    sb.Append(row["Value"]);
                    sb.Append(StringLiterals.SPACE);
                }
                else

                if (double.TryParse(row["Value"].ToString(), out double_compare) || DateTime.TryParse(row["Value"].ToString(), out dateValue))
                {
                    sb.Append(StringLiterals.TAB);
                    sb.Append(row["Value"]);
                    sb.Append(StringLiterals.SPACE);
                }
                else
                {
                    
                    sb.Append(StringLiterals.TAB);
                    sb.Append(Util.InsertInDoubleQuotes(row["Value"].ToString()));
                    sb.Append(StringLiterals.SPACE);
                }
                
                if (row["To Value"] != DBNull.Value && !string.IsNullOrEmpty(row["To Value"].ToString()))
                {
                    sb.Append(StringLiterals.HYPHEN);
                    sb.Append(StringLiterals.SPACE);

                    if (row["To Value"] == "LOVALUE" || row["To Value"] == "HIVALUE")
                    {
                        //sb.Append(StringLiterals.TAB).Append(row["To Value"]).Append(StringLiterals.SPACE);
                        sb.Append(row["To Value"]);
                        sb.Append(StringLiterals.SPACE);
                    }
                    else
                        if (double.TryParse(row["To Value"].ToString(), out double_compare) || DateTime.TryParse(row["To Value"].ToString(), out dateValue))
                    {
                        sb.Append(row["To Value"]);
                        sb.Append(StringLiterals.SPACE);
                    }
                    else
                    {
                        sb.Append(Util.InsertInDoubleQuotes(row["To Value"].ToString()));
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.EQUAL);
                sb.Append(StringLiterals.SPACE);
                sb.Append(Util.InsertInDoubleQuotes(row["Recoded Value"].ToString()));
                sb.Append(Environment.NewLine);
            }
            sb.Append(CommandNames.END);
            CommandText = sb.ToString();
        }

        #endregion //Protected Methods

        #region Public Methods
        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            if (fillRanges)
            {
                btnSaveOnly.Enabled = false;
            }
            else
            {
                btnSaveOnly.Enabled = inputValid;
            }
        }
        
        #endregion Public Methods

        private void cmbFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void cmbTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void txtStart_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void txtEnd_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void txtBy_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        //private void btnOK_Click(object sender, EventArgs e)
        //{
        //   FillDataGrid();
        //   OnOK();  
        //}

    }
}

