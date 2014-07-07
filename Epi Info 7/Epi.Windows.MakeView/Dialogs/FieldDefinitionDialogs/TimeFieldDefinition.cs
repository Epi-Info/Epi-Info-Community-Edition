#region Namespaces

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi.Data.Services;
using Epi.Fields;

#endregion //Namespaces

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Time Field Definition dialog
    /// </summary>
    public partial class TimeFieldDefinition : PatternableTextFieldDefinition
	{
		#region	Fields
		private TimeField field;
		#endregion	//Fields

		#region	Private Methods
		private void PopulatePatterns()
		{
			//System.Data.DataView patterns = page.GetMetadata().GetPatterns().DefaultView;
            System.Data.DataView patterns = (System.Data.DataView)AppData.Instance.DataPatternsDataTable.DefaultView;
			patterns.RowFilter = "DataTypeId = " + ((int) DataType.Time).ToString();
			cbxPattern.DataSource = patterns;
			cbxPattern.DisplayMember = Epi.ColumnNames.EXPRESSION;
			cbxPattern.ValueMember = Epi.ColumnNames.PATTERN_ID;
            cbxPattern.DropDownStyle = ComboBoxStyle.DropDownList;
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
            
		}
		#endregion	//Private Methods

		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public TimeFieldDefinition()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">The current page</param>
		public TimeFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
			PopulatePatterns();
			cbxPattern.SelectedIndex = -1;
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="field">The fied to be edited</param>
		public TimeFieldDefinition(MainForm frm, TimeField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			PopulatePatterns();
            cbxPattern.SelectedIndex = -1;
			LoadFormData();
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

        private void txtFieldName_TextChanged(object sender, EventArgs e)
        {

        }


        
	}
}
