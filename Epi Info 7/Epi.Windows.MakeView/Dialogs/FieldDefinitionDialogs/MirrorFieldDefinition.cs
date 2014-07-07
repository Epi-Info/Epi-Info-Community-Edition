using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{

    /// <summary>
    /// Field definition dialog for mirror fields
    /// </summary>
    public partial class MirrorFieldDefinition : GenericFieldDefinition
    {
        private MirrorField field;

        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public MirrorFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Mirror Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">The page</param>
		public MirrorFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
            PopulateVariables();
			cbxAssignedVariable.SelectedIndex = -1;
            SetEvents();
        }
        
        /// <summary>
        /// Constructor for Mirror Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The mirror field</param>
        public MirrorFieldDefinition(MainForm frm, MirrorField field)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            PopulateVariables();
            cbxAssignedVariable.SelectedIndex = -1;
            cbxAssignedVariable.DisplayMember = "Name";
            LoadFormData();
            SetEvents();
        }

        private void SetEvents()
        {
            this.cbxAssignedVariable.SelectedIndexChanged += new EventHandler(OnInputChange);
            this.txtFieldName.TextChanged += new EventHandler(OnInputChange);
            this.txtPrompt.TextChanged += new EventHandler(OnInputChange);
        }

        private void PopulateVariables()
        {
            foreach (IDataField field in this.page.GetView().Fields.DataFields)
            {
                if (!(  field.FieldType == MetaFieldType.Checkbox       
                    //||  field.FieldType == MetaFieldType.Codes          
                    ||  field.FieldType == MetaFieldType.CommandButton  
                    //||  field.FieldType == MetaFieldType.CommentLegal   
                    ||  field.FieldType == MetaFieldType.ForeignKey
                    ||  field.FieldType == MetaFieldType.Grid
                    ||  field.FieldType == MetaFieldType.Group
                    ||  field.FieldType == MetaFieldType.Image
                    //||  field.FieldType == MetaFieldType.LegalValues
                    ||  field.FieldType == MetaFieldType.Mirror
                    ||  field.FieldType == MetaFieldType.Multiline
                    ||  field.FieldType == MetaFieldType.Option
                    ||  field.FieldType == MetaFieldType.RecStatus
                    ||  field.FieldType == MetaFieldType.Relate
                    //||  field.FieldType == MetaFieldType.UniqueKey
                    ||  field.FieldType == MetaFieldType.YesNo
                    ))
                {
                    cbxAssignedVariable.Items.Add(field);
                }
            }
        }

        private void LoadFormData()
        {
            if (field.FieldType != MetaFieldType.Grid && field.FieldType != MetaFieldType.Option)
            {
                SetFontStyles(field);
            }
            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;
            if (field.SourceField == null)
            {
                cbxAssignedVariable.SelectedIndex = -1;
            }
            else if (field.SourceFieldId > 0)
            {
                cbxAssignedVariable.SelectedItem = field.SourceField;
            }
            promptFont = field.PromptFont;
            controlFont = field.ControlFont;
        }

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
            if (!string.IsNullOrEmpty(cbxAssignedVariable.Text))
            {
                field.SourceFieldId = ((Field)cbxAssignedVariable.SelectedItem).Id;
            }
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
            if (controlFont != null)
            {
                field.ControlFont = controlFont;
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

        private void OnInputChange(object sender, EventArgs e)
        {
            btnOk.Enabled = (
                !string.IsNullOrEmpty(this.cbxAssignedVariable.Text) &&
                !string.IsNullOrEmpty(this.txtFieldName.Text) &&
                !string.IsNullOrEmpty(this.txtPrompt.Text));
        }
    }
}

