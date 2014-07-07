#region namespaces

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Epi;
using Epi.Fields;

#endregion namespaces


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Checkbox Field Definition dialog
    /// </summary>
    public partial class CheckBoxFieldDefinition : TextFieldDefinition
	{
		#region Fields
	
		private CheckBoxField field;

		#endregion	//Fields

		#region Constructors

        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public CheckBoxFieldDefinition()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">Page the field will belong to</param>
        public CheckBoxFieldDefinition(MainForm frm, Page page) : base(frm)
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
        public CheckBoxFieldDefinition(MainForm frm, CheckBoxField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}
		#endregion Constructors

		#region Private Methods
		private void LoadFormData()
		{
            SetFontStyles(field);
            
            txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;
			chkReadOnly.Checked = field.IsReadOnly;
			chkRepeatLast.Checked = field.ShouldRepeatLast;
			promptFont = field.PromptFont;
            checkBoxPlaceBoxOnRight.Checked = field.BoxOnRight;
		}
		#endregion	//Private Methods
		
		#region Public Methods

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
            field.IsReadOnly = chkReadOnly.Checked;
            field.ShouldRepeatLast = chkRepeatLast.Checked;
            field.BoxOnRight = checkBoxPlaceBoxOnRight.Checked;
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
	}
}
