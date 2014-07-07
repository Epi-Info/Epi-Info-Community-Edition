#region	Namespaces

using Epi;
using Epi.Fields;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

#endregion	Namespaces

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The GUID Field Definition dialog
    /// </summary>
    public partial class GUIDFieldDefinition : TextFieldDefinition
    {
        #region Private Members

        private GUIDField field;

        #endregion

        #region	Constructors

        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public GUIDFieldDefinition()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public GUIDFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">Page the field will belong to</param>
		public GUIDFieldDefinition(MainForm frm, Page page) : base(frm)
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
		public GUIDFieldDefinition(MainForm frm, GUIDField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}

		#endregion

		#region	Private Methods

		private void LoadFormData()
		{
            SetFontStyles(field);

			txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;
			chkReadOnly.Checked = field.IsReadOnly;
			chkRepeatLast.Checked = field.ShouldRepeatLast;
			chkRequired.Checked = field.IsRequired;
		}

		#endregion	

		#region	Protected Methods

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
            field.ShouldRepeatLast = chkRepeatLast.Checked;
        }
		
		#endregion

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

		#endregion	

        #region Event Handlers

        /// <summary>
        /// Set control state after the form has loaded
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void GUIDFieldDefinition_Load(object sender, EventArgs e)
        {
            //GUID's are always read Only and Required
            chkReadOnly.Checked = true;
            chkReadOnly.Enabled = false;

            chkRequired.Checked = true;
            chkRequired.Enabled = false;

            chkRepeatLast.Checked = false;
            chkRepeatLast.Enabled = false;
        }

        #endregion
    }
}
