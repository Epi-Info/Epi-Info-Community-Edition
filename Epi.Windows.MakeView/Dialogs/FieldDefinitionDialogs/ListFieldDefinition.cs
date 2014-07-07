using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Collections;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// Field definition dialog for codes fields
    /// </summary>
    public partial class ListFieldDefinition : LegalValuesFieldDefinition
    {
        #region Public Interface
        #region Constructors
        /// <summary>
        /// Default Constsructor for exclusive use by the designer
        /// </summary>
        public ListFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="page">The current page</param>
        public ListFieldDefinition(MainForm frm, Page page)
            : base(frm)
        {
            InitializeComponent();
            this.Text = "List Field";
            this.mode = FormMode.Create;
            this.page = page;
            selectedFields = new NamedObjectCollection<Field>();
            btnOk.Enabled = true;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="field">The fied to be edited</param>
        public ListFieldDefinition(MainForm frm, DDListField field)
            : base(frm)
        {
            InitializeComponent();
            this.Text = "List Field";
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            selectedFields = new NamedObjectCollection<Field>();
            
            LoadFormData();
            btnOk.Enabled = true;
        }

        #endregion Constructors

        #region Public Enums and Constants
        
        #endregion Public Enums and Constants

        #region Public Properties
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

       

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            //this.btnOK.Enabled = true;
        }
        #endregion Public Methods
        #endregion Public Interface

        #region Protected Interface
        
        #region Protected Properties
        
        #endregion Protected Properties

        #region Protected Methods
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
            else if (!string.IsNullOrEmpty(this.txtDataSource.Text))
            {
                int vals = this.txtDataSource.Text.IndexOf("::");
                if (vals < 0)
                {
                    field.SourceTableName = this.txtDataSource.Text;
                }
                else
                {
                    field.SourceTableName = this.txtDataSource.Text.Substring(0,vals).Trim();
                    field.TextColumnName = this.txtDataSource.Text.Substring(vals + 2,this.txtDataSource.Text.Length - vals - 2).Trim();
                }
            }


            //build a string of form    fieldId:fieldname, fieldId1:fieldname1
            /*
            StringBuilder sb = new StringBuilder();
            string items = lbxLinkedFields.SelectedItem.ToString();
            
            foreach (Field selectedField in selectedFields)
            {
                sb.Append(selectedField.Name.ToString() + ":" + selectedField.Id.ToString() + ",");
            }

            //remove final extra comma
            if (sb.Length > 0)
            {
                sb = sb.Remove(sb.Length - 1, 1);
            }*/

            field.AssociatedFieldInformation = ((DataRowView)lbxLinkedFields.SelectedItem)[0].ToString();
            field.AssociatedFieldInformation += ":" + field.GetView().Fields[((DataRowView)lbxLinkedFields.SelectedItem)[0].ToString()].Id.ToString(); 
            
           

        }


        protected override void txtPrompt_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
            {
                txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                btnDataSource.Enabled = true;
                //btnOk.Enabled = false;
            }
        }


        protected override void txtPrompt_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                    btnDataSource.Enabled = true;
                    //btnOk.Enabled = false;
                }
            }
        }


        protected override void txtFieldName_Leave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnDataSource.Enabled = true;
                //btnOk.Enabled = string.IsNullOrEmpty(txtDataSource.Text) ? false : true;
            }
        }

        #endregion Protected Methods

        #region Protected Events
        /// <summary>
        /// Handles the click event for the "..." data source button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void btnDataSource_Click(object sender, EventArgs e)
        {
            if (ValidateToAddDataSource())
            {
                NamedObjectCollection<Field> columnNamesInLower = new NamedObjectCollection<Field>();
                int selectedIndex = 1;
                DataRowView item;
                string[] selectedViewFields = new string[lbxLinkedFields.SelectedItems.Count + 1];
                selectedViewFields[0] = field.Name;
                for (int i = 0; i < lbxLinkedFields.Items.Count; i++)
                {
                    item = (DataRowView)lbxLinkedFields.Items[i];
                    if (lbxLinkedFields.GetSelected(i))
                    {
                        selectedViewFields[selectedIndex] = item[lbxLinkedFields.DisplayMember].ToString();
                        DataRow selectRow = item.Row;
                        string fieldColumnName = (selectRow[ColumnNames.NAME].ToString());
                        string fieldStringID = (selectRow[ColumnNames.FIELD_ID].ToString());
                        if (DoesFieldNameExistInCollection(fieldColumnName, selectedFields) == false)
                        {
                            selectedFields.Add(page.GetView().GetFieldById(int.Parse(fieldStringID)));
                        }
                        selectedIndex++;
                    }
                }

                ListDialog codesDialog = new ListDialog((TableBasedDropDownField)this.Field, this.MainForm, txtFieldName.Text, this.page, selectedFields);
                DialogResult result = codesDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (!((string.IsNullOrEmpty(codesDialog.SourceTableName) && string.IsNullOrEmpty(codesDialog.TextColumnName))))
                    {
                        txtDataSource.Text = codesDialog.SourceTableName + " :: " + codesDialog.TextColumnName;
                        lbxLinkedFields.Enabled = false;
                        lblSelectFields.Enabled = false;
                        txtFieldName.Enabled = false;
                    }
                    else
                    {
                        //if code table has not been set - set these to empty
                        txtDataSource.Text = string.Empty;
                        field.SourceTableName = string.Empty;
                        field.TextColumnName = string.Empty;
                        lbxLinkedFields.Enabled = true;
                        lblSelectFields.Enabled = true;
                    }

                    this.sourceTableName = codesDialog.SourceTableName;
                    this.textColumnName = codesDialog.TextColumnName;
                    btnOk.Enabled = true;
                }
                btnOk.Enabled = true;
            }
            else
            {
                ShowErrorMessages();
            }
        }
        #endregion Protected Events

        #endregion Protected Interface

        #region Private Members

        #region Private Enums and Constants
        
        #endregion Private Enums and Constants

        #region Private Properties
        private new DDListField field;
        private new string sourceTableName;
        private new string textColumnName;
        private NamedObjectCollection<Field> selectedFields;
        private List<string> selectedIndex;
        private string fieldName = string.Empty;
       
        #endregion Private Properties

        #region Private Methods
        /// <summary>
        /// Load the form with the saved data
        /// </summary>
        private new void LoadFormData()
        {

            Configuration config = Configuration.GetNewInstance();
            FontStyle style = FontStyle.Regular;
            if (config.Settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }
            field.PromptFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);

            style = FontStyle.Regular;
            if (config.Settings.ControlFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.ControlFontItalics)
            {
                style |= FontStyle.Italic;
            }
            field.ControlFont = new Font(config.Settings.ControlFontName, (float)config.Settings.ControlFontSize, style);

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
            promptFont = field.PromptFont;
            controlFont = field.ControlFont;
            if (!(String.IsNullOrEmpty(txtPrompt.Text)))
            {
                btnDataSource.Enabled = true;
            }
            else
            {
                btnDataSource.Enabled = false;
            }

            Epi.Data.IDbDriver db = Epi.Data.DBReadExecute.GetDataDriver(page.view.Project.CollectedDataConnectionString);
            Epi.Data.Query query = db.CreateQuery("select [Name], [FieldId] " +
                    "from metaFields " +
                    "where [PageId] = @pageID and [FieldTypeId] in (1,3,4,17, 18, 19, 27) " +
                    " and [ViewId] = @viewID " +
                    "order by [Name]");
            query.Parameters.Add(new Epi.Data.QueryParameter("@pageID", DbType.Int32, page.Id));
            query.Parameters.Add(new Epi.Data.QueryParameter("@viewID", DbType.Int32, page.view.Id));
            
            //DataTable textFields = page.GetMetadata().GetTextFieldsForPage(page.GetView().Id, page.Id);
            DataTable textFields = db.Select(query);
            string expression = "Name = '" + txtFieldName.Text + "'";
            string sortOrder = "Name DESC";
            DataRow[] rows = textFields.Select(expression, sortOrder);
            if (rows.Length > 0) textFields.Rows.Remove(rows[0]);
            lbxLinkedFields.DataSource = textFields;
            lbxLinkedFields.DisplayMember = ColumnNames.NAME;

            if (!(string.IsNullOrEmpty(txtDataSource.Text)))
            {
                selectedIndex = page.GetProject().CollectedData.GetTableColumnNames(field.SourceTableName);

                String textFieldName = string.Empty;

                for (int i = 0; i < textFields.Rows.Count; i++)
                {
                    textFieldName = textFields.Rows[i][ColumnNames.NAME].ToString();

                    if (selectedIndex.Contains(textFieldName))
                    {
                        lbxLinkedFields.SetSelected(lbxLinkedFields.FindStringExact(textFieldName), true);
                    }
                    else
                    {
                        lbxLinkedFields.SetSelected(lbxLinkedFields.FindStringExact(textFieldName), false);
                    }
                }
                lbxLinkedFields.Enabled = true;
                lblSelectFields.Enabled = true;
                lbxLinkedFields.Visible = true;
                lblSelectFields.Visible = true;
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }

        private bool DoesFieldNameExistInCollection(string fieldname, NamedObjectCollection<Field> collection)
        {
            bool matchFound = false;
            foreach (string name in collection.Names)
            {
                if (fieldname.Equals(name))
                {
                    matchFound = true;
                }
            }
            return matchFound;
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            bool isValid = ValidateToAddDataSource();

            if (isValid)
            {
                if (string.IsNullOrEmpty(txtDataSource.Text))
                {
                    ErrorMessages.Add("Select a datasource");
                }
            }

            return (ErrorMessages.Count == 0);
        }

        private bool ValidateToAddDataSource()
        {
            if (lbxLinkedFields.Items.Count == 0)
            {
                ErrorMessages.Add(SharedStrings.WARNING_MUST_CREATE_TEXT_FIRST);
                return false;
            }
            else if (lbxLinkedFields.SelectedItems.Count == 0)
            {
                ErrorMessages.Add(SharedStrings.NO_LINKED_FIELD_SELECTED);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Click event for OK button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        protected void btnOk_Click(object sender, System.EventArgs e)
        {
            if (ValidateInput())
            {
                SetFieldProperties();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                ShowErrorMessages();
            }
        }

        #endregion Private Methods

        #region Private Events
        private void lstLinkedFields_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        
        #endregion Private Events


        #endregion Private Members
    }
}


