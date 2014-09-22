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
    /// Field definition dialog for legal values
    /// </summary>
    public partial class LegalValuesFieldDefinition : Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs.TextFieldDefinition
    {
        /// <summary>
        /// Field you are working on
        /// </summary>
        protected DDLFieldOfLegalValues field;

        /// <summary>
        /// The table name in the database
        /// </summary>
        protected string sourceTableName;

        /// <summary>
        /// The column name in the database
        /// </summary>
        protected string textColumnName;

        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public LegalValuesFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Legal Values Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
		public LegalValuesFieldDefinition(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Legal Values Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">The page</param>
		public LegalValuesFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}
        
        /// <summary>
        /// Constructor for the Legal Values Field Definition dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The legal values field</param>
		public LegalValuesFieldDefinition(MainForm frm, DDLFieldOfLegalValues field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}

        /// <summary>
        /// Load the information into the form
        /// </summary>
        protected void LoadFormData()
        {
            SetFontStyles(field);

            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;
            if (!string.IsNullOrEmpty(field.SourceTableName))
            {
                this.sourceTableName = field.SourceTableName;
                this.textColumnName = field.TextColumnName;
                txtDataSource.Text = field.SourceTableName + " :: " + field.TextColumnName;
            }
            chkReadOnly.Checked = field.IsReadOnly;
            chkRepeatLast.Checked = field.ShouldRepeatLast;
            chkRequired.Checked = field.IsRequired;
            
            if (string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = false;
            }
            else
            {
                btnDataSource.Enabled = true;
            }

            // special case when changing a field type from one type to this type - don't allow
            // the OK button to be pressed until there's a data source.
            if (string.IsNullOrEmpty(txtDataSource.Text))
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }

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
            if (!string.IsNullOrEmpty(this.sourceTableName) && !string.IsNullOrEmpty(this.textColumnName))
            {
                field.SourceTableName = this.sourceTableName;
                field.TextColumnName = this.textColumnName;
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

        public override void txtFieldName_TextChanged(object sender, System.EventArgs e)
        {
            base.txtFieldName_TextChanged(sender, e);
            btnDataSource.Enabled = btnOk.Enabled;
        }

        /// <summary>
        /// Event from clicking on the Data Source "..." button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void btnDataSource_Click(object sender, EventArgs e)
        {
            LegalValuesDialog sourceTableDialog = new LegalValuesDialog((TableBasedDropDownField)this.Field, this.MainForm, txtFieldName.Text, this.page);
            DialogResult result = sourceTableDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(sourceTableDialog.SourceTableName) && !string.IsNullOrEmpty(sourceTableDialog.TextColumnName))
                {
                    txtDataSource.Text = sourceTableDialog.SourceTableName + " :: " + sourceTableDialog.TextColumnName;
                }
                else if (string.IsNullOrEmpty(sourceTableDialog.SourceTableName) && string.IsNullOrEmpty(sourceTableDialog.TextColumnName))
                {
                    txtDataSource.Text = string.Empty;
                    field.SourceTableName = string.Empty;
                    field.TextColumnName = string.Empty;
                }
                else
                {
                    txtDataSource.Text = sourceTableDialog.SourceTableName + " :: " + sourceTableDialog.TextColumnName;
                }
                this.sourceTableName = sourceTableDialog.SourceTableName;
                this.textColumnName = sourceTableDialog.TextColumnName;

                btnOk.Enabled = true;
            }
        }

        protected virtual void txtFieldName_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = true;
                btnOk.Enabled = true;
            }
            else
            {
                btnDataSource.Enabled = false;
                btnOk.Enabled = false;
            }
        }

        protected override void txtPrompt_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
            {
                txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
            }

            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = true;
                btnOk.Enabled = true;
            }
            else
            {
                btnDataSource.Enabled = false;
                btnOk.Enabled = false;
            }
        }

        protected override void txtPrompt_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                    
                    if (!string.IsNullOrEmpty(txtFieldName.Text))
                    {
                        btnDataSource.Enabled = true;
                        btnOk.Enabled = true;
                    }
                    else
                    {
                        btnDataSource.Enabled = false;
                        btnOk.Enabled = false;
                    }
                }
            }
        }
        /// <summary>
        /// The view
        /// </summary>
        public virtual View View
        {
            set {/* Do Nothing */}
        }
    }
}

