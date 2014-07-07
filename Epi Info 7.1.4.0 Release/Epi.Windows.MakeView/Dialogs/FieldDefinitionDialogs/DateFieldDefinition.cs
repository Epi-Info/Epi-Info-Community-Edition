#region Namespaces

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Fields;

#endregion //Namespaces


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Data Field Definition dialog
    /// </summary>
    public partial class DateFieldDefinition : TextFieldDefinition
	{
		#region Fields
		private DateField field;
		#endregion	//Fields

		#region Private Methods

		private void LoadFormData()
		{
            SetFontStyles(field);

			txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;
			chkReadOnly.Checked = field.IsReadOnly;
			chkRepeatLast.Checked = field.ShouldRepeatLast;
            chkRequired.Checked = field.IsRequired;
            
            if (!string.IsNullOrEmpty(field.Lower))
            {
                if (field.LowerDate > lowerDatePicker.MinDate && field.LowerDate < lowerDatePicker.MaxDate)
                {
                    lowerDatePicker.Value = field.LowerDate;
                }
                else
                {
                    lowerDatePicker.Value = DateTime.Today;
                }
            }
            
            if (!string.IsNullOrEmpty(field.Upper))
            {
                if (field.UpperDate > upperDatePicker.MinDate && field.UpperDate < upperDatePicker.MaxDate)
                {
                    upperDatePicker.Value = field.UpperDate;
                }
                else
                {
                    upperDatePicker.Value = DateTime.Today;
                }
            }
            
            this.UseRange.Checked = ((field.Lower.Length + field.Upper.Length) != 0);
            
            int fieldType = (int)field.FieldType;

            EnableDatePickers(UseRange.Checked);
		}
		#endregion Private Methods	

		#region	Constructors
        
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public DateFieldDefinition()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">The current page</param>
        public DateFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="field">The fied to be edited</param>
        public DateFieldDefinition(MainForm frm, DateField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;

            LoadFormData();            
		}

		#endregion	//Constructors		

		#region	Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
         
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
            
            if (UseRange.Checked)
            {
                field.LowerDate = lowerDatePicker.Value;
                field.UpperDate = upperDatePicker.Value;
            }
            else
            {
                field.Lower = String.Empty;
                field.Upper = String.Empty;
            }
        }

        /// <summary>
        /// Validate Dialog Input
        /// </summary>
        /// <returns>bool</returns>
        protected override bool ValidateDialogInput()
        {
            bool isValid = true;
            
            if (UseRange.Checked)
            {
                DateTime upper;
                DateTime lower;
                
                if (!DateTime.TryParse(lowerDatePicker.Text, out lower))
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.INVALID_LOWER_DATE_RANGE);
                }
                
                if (!DateTime.TryParse(upperDatePicker.Text, out upper))
                {
                    isValid = false;
                    ErrorMessages.Add(SharedStrings.INVALID_UPPER_DATE_RANGE);
                }

                if (isValid)
                {
                    if (lower > upper)
                    {
                        isValid = false;
                        ErrorMessages.Add(SharedStrings.UPPER_DATE_RANGE_IS_LOWER);
                    }
                }
            }

            if (isValid)
            {
                return base.ValidateDialogInput();
            }
            else
            {
                ShowErrorMessages();
                return false;
            }
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
		#endregion	Public Methods

        private void UseRange_CheckedChanged(object sender, EventArgs e)
        {
            EnableDatePickers(((CheckBox)sender).Checked);
        }

        private void EnableDatePickers(bool enabled)
        {
            this.labelLowerRange.Enabled = enabled;
            this.lowerDatePicker.Enabled = enabled;

            this.labelUpperRange.Enabled = enabled;
            this.upperDatePicker.Enabled = enabled;
        }
	}
}
