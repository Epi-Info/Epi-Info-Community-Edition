using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Epi.Data.Services;
using Epi.Fields;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Number Field Definition dialog
    /// </summary>
    public partial class NumberFieldDefinition : ContiguousFieldDefinition
    {
        #region	Fields
        private NumberField field;
        #endregion	//Fields

        #region	Private Methods
         
        private void PopulatePatterns()
        {
            System.Data.DataView patterns = (System.Data.DataView)AppData.Instance.DataPatternsDataTable.DefaultView;

            //System.Data.DataView patterns = page.GetMetadata().GetPatterns().DefaultView;
            patterns.RowFilter = "DataTypeId = " + ((int)DataType.Number).ToString();
            cbxPattern.DataSource = patterns;
            cbxPattern.DisplayMember = Epi.ColumnNames.EXPRESSION;
            cbxPattern.ValueMember = Epi.ColumnNames.PATTERN_ID;
        }

        private void LoadFormData()
        {
            SetFontStyles(field);

            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;
            chkReadOnly.Checked = field.IsReadOnly;
            chkRepeatLast.Checked = field.ShouldRepeatLast;
            chkRequired.Checked = field.IsRequired;

            int fieldType = (int)field.FieldType;
            
            if (!string.IsNullOrEmpty(field.Pattern) )
            {
                cbxPattern.Text = field.Pattern;
            }
            else
            {   //set a default number pattern
                //cbxPattern.Text = "##";
                //cbxPattern.Text = AppData.Instance.DataPatternsDataTable.GetDefaultPattern(AppData.Instance.FieldTypesDataTable.GetPatternIdByFieldId(fieldType));
                cbxPattern.Text = string.Empty;
            }
            
            // Enabled Range values
            if (!string.IsNullOrEmpty(field.Lower))
            {
                txtNumberLower.Text = field.Lower;
            }
            if (!string.IsNullOrEmpty(field.Upper))
            {
                txtNumberUpper.Text = field.Upper;
            }
            
            cbxRange.Checked = ((field.Lower.Length + field.Upper.Length) != 0);
        }
        #endregion	//Private Methods

        #region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public NumberFieldDefinition()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for the Number Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">The current page</param>
        public NumberFieldDefinition(MainForm frm, Page page)
            : base(frm)
        {
            InitializeComponent();
            Construct();
            this.mode = FormMode.Create;
            this.page = page;
            PopulatePatterns();
            cbxPattern.SelectedIndex = -1;
        }

        /// <summary>
        /// Constructor for the Number Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The number field</param>
        public NumberFieldDefinition(MainForm frm, NumberField field)
            : base(frm)
        {
            InitializeComponent();
            Construct();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            PopulatePatterns();
            cbxPattern.SelectedIndex = -1;
            LoadFormData();
        }

        private void Construct()
        {
            txtNumberUpper.Enabled = false;
            txtNumberLower.Enabled = false;

        }
        #endregion Constructors


        #region	Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;

            if (!string.IsNullOrEmpty(cbxPattern.Text))
            {                
               field.Pattern = cbxPattern.Text;               
            }
            else
            {
                field.Pattern = string.Empty;
            }
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
            if (controlFont != null)
            {
                field.ControlFont = controlFont;
            }
            
            field.IsRequired = chkRequired.Checked;
            field.IsReadOnly = chkReadOnly.Checked;
            field.ShouldRepeatLast = chkRepeatLast.Checked;

            if (cbxRange.Checked)
            {
                field.Lower = txtNumberLower.Text;
                field.Upper = txtNumberUpper.Text;
            }
            else
            {
                field.Lower = String.Empty;
                field.Upper = String.Empty;
            }
            //cbxRange.Checked = ((field.Lower.Length + field.Upper.Length) > 0);

        }

        /// <summary>
        /// Gets the field defined by this field definition dialog
        /// </summary>
        public override RenderableField Field
        {
            get
            {
                return field;
            }
        }
        #endregion	//Public Methods

        /// <summary>
        /// Validates the pattern combo box for numeric fields.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        protected override void cbxPattern_Validating(object sender, CancelEventArgs e)
        {
            base.cbxPattern_Validating(sender, e);

            // validate pattern text, should only ever contain ## and . in numeric fields (E. Knudsen 4/4/2011)
            bool valid = true;

            if (cbxPattern.Text.Length != 0)
            {
                int periods = 0;

                for (int i = 0; i < cbxPattern.Text.Length; i++)
                {
                    string viewChar = cbxPattern.Text.Substring(i, 1);
                    System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(viewChar, "[#.]");
                    // if the project name does not consist of only letters and numbers...
                    if (!m.Success)
                    {
                        // we found an invalid character; invalidate the pattern
                        valid = false;
                        break; // stop the for loop here, no point in continuing
                    }
                    if (viewChar == ".")
                    {
                        periods++;
                    }
                    if (periods > 1)
                    {
                        valid = false;
                        break;
                    }
                }

                if (!cbxPattern.Text.StartsWith("#") || !cbxPattern.Text.EndsWith("#"))
                {
                    if (cbxPattern.Text.StartsWith("N") || cbxPattern.Text.EndsWith("e"))
                        valid = true;
                    else
                        valid = false;
                }
            }
            else
            {
                valid = true;
            }

            if (!valid)            
            {
                e.Cancel = true;                
                MsgBox.ShowError(SharedStrings.INVALID_NUMERIC_PATTERN);
            }
        }

        private void RangeValue_Keypress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '.')
            {
                int isNumber = 0;
                e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
            }
        }

        private void RangeValue_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(((Control)sender).Text))
            {
                string mask = cbxPattern.Text;
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

        /// <summary>
        /// Handles the change event of the Range selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        protected override void cbxRange_CheckedChanged(object sender, System.EventArgs e)
        {
            lblLower.Enabled = cbxRange.Checked;
            lblUpper.Enabled = cbxRange.Checked;
            txtNumberLower.Enabled = cbxRange.Checked;
            txtNumberUpper.Enabled = cbxRange.Checked;
            if (!cbxRange.Checked)
            {
                txtNumberLower.Text = string.Empty;
                txtNumberUpper.Text = string.Empty;
            }
        }

        protected override void btnOk_Click(object sender, System.EventArgs e)
        {
            ErrorMessages.Clear();
            bool isValid = true;
            
            if (cbxRange.Checked)
            {
                double lower;
                double upper;

                bool lowerIsNumber = Double.TryParse(txtNumberLower.Text, out lower);
                bool upperIsNumber = Double.TryParse(txtNumberUpper.Text, out upper);

                if (string.IsNullOrEmpty(txtNumberLower.Text) == false && lowerIsNumber == false)
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.NOT_A_NUMBER);
                }

                if (string.IsNullOrEmpty(txtNumberUpper.Text) == false && upperIsNumber == false)
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.NOT_A_NUMBER);
                }

                if (lowerIsNumber && upperIsNumber)
                {
                    if (lower > upper)
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.LOWER_RANGE_GREATER_THAN_UPPER_RANGE);
                    }
                }

                if (string.IsNullOrEmpty(cbxPattern.Text) == false && cbxPattern.Text != "None")
                {
                    string patternMaxText = cbxPattern.Text.Replace('#', '9');
                    double patternMax = double.PositiveInfinity;
                    upperIsNumber = Double.TryParse(patternMaxText, out patternMax);

                    string patternMinText = cbxPattern.Text.Replace('#', '9');
                    double patternMin = double.NegativeInfinity;
                    lowerIsNumber = Double.TryParse(patternMinText, out patternMin);

                    if (upperIsNumber)
                    {
                        if (patternMax < upper)
                        {
                            isValid = false;
                            ErrorMessages.Add(SharedStrings.PATTERN_RESTRICTS_ENTRY_OF_UPPER_RANGE);
                        }
                    }

                    if (lowerIsNumber)
                    {
                        if (lower < 0)
                        {
                            patternMin = patternMax * -1;
                            if (patternMin > lower)
                            {
                                isValid = false;
                                ErrorMessages.Add(SharedStrings.PATTERN_RESTRICTS_ENTRY_OF_LOWER_RANGE);
                            }
                        }
                        else
                        {
                            if (patternMin < lower)
                            {
                                isValid = false;
                                ErrorMessages.Add(SharedStrings.PATTERN_RESTRICTS_ENTRY_OF_LOWER_RANGE);
                            }
                        }
                    }
                }
            }

            if (isValid == true && base.ValidateDialogInput())
            {
                SetFieldProperties();
                page.GetView().MustRefreshFieldCollection = true;
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                ShowErrorMessages();
            }
        }
    }
}
