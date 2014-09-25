#region	Namespaces
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Epi;
using Epi.Fields;

#endregion	Namespaces

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Single Line Text Field Definition dialog
    /// </summary>
    public partial class SingleLineTextFieldDefinition : TextFieldDefinition
	{
        private SingleLineTextField field;

		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public SingleLineTextFieldDefinition()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public SingleLineTextFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">Page the field will belong to</param>
		public SingleLineTextFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="field">The field to be edited</param>
		public SingleLineTextFieldDefinition(MainForm frm, SingleLineTextField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}
		#endregion	//Constructors

		#region	Private Methods
		private void LoadFormData()
		{
            SetFontStyles(field);

			txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;            
			chkReadOnly.Checked = field.IsReadOnly;
			chkRepeatLast.Checked = field.ShouldRepeatLast;
			chkRequired.Checked = field.IsRequired;
			
            if (field.MaxLength > 0)
			{
                mtbSize.Text = field.MaxLength.ToString();
			}
		}
		#endregion	//Private Methods


        #region	Protected Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;

            SetMaxTextboxSize(field);

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
        }

        protected void SetMaxTextboxSize(TextField field)
        {
            if (!string.IsNullOrEmpty(mtbSize.Text))
            {
                int size;
                bool couldParse = int.TryParse(mtbSize.Text, out size);

                if (couldParse && size < 255)
                {
                    field.MaxLength = size;
                }
                else if (couldParse && size > 254)
                {
                    MessageBox.Show("The value entered in the Maximum Number of Characters field was too large (maximum) so it will be set to 254 charaters.", "", MessageBoxButtons.OK);
                    field.MaxLength = 254;
                }
            }
            else
            {
                field.MaxLength = 0;
            }
        }

		#endregion	//Protected Methods

		#region	Public Methods
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
	}
}
