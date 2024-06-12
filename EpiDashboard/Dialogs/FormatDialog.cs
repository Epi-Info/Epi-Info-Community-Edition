using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EpiDashboard;
using Epi;
using Epi.Core;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Dialogs;
using EpiDashboard.Rules;
using static EpiDashboard.Mapping.ChoroplethLayerProvider;

namespace EpiDashboard.Dialogs
{
    public partial class FormatDialog : DialogBase
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private Rule_Format formatRule;
        private bool editMode;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor used for creating a new format rule
        /// </summary>
        public FormatDialog(DashboardHelper dashboardHelper)
        {
            InEditMode = false;
            this.dashboardHelper = dashboardHelper;
            InitializeComponent();
            FillSelectionComboboxes();
        }

        /// <summary>
        /// Constructor used for editing an existing format rule
        /// </summary>
        public FormatDialog(DashboardHelper dashboardHelper, Rule_Format formatRule)
        {
            InEditMode = true;
            this.dashboardHelper = dashboardHelper;
            this.formatRule = formatRule;            
            InitializeComponent();
            FillSelectionComboboxes();

            this.txtDestinationField.Text = formatRule.DestinationColumnName;
            this.txtDestinationField.Enabled = false;

            this.cbxFieldName.Enabled = false;
        }
        #endregion // Constructors

        /// <summary>
        /// Gets the format rule
        /// </summary>
        public Rule_Format FormatRule
        {
            get
            {
                return this.formatRule;
            }
            private set
            {
                this.formatRule = value;
            }
        }

        /// <summary>
        /// Gets the View associated with the attached dashboard helper
        /// </summary>
        private Epi.View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        /// <summary>
        /// Gets whether or not the rule is being edited
        /// </summary>
        private bool InEditMode
        {
            get
            {
                return this.editMode;
            }
            set
            {
                this.editMode = value;
            }
        }

        /// <summary>
        /// Fills in the Field Names combo box
        /// </summary>
        private void FillSelectionComboboxes()
        {
            cbxFieldName.Items.Clear();

            List<string> fieldNames = new List<string>();

            //if (dashboardHelper.IsUsingEpiProject)
            //{
            //    foreach (Field f in this.View.Fields)
            //    {
            //        if (f is DateTimeField || f is DateField /* || f is NumberField*/)
            //        {
            //            fieldNames.Add(f.Name);
            //        }
            //    }
            //}
            //else
            //{
                ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
                fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);
            //}

            fieldNames.Sort();
            cbxFieldName.DataSource = fieldNames;

            if (InEditMode)
            {
                cbxFieldName.SelectedItem = FormatRule.SourceColumnName;
            }
        }

        /// <summary>
        /// Gets the appropriate FormatType based on the selected format option
        /// </summary>
        /// <returns>FormatType</returns>
        private FormatTypes GetFormatType()
        {
            #region Input Validation
            if (cbxFormatOptions.SelectedItem == null)
            {
                throw new ApplicationException("No value selected for the format option.");
            }
            #endregion // Input Validation

            FormatTypes formatType = FormatTypes.Day;

            string option = cbxFormatOptions.SelectedItem.ToString();

            switch (option)
            {
                case "an integer":
                    formatType = FormatTypes.NumericInteger;
                    break;
                case "a decimal with one digit":
                    formatType = FormatTypes.NumericDecimal1;
                    break;
                case "a decimal with two digits":
                    formatType = FormatTypes.NumericDecimal2;
                    break;
                case "a decimal with three digits":
                    formatType = FormatTypes.NumericDecimal3;
                    break;
                case "a decimal with four digits":
                    formatType = FormatTypes.NumericDecimal4;
                    break;
                case "a decimal with five digits":
                    formatType = FormatTypes.NumericDecimal5;
                    break;
                case "the day":
                    formatType = FormatTypes.Day;
                    break;
                case "the numeric day":
                    formatType = FormatTypes.NumericDay;
                    break;
                case "the day name":
                    formatType = FormatTypes.FullDayName;
                    break;
                case "the abbreviated day name":
                    formatType = FormatTypes.ShortDayName;
                    break;
                case "the month":
                    formatType = FormatTypes.Month;
                    break;
                case "the numeric month":
                    formatType = FormatTypes.NumericMonth;
                    break;
                case "the month and four-digit year":
                    formatType = FormatTypes.MonthAndFourDigitYear;
                    break;
                case "the month name":
                    formatType = FormatTypes.FullMonthName;
                    break;
                case "the abbreviated month name":
                    formatType = FormatTypes.ShortMonthName;
                    break;
                case "the standard date":
                    formatType = FormatTypes.RegularDate;
                    break;
                case "the long date":
                    formatType = FormatTypes.LongDate;
                    break;
                case "the epi week":
                    formatType = FormatTypes.EpiWeek;
                    break;
                case "the four-digit year":
                    formatType = FormatTypes.FourDigitYear;
                    break;
                case "the two-digit year":
                    formatType = FormatTypes.TwoDigitYear;
                    break;
                case "the numeric year":
                    formatType = FormatTypes.NumericYear;
                    break;
                case "the RFC 1123 date":
                    formatType = FormatTypes.RFC1123;
                    break;
                case "the sortable date":
                    formatType = FormatTypes.SortableDateTime;
                    break;
                case "the hour":
                    formatType = FormatTypes.Hours;
                    break;
            }

            return formatType;
        }

        private void FillDateFormatOptions()
        {
            cbxFormatOptions.Items.Clear();
            cbxFormatOptions.Items.Add("the day");
            cbxFormatOptions.Items.Add("the numeric day");
            cbxFormatOptions.Items.Add("the day name");
            cbxFormatOptions.Items.Add("the abbreviated day name");
            cbxFormatOptions.Items.Add("the month");
            cbxFormatOptions.Items.Add("the numeric month");
            cbxFormatOptions.Items.Add("the month and four-digit year");
            cbxFormatOptions.Items.Add("the month name");
            cbxFormatOptions.Items.Add("the abbreviated month name");
            cbxFormatOptions.Items.Add("the standard date");
            cbxFormatOptions.Items.Add("the long date");
            cbxFormatOptions.Items.Add("the epi week");
            cbxFormatOptions.Items.Add("the four-digit year");
            cbxFormatOptions.Items.Add("the two-digit year");
            cbxFormatOptions.Items.Add("the numeric year");
            cbxFormatOptions.Items.Add("the hour");
            cbxFormatOptions.Items.Add("the RFC 1123 date");
            cbxFormatOptions.Items.Add("the sortable date");
        }

        private void FillNumberFormatOptions()
        {
            cbxFormatOptions.Items.Clear();
            cbxFormatOptions.Items.Add("an integer");
            cbxFormatOptions.Items.Add("a decimal with one digit");
            cbxFormatOptions.Items.Add("a decimal with two digits");
            cbxFormatOptions.Items.Add("a decimal with three digits");
            cbxFormatOptions.Items.Add("a decimal with four digits");
            cbxFormatOptions.Items.Add("a decimal with five digits");
        }

        private void cbxFieldName_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbxFormatOptions.Items.Clear();

            Configuration config = dashboardHelper.Config;
            
            if (dashboardHelper.IsColumnNumeric(cbxFieldName.SelectedItem.ToString()))
            {
                FillNumberFormatOptions();
            }
            else if (dashboardHelper.IsColumnDateTime(cbxFieldName.SelectedItem.ToString()))
            {
                FillDateFormatOptions();
            }

            if (InEditMode)
            {
                cbxFormatOptions.SelectedItem = FormatRule.FormatString;
            }
            else
            {
                txtDestinationField.Text = cbxFieldName.SelectedItem.ToString() + "_FORMATTED";
            }
        }

        private void cbxFormatOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxFormatOptions.SelectedIndex >= 0)
            {
                FormatTypes type = GetFormatType();

                if (type != FormatTypes.EpiWeek)
                {
                    Rule_Format tempFormatRule = new Rule_Format(dashboardHelper, "temp", "temp", "temp", "temp", type);
                    string formatString = tempFormatRule.GetFormatString();
                    txtPreview.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, formatString, DateTime.Now);
                    if ((type == FormatTypes.NumericDay || type == FormatTypes.NumericMonth) && txtPreview.Text[0] == '0')
                        txtPreview.Text = txtPreview.Text.Trim('0');
                }
                else
                {
                    StatisticsRepository.EpiWeek epiWeek = new StatisticsRepository.EpiWeek();                                        
                    txtPreview.Text = epiWeek.GetEpiWeek(DateTime.Now).ToString().Trim();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDestinationField.Text))
            {
                MsgBox.ShowError("Destination field is blank.");
                this.DialogResult = DialogResult.None;
                return;
            }
            else if (cbxFormatOptions.SelectedIndex < 0)
            {
                MsgBox.ShowError("No format options selected.");
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!editMode)
            {
                foreach (string s in dashboardHelper.GetFieldsAsList())
                {
                    if (txtDestinationField.Text.ToLowerInvariant().Equals(s.ToLowerInvariant()))
                    {
                        MsgBox.ShowError("Destination field name already exists as a column in this data set. Please use another name.");
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }

                foreach (IDashboardRule rule in dashboardHelper.Rules)
                {
                    if (rule is DataAssignmentRule)
                    {
                        DataAssignmentRule assignmentRule = rule as DataAssignmentRule;
                        if (txtDestinationField.Text.ToLowerInvariant().Equals(assignmentRule.DestinationColumnName.ToLowerInvariant()))
                        {
                            MsgBox.ShowError("Destination field name already exists as a defined field with recoded values. Please use another field name.");
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                    }
                }
            }

            FormatRule = new Rule_Format(this.dashboardHelper, "Format the display of " + cbxFieldName.SelectedItem.ToString() + " to show " + cbxFormatOptions.SelectedItem.ToString() + " and place the formatted values in " + txtDestinationField.Text, cbxFieldName.SelectedItem.ToString(), txtDestinationField.Text, cbxFormatOptions.SelectedItem.ToString(), GetFormatType());
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
