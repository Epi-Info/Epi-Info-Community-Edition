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
    /// Field definition dialog for comment legal fields
    /// </summary>
    public partial class CommentLegalFieldDefinition : Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs.LegalValuesFieldDefinition
    {
        private new DDLFieldOfCommentLegal field;
        private new string sourceTableName;
        private new string textColumnName;
        //private bool isCodeTableProcessed = false;

        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public CommentLegalFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="page">The current page</param>
        public CommentLegalFieldDefinition(MainForm frm, Page page)
            : base(frm)
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
        public CommentLegalFieldDefinition(MainForm frm, DDLFieldOfCommentLegal field)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            LoadFormData();
        }

        /// <summary>
        /// Load the form with the saved data
        /// </summary>
        private new void LoadFormData()
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
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
            if (controlFont != null)
            {
                field.ControlFont = controlFont;
            }
            field.Name = txtFieldName.Text;
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

        /// <summary>
        /// Handles the click event of the Datasource "..." button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void btnDataSource_Click(object sender, EventArgs e)
        {
            CommentLegalDialog commentLegalValuesDialog = new CommentLegalDialog((TableBasedDropDownField)this.Field, this.MainForm, txtFieldName.Text, this.page);
            DialogResult result = commentLegalValuesDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!((string.IsNullOrEmpty(commentLegalValuesDialog.SourceTableName) && string.IsNullOrEmpty(commentLegalValuesDialog.TextColumnName)))) 
                {
                    txtDataSource.Text = commentLegalValuesDialog.SourceTableName + " :: " + commentLegalValuesDialog.TextColumnName;
                }
                else
                {
                    txtDataSource.Text = string.Empty;
                    field.SourceTableName = string.Empty;
                    field.TextColumnName = string.Empty;
                }

                this.sourceTableName = commentLegalValuesDialog.SourceTableName;
                this.textColumnName = commentLegalValuesDialog.TextColumnName; 

                btnOk.Enabled = true;
            }
        }

        protected virtual void txtFieldName_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = true;
                btnOk.Enabled = string.IsNullOrEmpty(txtDataSource.Text) ? false : true;
            }
        }

        protected override void txtPrompt_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
            {
                txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                btnDataSource.Enabled = true;
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
                    btnDataSource.Enabled = false;
                    btnOk.Enabled = false;
                }
                else
                {
                    btnDataSource.Enabled = btnOk.Enabled = true;
                }
            }
        }

        protected void txtFieldName_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = btnOk.Enabled = false;
            }
            else
            {
                btnDataSource.Enabled = btnOk.Enabled = true;
            }
        }
    }
}

