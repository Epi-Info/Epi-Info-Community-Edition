    using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using Epi;
using Epi.Data;
using Epi.Data.Services;
using Epi.Windows;
using Epi.Fields;
using Epi.Windows.Dialogs;
using System.Text.RegularExpressions;


namespace Epi.Windows.MakeView.Dialogs
{
    public partial class CommentLegalDialog : LegalValuesDialog
    {
        #region Public Interface

        #region Constructors

        /// <summary>
        /// Default Constructor - Design Mode only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CommentLegalDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the Comment Legal dialog
        /// </summary>
        /// <param name="field">The field associated with this comment legal</param>
        /// <param name="frm">The main form</param>
        /// <param name="name">The name of the field</param>
        /// <param name="currentPage">The current page</param>
        public CommentLegalDialog(TableBasedDropDownField field, MainForm frm, string name, Page currentPage)
            : base(field, frm, name, currentPage)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            //dgCodes.CaptionText = "Comment Legal values for: " + name;
        }

        /// <summary>
        /// Constructor for the Comment Legal dialog
        /// </summary>
        /// <param name="column">The column associated with this comment legal grid column</param>
        /// <param name="frm">The main form</param>
        /// <param name="name">The name of the column</param>
        /// <param name="currentPage">The current page</param>
        public CommentLegalDialog(TableBasedDropDownColumn column, MainForm frm, string name, Page currentPage)
            : base(column, frm, name, currentPage)
        {
            InitializeComponent();
            fieldName = name;
            page = currentPage;
            //dgCodes.CaptionText = "Comment Legal values for: " + name;
        }

        /// <summary>
        /// Constructor for the Comment Legal dialog
        /// </summary>
        /// <param name="field">The comment legal field</param>
        /// <param name="currentPage">The current page</param>
        public CommentLegalDialog(RenderableField field, Page currentPage)
            : base(field, currentPage)
        {
            InitializeComponent();
            page = currentPage;          
            DDLFieldOfCommentLegal ddlField = (DDLFieldOfCommentLegal)field;
            codeTable = ddlField.GetSourceData();
            //if (dgCodes.AllowSorting)
            //{
            //    ddlField.ShouldSort = true;
            //}
            //else
            //{
            //    ddlField.ShouldSort = false;
            //}

            dgCodes.DataSource = codeTable;
            sourceTableName = codeTable.TableName;
            textColumnName = fieldName;
            //dgCodes.AllowSorting = true;
        }
        #endregion Constructors

        #region Public Enums and Constants
        
        #endregion Public Enums and Constants

        #region Public Properties

        /// <summary>
        /// Returns the source's table name
        /// </summary>
        public new string SourceTableName
        {
            get
            {
                return (sourceTableName);
            }
        }

        /// <summary>
        /// Returns the text column name
        /// </summary>
        public new string TextColumnName
        {
            get
            {
                return (textColumnName);
            }
        }

        #endregion Public Properties

        #region Public Methods
        
        #endregion Public Methods

        #endregion Public Interface

        #region Protected Interface
        
        #region Protected Properties
        
        #endregion Protected Properties

        #region Protected Methods

        /// <summary>
        /// Show the field selected for a Use Existing codetable
        /// </summary>
        /// <param name="tableName">Table name</param>
        protected override void ShowFieldSelection(string tableName)
        {
            string separator = " - ";
            if (!tableName.Contains(separator))
            {
                FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName);
                DialogResult result = fieldSelection.ShowDialog();
                if (result == DialogResult.OK)
                {
                    textColumnName = fieldSelection.ColumnName;
                    sourceTableName = tableName;
                    codeTable = GetProject().GetTableData(tableName, textColumnName);
                    fieldSelection.Close();
                    DisplayData();
                    isExclusiveTable = true;
                }
            }
            else if(DdlField!=null) //using Form table as Datasource
            {
                string[] view_page = sourceTableName.Replace(separator, "^").Split('^');
                string viewName = view_page[0].ToString();
                string pageName = view_page[1].ToString();
                string filterExpression = string.Empty;
                string tableName1 = null;
                View targetView = page.GetProject().Metadata.GetViewByFullName(viewName);

                if (targetView != null)
                {
                    DataTable targetPages = page.GetProject().Metadata.GetPagesForView(targetView.Id);
                    DataView dataView = new DataView(targetPages);

                    filterExpression = string.Format("Name = '{0}'", pageName);

                    DataRow[] pageArray = targetPages.Select(filterExpression);

                    if (pageArray.Length > 0)
                    {
                        int pageId = (int)pageArray[0]["PageId"];
                        tableName1 = viewName + pageId;
                    }
                }
                if (page.GetProject().CollectedData.TableExists(tableName1))
                {
                    FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName1);
                    DialogResult result = fieldSelection.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        textColumnName = fieldSelection.ColumnName;
                        codeTable = page.GetProject().GetTableData(tableName1, textColumnName, string.Empty);
                        fieldSelection.Close();
                        DisplayData();
                        isExclusiveTable = true;
                    }
                }
            }
            else
            {
                FieldSelectionDialog fieldSelection = new FieldSelectionDialog(MainForm, page.GetProject(), tableName);
                DialogResult result = fieldSelection.ShowDialog();
                if (result == DialogResult.OK)
                {
                    textColumnName = fieldSelection.ColumnName;
                    sourceTableName = tableName;
                    codeTable = page.GetProject().GetTableData(tableName, textColumnName, string.Empty);
                    fieldSelection.Close();
                    DisplayData();
                    isExclusiveTable = true;
                }
            }
        }

        #endregion Protected Methods

        #region Protected Events

        /// <summary>
        /// Event from when the Create New button is clicked
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void btnCreate_Click(object sender, System.EventArgs e)
        {
            CreateCommentLegal();
            creationMode = CreationMode.CreateNew;
            btnCreate.Enabled = false;
            btnFromExisting.Enabled = false; 
            btnUseExisting.Enabled = false;
            dgCodes.Visible = true;
            btnOK.Enabled = true;
        }

        /// <summary>
        /// Event from clicking the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void btnOK_Click(object sender, System.EventArgs e)
        {
            if ((DataTable)dgCodes.DataSource != null)
            {
                ((DataTable)dgCodes.DataSource).AcceptChanges();
            }

            SaveShouldSort();

            if (CheckForHyphens())
            {
                SaveCodeTableToField();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
        }

        #endregion Protected Events
        #endregion Protected Interface

        #region Private Members

        #region Private Enums and Constants
        
        #endregion Private Enums and Constants

        #region Private Properties
        private string fieldName = string.Empty;
        private DataTable codeTable;
        private Page page;
        private int oldCurrentRow;
        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Set ShouldSort based on if the checkbox is checked
        /// </summary>
        private void SaveShouldSort()
        {
            if (DdlField != null)
            {
                DdlField.ShouldSort = !cbxSort.Checked;
            }
        }

        /// <summary>
        /// Verify hyphens exist for Comment Legal field
        /// </summary>
        private bool CheckForHyphens()
        {
            bool isValidated;
            Regex commentLegalCheck = new Regex(@"^[0-9a-zA-Z]+(\s)*(-){1}");

            codeTable = (DataTable)dgCodes.DataSource;

            if (codeTable != null && string.IsNullOrEmpty(textColumnName))
            {
                textColumnName = codeTable.Columns[0].ColumnName;
            }

            if (codeTable != null)
            {
                isValidated = true;
                foreach (DataRow row in codeTable.Rows)
                {
                    #region Validation
                    if (textColumnName == null)
                    {
                        throw new ArgumentNullException("textColumnName");
                    }
                    #endregion Input validation

                    if (((string.IsNullOrEmpty(textColumnName)) || !(commentLegalCheck.IsMatch(row[textColumnName].ToString()))))
                    {
                        string msg = SharedStrings.SEPARATE_COMMENT_LEGAL_WITH_HYPEN + ": \n" + row[textColumnName].ToString();
                        MsgBox.ShowError(msg);
                        isValidated = false;
                    }

                    if (isValidated == false)
                    {
                        break;
                    }
                }
            }
            else
            {
                isValidated = true;
            }

            return isValidated;
        }

        /// <summary>
        /// Get the current project
        /// </summary>
        /// <returns>Current project</returns>
        private Project GetProject()
        {
            return page.GetProject();
        }

        /// <summary>
        /// Set up comment legal
        /// </summary>
        private void CreateCommentLegal()
        {
            dgCodes.Visible = true;
            btnOK.Visible = true;

            DataTable bindingTable = page.GetProject().CodeData.GetCodeTableNamesForProject(page.GetProject());
            DataView dataView = bindingTable.DefaultView;
            //if (dgCodes.AllowSorting)
            {
                dataView.Sort = GetDisplayString(page);
            }
            string cleanedCodeTableName = CleanCodeTableName(fieldName, dataView);

            page.GetProject().CreateCodeTable(cleanedCodeTableName, fieldName.ToLower());
            codeTable = page.GetProject().GetTableData(cleanedCodeTableName);
            codeTable.TableName = cleanedCodeTableName;
            dgCodes.DataSource = codeTable;

            sourceTableName = codeTable.TableName;
            textColumnName = fieldName;
            dgCodes.Focus(); // Fix to focus the cursor in the data grid when 'Create New' is pressed. Really, this needs to be combined with the Legal Value dialog.
        }

        //Show the data
        private void DisplayData()
        {
            if (codeTable != null)
            {
                dgCodes.Visible = true;
                btnOK.Enabled = true;
                btnOK.Visible = true;

                btnCreate.Enabled = false;
                btnFromExisting.Enabled = false;
                btnUseExisting.Enabled = false;

                btnDelete.Enabled = false;
                btnDelete.Visible = true;

                codeTable.TableName = sourceTableName;
                dgCodes.DataSource = codeTable;
                //dgCodes.CaptionText = sourceTableName;

                if (DdlField != null) cbxSort.Checked = !DdlField.ShouldSort;
            }
        }

        #endregion Private Methods

        #region Private Events

        /// <summary>
        /// Handles the Load event from Comment Legal
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CommentLegal_Load(object sender, System.EventArgs e)
        {
            DisplayData();
            dgCodes.Focus();
        }

        #endregion Private Events

        #endregion Private Members
    }
}
