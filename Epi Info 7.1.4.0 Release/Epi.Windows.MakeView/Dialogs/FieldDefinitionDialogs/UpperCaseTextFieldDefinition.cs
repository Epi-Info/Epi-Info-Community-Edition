
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Fields;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{

    public partial class UpperCaseTextFieldDefinition : SingleLineTextFieldDefinition 
	{

        private UpperCaseTextField field;

		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public UpperCaseTextFieldDefinition()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Constructor
		/// </summary>        
		/// <param name="frm">The main form</param>
		public UpperCaseTextFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">Page the field will belong to</param>
		public UpperCaseTextFieldDefinition(MainForm frm, Page page) : base(frm)
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
		public UpperCaseTextFieldDefinition(MainForm frm, UpperCaseTextField field) : base(frm)
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

		#region	Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
            
            base.SetMaxTextboxSize(field);

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
