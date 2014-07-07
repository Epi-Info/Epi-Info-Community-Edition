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


namespace Epi.Windows.Dialogs
{
	/// <summary>
	/// Data Table Properties Dialog
	/// </summary>
    public partial class DataTablePropertiesDialog : DialogBase
	{

		#region Private Members
		private string originalDataTableName = string.Empty;
		private string originalStartingUniqueId = string.Empty;
		private Project project;
        private View currentView;
		#endregion

		#region Constructors

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public DataTablePropertiesDialog()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Default constructor for the class
		/// </summary>
		public DataTablePropertiesDialog(MainForm frm, Project currentProject, View view) : base(frm)
		{
            InitializeComponent();
			project = currentProject;
            currentView = view;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the name of the data table
		/// </summary>
		public string DataTableName
		{
			get
			{
				return txtDataTableName.Text;
			}
			set
			{
				txtDataTableName.Text = value;
			}
		}
		#endregion

		#region Private Properties
		/// <summary>
		/// Gets or sets the starting index of the data table
		/// </summary>
		public int StartingIndex
		{
			get
			{
				return int.Parse(txtIdStart.Text);
			}
			set
			{
				txtIdStart.Text = value.ToString();
			}
		}
		#endregion

        #region Private Methods
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
        /// Validates the starting index.  Its value must be numeric.
        /// </summary>
        private bool ValidateStartingId()
        {
            bool valid = true;
            for (int i = 0; i < txtIdStart.Text.Trim().Length; i++)
            {
                string uniqueId = txtIdStart.Text.Trim().Substring(i, 1);
                Match m = Regex.Match(uniqueId, "[0-9]");
                if (!m.Success)
                {
                    MsgBox.Show(SharedStrings.INVALID_STARTING_UNIQUE_ID, "Invalid Unique Id", MessageBoxButtons.OK);
                    valid = false;
                    txtIdStart.Clear();
                    txtIdStart.Focus();
                    return valid;
                }
            }

            if (valid)
            {
                StartingIndex = int.Parse(txtIdStart.Text.Trim());
            }
            return valid;
        }

        /// <summary>
        /// Enables or disables OK button
        /// </summary>
        private void EnableDisableOk()
        {
            btnOk.Enabled = (!string.IsNullOrEmpty(txtDataTableName.Text.Trim()) && !string.IsNullOrEmpty(txtIdStart.Text.Trim()));
        }

        #endregion // Private Methods

        #region Event Handlers

        /// <summary>
		/// Handles the click event of the OK button
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void btnOk_Click(object sender, System.EventArgs e)
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
		/// Handles the click event of the cancel button
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}
	
		/// <summary>
		/// Handles the load event of the form
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void DataTablePropertiesDialog_Load(object sender, System.EventArgs e)
		{
			txtDataTableName.Text = DataTableName;
			txtIdStart.Text = "1";
			originalDataTableName = txtDataTableName.Text.Trim();
			originalStartingUniqueId = txtIdStart.Text.Trim();
		}
 
		/// <summary>
		/// Handles the change event of the textbox
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void txtDataTableName_TextChanged(object sender, System.EventArgs e)
		{
            EnableDisableOk();
		}

		/// <summary>
		/// Handles the change event of the textbox
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void txtIdStart_TextChanged(object sender, System.EventArgs e)
		{
            EnableDisableOk();
		}
		#endregion
	}
}