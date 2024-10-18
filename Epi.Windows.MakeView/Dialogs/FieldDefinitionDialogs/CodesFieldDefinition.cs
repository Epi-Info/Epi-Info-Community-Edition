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
    public partial class CodesFieldDefinition : LegalValuesFieldDefinition
    {
        #region Public Interface
        #region Constructors
        /// <summary>
        /// Default Constsructor for exclusive use by the designer
        /// </summary>
        public CodesFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="page">The current page</param>
        public CodesFieldDefinition(MainForm frm, Page page)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Create;
            this.page = page;
            selectedFields = new NamedObjectCollection<Field>();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="field">The fied to be edited</param>
        public CodesFieldDefinition(MainForm frm, DDLFieldOfCodes field)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            selectedFields = new NamedObjectCollection<Field>();
            LoadFormData();
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

            if (string.IsNullOrEmpty(field.AssociatedFieldInformation))
            {
                StringBuilder sb = new StringBuilder();
                string items = lbxLinkedFields.SelectedItems.ToString();

                foreach (Field selectedField in selectedFields)
                {
                    sb.Append(selectedField.Name.ToString() + ":" + selectedField.Id.ToString() + ",");
                }

                if (sb.Length > 0)
                {
                    sb = sb.Remove(sb.Length - 1, 1);
                }

                if (sb != null && sb.Length > 0)
                {
                    field.AssociatedFieldInformation = sb.ToString();
                }

                if (string.IsNullOrEmpty(field.AssociatedFieldInformation))
                {
                    field.AssociatedFieldInformation = field.RelateConditionString;
                }    
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
                string[] selectedFieldNames = new string[lbxLinkedFields.SelectedItems.Count];

                for (int i = 0; i < lbxLinkedFields.Items.Count; i++)
                {
                    item = (DataRowView)lbxLinkedFields.Items[i];
                    if (lbxLinkedFields.GetSelected(i))
                    {
                        selectedFieldNames[selectedIndex - 1] = item[lbxLinkedFields.DisplayMember].ToString();
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

                CodesDialog codesDialog = new CodesDialog((TableBasedDropDownField)this.Field, this.MainForm, txtFieldName.Text, this.page, selectedFields);

                DialogResult result = codesDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (!((string.IsNullOrEmpty(codesDialog.SourceTableName) && string.IsNullOrEmpty(codesDialog.TextColumnName))))
                    {
                        txtDataSource.Text = codesDialog.SourceTableName + " :: " + codesDialog.TextColumnName;
                        lbxLinkedFields.Enabled = true;
                        lblSelectFields.Enabled = true;
                        txtFieldName.Enabled = false;

                        string dialogRelateCondition = codesDialog.relateCondition;
                        if (string.IsNullOrEmpty(dialogRelateCondition))
                        {
                            ((DDLFieldOfCodes)field).AssociatedFieldInformation = ((DDLFieldOfCodes)field).RelateConditionString;
                        }
                        else
                        {
                            ((DDLFieldOfCodes)field).AssociatedFieldInformation = dialogRelateCondition;
                        }

                        ((DDLFieldOfCodes)field).ShouldSort = codesDialog.ShouldSort;
                    }
                    else
                    {
                        txtDataSource.Text = string.Empty;
                        field.SourceTableName = string.Empty;
                        field.TextColumnName = string.Empty;

                        lbxLinkedFields.Enabled = true;
                        lbxLinkedFields.Visible = true;
                        lblSelectFields.Enabled = true;
                        lblSelectFields.Visible = true;

                        ((DDLFieldOfCodes)field).AssociatedFieldInformation = string.Empty;
                    }

                    this.sourceTableName = codesDialog.SourceTableName;
                    this.textColumnName = codesDialog.TextColumnName;
                    btnOk.Enabled = true;
                }
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
        private new DDLFieldOfCodes field;
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

            if (!(String.IsNullOrEmpty(txtPrompt.Text)))
            {
                btnDataSource.Enabled = true;
            }
            else
            {
                btnDataSource.Enabled = false;
            }

            DataTable fields = page.GetMetadata().GetCodeTargetCandidates(page.Id);
            string expression = string.Format("FieldTypeId = 1 OR FieldTypeId = 2 OR FieldTypeId = 17 OR FieldTypeId = 18");
            
            lbxLinkedFields.DataSource = fields;
            lbxLinkedFields.DisplayMember = ColumnNames.NAME;
            lbxLinkedFields.SelectedItem = null;

            string fieldName = string.Empty;
            int fieldId;

            DataRow[] codeFieldRows = fields.Select(string.Format("{0} = '{1}'", ColumnNames.NAME, field.Name));

            if (codeFieldRows != null && codeFieldRows.Length > 0  && codeFieldRows[0] != null)
            {
                fields.Rows.Remove(codeFieldRows[0]);
                fields.AcceptChanges();
            }

            for (int i = 0; i < fields.Rows.Count; i++)
            {
                fieldId = (int)fields.Rows[i][ColumnNames.FIELD_ID];
                fieldName = (string)fields.Rows[i][ColumnNames.NAME]; 
                bool isSelected = field.PairAssociated.ContainsValue(fieldId);
                lbxLinkedFields.SetSelected(lbxLinkedFields.FindStringExact(fieldName), isSelected);
            }

            if (!(string.IsNullOrEmpty(txtDataSource.Text)))
            {
                lbxLinkedFields.Enabled = true;
                lblSelectFields.Enabled = true;
                lbxLinkedFields.Visible = true;
                lblSelectFields.Visible = true;
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
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
            bool isValid = true; // ValidateToAddDataSource();
            return (ErrorMessages.Count == 0);
        }

        private bool ValidateToAddDataSource()
        {
            if (lbxLinkedFields.Items.Count == 0)
            {
                ErrorMessages.Add(SharedStrings.WARNING_MUST_CREATE_TEXT_FIRST);
                return false;
            }
            else if (lbxLinkedFields.SelectedItems.Count == 0 && string.IsNullOrEmpty(txtDataSource.Text))
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
        protected override void btnOk_Click(object sender, System.EventArgs e)
        {
            ErrorMessages.Clear();
            //if (ValidateInput()) && ValidateToAddDataSource())
            if (ValidateDialogInput())
            {
                this.SetFieldProperties();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                this.ShowErrorMessages();
            }
        }

        #endregion Private Methods

        #endregion Private Members
    }
}


