#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi.Data.Services;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// ***** This is a wireframe and currently contains no functionality *****
    /// </summary>
    public partial class TableDefinitionDialog : DialogBase
    {
        #region Private Data Members
        private View currentView;
        private Int32 startingId;
        private string dataTableName;
        private string UNIQUEID_DEFAULT = "1";

        #endregion  //Private Data Members

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public TableDefinitionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor of the Table Definition dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public TableDefinitionDialog(MainForm frm)
            : base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Constructor of the Table Definition dialog
        /// </summary>
        /// <param name="view">The current view of the dialog</param>
        public TableDefinitionDialog(View view)
        {
            InitializeComponent();
            currentView = view;
        }

        #endregion  Constructors

        #region Event Handlers

        /// <summary>
        /// Cancel button closes this dialog 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the text changed event of the data table name
        /// Enables/Disables OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtDataTableName_TextChanged(object sender, EventArgs e)
        {
            EnableDisableOk();
        }

        /// <summary>
        /// Handles the text changed event of the Unique Id
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtUniqueId_TextChanged(object sender, EventArgs e)
        {
            EnableDisableOk();
        }

        /// <summary>
        /// Calls the validating method of the unique ID
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        private void txtUniqueId_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUniqueId.Text.Trim()))
            {
                if(ValidateStartingId() == false)
                {
                    //ValidateStartingId() handles error messages
                }
            }
        }

        /// <summary>
        /// Validates the data table name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtDataTableName_Leave(object sender, EventArgs e)
        {
            /*
            bool valid = true;
            if (!string.IsNullOrEmpty(txtDataTableName.Text))
            {
                Match nameMatch = Regex.Match(txtDataTableName.Text.Substring(0, 1), "[0-9]");
                if (nameMatch.Success)
                {
                    MsgBox.ShowError(SharedStrings.PROJECT_NAME_BEGIN_NUMERIC);
                    txtDataTableName.Clear();
                    valid = false;
                }
                else if (currentView.Project.CollectedData.TableExists(txtDataTableName.Text.Trim()))
                {
                    MsgBox.ShowError(SharedStrings.INVALID_DATA_TABLE_NAME);
                    txtDataTableName.Clear();
                    valid = false;
                }
                else
                {
                    for (int i = 0; i < txtDataTableName.Text.Trim().Length - 1; i++)
                    {
                        string dataTableChar = txtDataTableName.Text.Trim().Substring(i, 1);
                        Match m = Regex.Match(dataTableChar, "[ A-Za-z0-9_]");
                        if (!m.Success)
                        {
                            string newDataTableName = Util.RemoveNonAlphaNumericExceptSpaces(txtDataTableName.Text.Trim());
                            if (!string.IsNullOrEmpty(newDataTableName))
                            {
                                //Ensure altered data table name is not a reserved word before assigning it.
                                int nPos = newDataTableName.IndexOf(" ");
                                if (nPos >= 0 || AppData.Instance.IsReservedWord(newDataTableName))
                                {
                                    MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                                    txtDataTableName.Clear();
                                    valid = false;
                                    return;
                                }
                                else
                                {
                                    if (newDataTableName.Trim().Length > 64)
                                    {
                                        MsgBox.ShowError(SharedStrings.DATATABLE_NAME_TOO_LONG_AND_INVALID_CHARACTERS);
                                        txtDataTableName.Text = string.Empty;
                                        valid = false;
                                        return;
                                    }
                                    else
                                    {
                                        MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME + SharedStrings.RENAME_DATA_TABLE_NAME + StringLiterals.SPACE + newDataTableName);
                                        txtDataTableName.Text = Util.RemoveNonAlphaNumericExceptSpaces(newDataTableName);
                                        valid = false;
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                                valid = false;
                                txtDataTableName.Text = string.Empty;
                                return;
                            }
                        }
                    }
                }

                if (valid)
                {
                    int nPos = txtDataTableName.Text.IndexOf(" ");
                    if (nPos >= 0 || AppData.Instance.IsReservedWord(txtDataTableName.Text.Trim()))
                    {
                        MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                        txtDataTableName.Clear();
                    }
                    else
                    {
                        if (txtDataTableName.Text.Trim().Length > 64)
                        {
                            MsgBox.ShowError(SharedStrings.DATA_TABLE_NAME_TOO_LONG);
                            txtDataTableName.Text = string.Empty;
                        }
                    }
                }
                txtDataTableName.Text = txtDataTableName.Text.Trim();
                dataTableName = txtDataTableName.Text;
            }
            */ 
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            txtDataTableName.Text = txtDataTableName.Text.Trim();

            if (ValidateFieldsBeforeSave() == true)
            {
                DataTableName = txtDataTableName.Text;
                this.Close();                
            }
            else //don't close and allow user to go back to dialog
            {
                txtDataTableName.Focus();
                this.DialogResult = DialogResult.None;
            }            
        }

        /// <summary>
        /// Handles the Load event of the Table Definition Dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void TableDefinitionDialog_Load(object sender, EventArgs e)
        {
            //fix defect 252

            //set initial Data Table name to view name 
            if (!string.IsNullOrEmpty(currentView.Name))
            {
                txtDataTableName.Text = currentView.Name;
            }

            //set initial Unique ID to defaulted initial value
            if (!string.IsNullOrEmpty(UNIQUEID_DEFAULT))
            {
                txtUniqueId.Text = UNIQUEID_DEFAULT;
            }
        }

        #endregion  //Event Handlers

        #region Private Methods

        /// <summary>
        /// Enables or disables OK button
        /// </summary>
        private void EnableDisableOk()
        {
            btnOK.Enabled = (!string.IsNullOrEmpty(txtDataTableName.Text.Trim()) && !string.IsNullOrEmpty(txtUniqueId.Text.Trim()));
        }

        /// <summary>
        /// Validates the starting index.  Its value must be numeric.
        /// </summary>
        private bool ValidateStartingId()
        {
            bool valid = true;

            string uniqueId = txtUniqueId.Text.Trim();
            Match m = Regex.Match(uniqueId, "[0-9]");

            if (!m.Success)
            {
                MsgBox.Show(SharedStrings.INVALID_STARTING_UNIQUE_ID, "Invalid Unique Id", MessageBoxButtons.OK);
                valid = false;
                txtUniqueId.Clear();
                txtUniqueId.Focus();
                return valid;
            }

            Int32 maxValue = (Int32)(Int32.MaxValue - Int16.MaxValue);
            Int32 givenValue = 1;

            bool couldParse = int.TryParse(txtUniqueId.Text.Trim(), out givenValue);

            if (false == couldParse)
            {
                valid = false;
                txtUniqueId.Clear();
                txtUniqueId.Focus();
                return valid;
            }

            if (givenValue > maxValue)
            {
                MsgBox.Show(SharedStrings.INVALID_STARTING_UNIQUE_ID_TOO_LONG, "Invalid Unique Id", MessageBoxButtons.OK);
                valid = false;
                txtUniqueId.Focus();
                return valid;
            }

            if (valid)
            {
                startingId = int.Parse(txtUniqueId.Text.Trim());
            }
            return valid;
        }

        /// <summary>
        /// Validate fields before saving, do not need to give fields focus
        /// </summary>
        private bool ValidateFieldsBeforeSave()
        {
            //Both the DataTableName and StartingID need to be valid before saving
            if (ValidateDataTableName() == true) 
            {
                if (ValidateStartingId() == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }            
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Validates Data Table Name entered in dialog
        /// </summary>
        /// <returns>Boolean isValid</returns>
        private bool ValidateDataTableName()
        {
            bool valid = true;
            if (!string.IsNullOrEmpty(txtDataTableName.Text))
            {
                Match nameMatch = Regex.Match(txtDataTableName.Text.Substring(0, 1), "[0-9]");
                Match underscoreMatch = Regex.Match(txtDataTableName.Text.Substring(0, 1), "[_]");
                if (nameMatch.Success)
                {
                    MsgBox.ShowError(SharedStrings.DATA_TABLE_NAME_BEGIN_NUMERIC);
                    txtDataTableName.Clear();
                    valid = false;
                }
                else if (underscoreMatch.Success)
                {
                    MsgBox.ShowError(SharedStrings.DATA_TABLE_NAME_BEGIN_UNDERSCORE);
                    txtDataTableName.Clear();
                    valid = false;
                }
                else if (currentView.Project.CollectedData.TableExists(txtDataTableName.Text.Trim()))
                {
                    MsgBox.ShowError(string.Format(SharedStrings.DATA_TABLE_NAME_ALREADY_EXISTS, txtDataTableName.Text.Trim()));
                    txtDataTableName.Clear();
                    valid = false;
                }
                else
                {
                    for (int i = 0; i < txtDataTableName.Text.Trim().Length - 1; i++)
                    {
                        string dataTableChar = txtDataTableName.Text.Trim().Substring(i, 1);
                        Match m = Regex.Match(dataTableChar, "[ A-Za-z0-9_]");
                        if (!m.Success)
                        {
                            string newDataTableName = Util.RemoveNonAlphaNumericExceptSpaces(txtDataTableName.Text.Trim());
                            if (!string.IsNullOrEmpty(newDataTableName))
                            {
                                //Ensure altered data table name is not a reserved word before assigning it.
                                int nPos = newDataTableName.IndexOf(" ");
                                if (nPos >= 0 || AppData.Instance.IsReservedWord(newDataTableName))
                                {
                                    MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                                    txtDataTableName.Clear();
                                    valid = false;
                                    return valid;
                                }
                                else
                                {
                                    if (newDataTableName.Trim().Length > 64)
                                    {
                                        MsgBox.ShowError(SharedStrings.DATATABLE_NAME_TOO_LONG_AND_INVALID_CHARACTERS);
                                        txtDataTableName.Text = string.Empty;
                                        valid = false;
                                        return valid;
                                    }
                                    else
                                    {
                                        MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME + SharedStrings.RENAME_DATA_TABLE_NAME + StringLiterals.SPACE + newDataTableName);
                                        txtDataTableName.Text = Util.RemoveNonAlphaNumericCharacters(newDataTableName);                                        
                                        valid = false;
                                        return valid;
                                    }
                                }
                            }
                            else
                            {
                                MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                                valid = false;
                                txtDataTableName.Text = string.Empty;
                                return valid;
                            }
                        }
                    }
                }
                if (valid)
                {
                    int nPos = txtDataTableName.Text.IndexOf(" ");
                    if (nPos >= 0 || AppData.Instance.IsReservedWord(txtDataTableName.Text.Trim()))
                    {
                        MsgBox.ShowError(SharedStrings.INVALID_DATATABLE_NAME);
                        txtDataTableName.Clear();
                        valid = false;
                        return valid;
                    }
                    else
                    {
                        if (txtDataTableName.Text.Trim().Length > 64)
                        {
                            MsgBox.ShowError(SharedStrings.DATA_TABLE_NAME_TOO_LONG);
                            txtDataTableName.Text = string.Empty;
                            valid = false;
                            return valid;
                        }
                    }
                }
            }
            return valid;
        }

        #endregion  //Private Methods

        #region Public Properties

        /// <summary>
        /// Gets or sets the unique starting ID
        /// </summary>
        public Int32 StartingID
        {
            get
            {
                return startingId;
            }
            set
            {
                startingId = value;
            }
        }

        /// <summary>
        /// Gets or sets the datatable name
        /// </summary>
        public string DataTableName
        {
            get
            {
                return dataTableName;
            }
            set
            {
                dataTableName = value;
            }
        }

        #endregion //Public Properties



    }
}

