using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using System.Data;
using Epi.Collections;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Fields;

namespace Epi.Windows.MakeView.Dialogs
{

    public partial class MatchFieldsDialog : DialogBase
    {
        #region Public Interface

        #region Constructors
        /// <summary>
        /// Default constructor - Design time only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public MatchFieldsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="project">Current project</param>
        /// <param name="tableName">Name of the table selected</param>
        /// <param name="selectedViewFields">Selected view fields</param>
        /// <param name="nonSelectedViewFields">Remaining view fields</param>
        /// <param name="selectedFields">Selected fields</param>
        public MatchFieldsDialog(MainForm frm, Project project, string tableName,  List<string> selectedFieldsToBeLinked, string textColumnName, Dictionary<string, string> fieldColumnNamePairs)
            : base(frm)
        {
            InitializeComponent();
            MINIMUM_VALID_INDEX = 0;
            UNSELECTED_INDEX = -1;

            if (project.CollectedData.TableExists(tableName))
            {
                codeTableFields = project.CollectedData.GetTextColumnNames(tableName);
            }
            else
            {
                string separator = " - ";

                if (tableName.Contains(separator))
                {
                    string[] view_page = tableName.Replace(separator, "^").Split('^');
                    string viewName = view_page[0].ToString();
                    string pageName = view_page[1].ToString();

                    View view = project.Metadata.GetViewByFullName(viewName);
                    DataTable pages = project.Metadata.GetPagesForView(view.Id);
                    string filterExpression = string.Format("Name = '{0}'", pageName);
                    DataRow[] pageArray = pages.Select(filterExpression);

                    if (pageArray.Length > 0)
                    {
                        int pageId = (int)pageArray[0]["PageId"];

                        tableName = viewName + pageId;
                        codeTableFields = project.CollectedData.GetTextColumnNames(tableName);

                        DataTable fields = project.Metadata.GetFieldsOnPageAsDataTable(pageId);
                        filterExpression = string.Format("FieldTypeId = {0}", 1);
                        DataRow[] fieldArray = fields.Select(filterExpression);
                        if (fieldArray.Length > 0 && codeTableFields != null)
                        {
                            foreach (DataRow row in fieldArray)
                            { 
                                DataRowView newRow = codeTableFields.AddNew();
                                newRow["TABLE_NAME"] = tableName;
                                newRow["COLUMN_NAME"] = row["Name"];
                                newRow.EndEdit();
                            }
                        }
                    }
                }
            }

            this.selectedFieldsToBeLinked = selectedFieldsToBeLinked;
            currentProject = project;
            currentTableName = tableName;
            cbxViewField.Enabled = false;
            cbxTableField.Enabled = false;
            LoadCodeFields();
            cbxSelectedTableInformation.SelectedItem = textColumnName;

            //  Form Fields [v]		Table Fields [v]
            //  cbxViewField        cbxTableField

            foreach (KeyValuePair<string, string> kvp in fieldColumnNamePairs)
            {
                string fieldName = kvp.Key;
                string columnName = kvp.Value;

                cbxViewField.Items.Remove(fieldName);
                cbxTableField.Items.Remove(columnName);

                string newAssociation = "";
                newAssociation = fieldName + " = " + columnName;
                lbxAssociatedLinkedFields.Items.Add(newAssociation);
            }

            btnOK.Enabled = lbxAssociatedLinkedFields.Items.Count == 0 ? false : true;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// The Codes collection
        /// </summary>
        public KeyValuePairCollection Codes
        {
            get
            {
                return (kvPairs);
            }
        }

        /// <summary>
        /// The data that will match with the newly created Codes field in the first column
        /// </summary>
        public string PrimaryColumnData
        {
            get
            {
                return (primaryColumnData);
            }
        }

        #endregion Public Properties

        #region Public Methods
        
        #endregion Public Methods

        #endregion Public Interface

        #region Protected Interface
        
        #region Protected Properties

        /// <summary>
        /// Data View containing data for codeTable
        /// </summary>
        protected DataView codeTableFields;

        /// <summary>
        /// A collection of strings that hold the associated link definitions
        /// </summary>
        protected Collection<string> associationInformation;

        /// <summary>
        /// A collection of a collection of strings which hold the collection of associated linked fields
        /// </summary>
        protected Collection<Collection<string>> collectionOfAssociations;

        #endregion Protected Properties

        #region Protected Methods
        
        #endregion Protected Methods

        #region Protected Events
        
        #endregion Protected Events

        #endregion Protected Interface

        #region Private Members

        #region Private Enums and Constants
        private int MINIMUM_VALID_INDEX;
        private int UNSELECTED_INDEX;
        #endregion Private Enums and Constants

        #region Private Properties
        private KeyValuePairCollection kvPairs;
        private List<string> selectedFieldsToBeLinked;
        private Project currentProject;
        private string currentTableName;
        private string previousSelectedTableField;
        private string primaryColumnData;
        #endregion Private Properties

        #region Private Methods

        private KeyValuePair ParseLinkedFields(Object item)
        {
            int INDEX_DELIMITER_OFFSET = 1;
            int indexOfEquals = -1;
            string firstValue;
            string lastValue;
            string beginningString = item.ToString();

            indexOfEquals = beginningString.IndexOf("=");
            firstValue = beginningString.Substring(0, (indexOfEquals - INDEX_DELIMITER_OFFSET));
            lastValue = beginningString.Substring((indexOfEquals + (2 * INDEX_DELIMITER_OFFSET)), (beginningString.Length - (indexOfEquals + (2 * INDEX_DELIMITER_OFFSET))));
            return (new KeyValuePair(firstValue, lastValue));
        }

        /// <summary>
        /// Load data into the Code Fields
        /// </summary>
        private void LoadCodeFields()
        {
            if (currentProject.CollectedData.TableExists(currentTableName) == false)
            {
                btnPreviewTable.Enabled = false; 
            }
            else
            {
                btnPreviewTable.Enabled = true; 
            }

            SetSelectedTableInformationDDL(codeTableFields);
            SetViewFieldsDDL();
            SetTableFieldsDDL(codeTableFields);
            btnOK.Enabled = lbxAssociatedLinkedFields.Items.Count == 0 ? false : true;
        }

        #region Called for LoadCodeFields()

      
        private void SetSelectedTableInformationDDL(DataView codeTableFields)
        {
            string columnName = string.Empty;
            foreach (DataRowView row in codeTableFields)
            {
                columnName = row[ColumnNames.COLUMN_NAME].ToString();
                if (!(columnName.Equals(ColumnNames.REC_STATUS)) && !(columnName.Equals(ColumnNames.UNIQUE_KEY)))
                {
                    cbxSelectedTableInformation.Items.Add(columnName);
                }
            }

            cbxSelectedTableInformation.SelectedIndex = UNSELECTED_INDEX;
            cbxSelectedTableInformation.SelectedIndexChanged += new EventHandler(cbxSelectedTableInformation_SelectedIndexChanged);
        }

        private void SetViewFieldsDDL()
        {
            if (selectedFieldsToBeLinked != null)
            {
                foreach (string name in selectedFieldsToBeLinked)
                {
                    cbxViewField.Items.Add(name);
                }
            }
        }

        private void SetTableFieldsDDL(DataView codeTableFields)
        {
            string columnName = string.Empty;
            foreach (DataRowView row in codeTableFields)
            {
                columnName = row[ColumnNames.COLUMN_NAME].ToString();
                if (!(columnName.Equals(ColumnNames.REC_STATUS)) && !(columnName.Equals(ColumnNames.UNIQUE_KEY)))
                {
                    cbxTableField.Items.Add(columnName);
                }
            }

            cbxTableField.SelectedIndex = UNSELECTED_INDEX;
        }

        #endregion

        private string CreateDisplayText(Collection<string> pair)
        {
            string displayText = String.Empty;

            foreach (string item in pair)
            {
                displayText = displayText + item.ToString() + " = ";
            }

            displayText = displayText.Substring(0, displayText.Length - 3);

            return displayText;
        }

        private void UpdateAbilityToLink()
        {
            if ((cbxViewField.Items.Count == 0) || (cbxTableField.Items.Count == 0))
            {
                btnLink.Enabled = false;
            }
            else
            {
                btnLink.Enabled = true;
            }
        }

        #endregion Private Methods

        #region Private Events

        /// <summary>
        /// Handles the click event of cancel
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void cbxSelectedTableInformation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxSelectedTableInformation.SelectedItem != null)
            {
                string selectedItemString = cbxSelectedTableInformation.SelectedItem.ToString();
                primaryColumnData = selectedItemString;

                cbxTableField.Enabled = true;
                cbxViewField.Enabled = true;

                //if you select a value that is already linked, unlink it upon selection
                if (lbxAssociatedLinkedFields.Items.Contains(selectedItemString))
                {
                    int indexOfItemToRemove = lbxAssociatedLinkedFields.FindStringExact(selectedItemString);
                    lbxAssociatedLinkedFields.Items.RemoveAt(indexOfItemToRemove);

                    //add to drop down
                    cbxTableField.Items.Add(selectedItemString);
                }

                if (previousSelectedTableField == null)
                {
                    //since this is the first time a selection has been made, set previous as current
                    previousSelectedTableField = selectedItemString;

                    //remove current selection from TableFields dropdown
                    //find the item selected in the initial dropdown and remove it
                    if (cbxTableField.Items.Contains(selectedItemString))
                    {
                        int indexOfItemToRemove = cbxTableField.FindStringExact(selectedItemString);
                        cbxTableField.Items.RemoveAt(indexOfItemToRemove);
                    }
                }
                else
                {
                    //add the previous selection back to the TableFields dropdown
                    cbxTableField.Items.Add(previousSelectedTableField);

                    //remove current selection from TableFields dropdown
                    //find the item selected in the initial dropdown and remove it
                    if (cbxTableField.Items.Contains(selectedItemString))
                    {
                        int indexOfItemToRemove = cbxTableField.FindStringExact(selectedItemString);
                        cbxTableField.Items.RemoveAt(indexOfItemToRemove);
                    }

                    //now that we are done processing the previous selection, set the cufrrent to previous
                    previousSelectedTableField = selectedItemString;
                }

                UpdateAbilityToLink();
            }
        }

        /// <summary>
        /// Handles DoubleClick event of table fields list box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxAssociatedLinkedFields_DoubleClick(object sender, System.EventArgs e)
        {
            btnUnlink_Click(sender, e);
        }

        /// <summary>
        /// Handles load event of the form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CodesUsingExistingTable_Load(object sender, System.EventArgs e)
        {
        //    LoadCodeFields();
        }

        /// <summary>
        /// Handles click event of OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            primaryColumnData = cbxSelectedTableInformation.SelectedItem.ToString();
            
            kvPairs = new KeyValuePairCollection();
            kvPairs.Delimiter = CharLiterals.SPACE;
            
            for (int index = 0; index < lbxAssociatedLinkedFields.Items.Count; index++)
            {
                kvPairs.Add(ParseLinkedFields(lbxAssociatedLinkedFields.Items[index].ToString()));
            }
            
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Handles the Click Event of the Unlink button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnUnlink_Click(object sender, EventArgs e)
        {
            int selectedItem = lbxAssociatedLinkedFields.SelectedIndex;

            if (selectedItem > -1)
            {
                string association = lbxAssociatedLinkedFields.SelectedItem.ToString();

                string viewFieldToAdd = String.Empty;
                string tableFieldToAdd = String.Empty;

                string[] associationArray = association.Split('=');

                viewFieldToAdd = associationArray[0].Trim();
                tableFieldToAdd = associationArray[1].Trim();

                if (string.IsNullOrEmpty(viewFieldToAdd) == false && selectedFieldsToBeLinked.Contains(viewFieldToAdd))
                {
                    cbxViewField.Items.Add(viewFieldToAdd);
                }

                if (string.IsNullOrEmpty(tableFieldToAdd) == false)
                { 
                    cbxTableField.Items.Add(tableFieldToAdd);
                }

                if (selectedItem >= MINIMUM_VALID_INDEX)
                {
                    lbxAssociatedLinkedFields.Items.RemoveAt(selectedItem);
                }

                UpdateAbilityToLink();
            }

            btnOK.Enabled = lbxAssociatedLinkedFields.Items.Count == 0 ? false : true;
        }

        /// <summary>
        /// Handles the Click Event of the Link button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnLink_Click(object sender, EventArgs e)
        {
            if ((cbxViewField.SelectedItem != null) && (cbxTableField.SelectedItem != null))
            {
                string newAssociation = "";
                newAssociation = cbxViewField.SelectedItem.ToString() + " = " + cbxTableField.SelectedItem.ToString();
                lbxAssociatedLinkedFields.Items.Add(newAssociation);

                cbxTableField.Items.RemoveAt(cbxTableField.SelectedIndex);
                cbxViewField.Items.RemoveAt(cbxViewField.SelectedIndex);

                btnOK.Enabled = true;
            }

            btnOK.Enabled = lbxAssociatedLinkedFields.Items.Count == 0 ? false : true;

            UpdateAbilityToLink();
        }

        /// <summary>
        /// Handles the Click Event of the Preview Table button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnPreviewTable_Click(object sender, EventArgs e)
        {
            PreviewTableDialog previewTableDialog = new PreviewTableDialog(this.MainForm, currentProject, cbxSelectedTableInformation.Items, currentTableName);
            DialogResult result = previewTableDialog.ShowDialog();
        }

        private void cbxTableField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxTableField.Items.Contains(cbxSelectedTableInformation.SelectedItem.ToString()))
            {
                int indexOfItemToRemove = cbxTableField.FindStringExact(cbxSelectedTableInformation.SelectedItem.ToString());
                cbxTableField.Items.RemoveAt(indexOfItemToRemove);
            }
            UpdateAbilityToLink();
        }
        #endregion Private Events

        #endregion Private Members

    }
}
