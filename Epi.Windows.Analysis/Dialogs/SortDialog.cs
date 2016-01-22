using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Analysis;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Sort command
	/// </summary>
    public partial class SortDialog : CommandDesignDialog
	{
		#region Private Class Members
        //private DataTable variables = null;
		#endregion //Private Class Members

		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public SortDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for Sort Dialog
        /// </summary>
        /// <param name="frm"></param>
        public SortDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
            InitializeComponent();
            Construct();
        }
		#endregion Constructors

        #region Public Methods
        /// <summary>
        /// Enable/disable OK and Save buttons depending on completeness of input
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }

        #endregion

        #region Protected Methods
        /// <summary>
		/// Validates user input
		/// </summary>
		/// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput ();
			if (lbxSortVar.Items.Count == 0 )
			{
				ErrorMessages.Add(SharedStrings.NO_SORT_VARIABLES_SELECTED);
			}
			return (ErrorMessages.Count == 0);
		}
		/// <summary>
		/// Generate the command text
		/// </summary>
		protected override void GenerateCommand()
		{
			WordBuilder command = new WordBuilder();

			command.Append(CommandNames.SORT);
			foreach (string item in lbxSortVar.Items) 
			{
				command.Append(item.Substring(0,item.Length - 4));;
				//Append 'DESCENDING'  
				if (item.EndsWith(StringLiterals.HYPHEN + StringLiterals.PARANTHESES_CLOSE))
				{
					command.Append(CommandNames.DESCENDING);
				}
			}
			CommandText = command.ToString();
		}

		#endregion //Protected Methods

		#region Event Handlers
		/// <summary>
		/// Clears all user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard |
                          VariableType.Global;  //Zack / delete permanent vars since no need to sort by constant
            FillVariableListBox(lbxAvailableVar, scopeWord);  //Zack, don't need this.  see defect 44 and Epi3x behavior
            //even if want do this, the sort vars should not appear in the lbxAvailableVar  

            lbxSortVar.Items.Clear();
			CheckForInputSufficiency();
			
		}
		/// <summary>
		/// Populates  Sort Variables listbox with value selected from Available variable along with sort order selected
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">DoubleClick</param>
		private void lbxAvailableVar_DoubleClick(object sender, System.EventArgs e)
		{
		
            if (lbxAvailableVar.Items.Count > 0)
			{
                string strSortDirection = String.Empty;
                if (rdbAscending.Checked)
                {
                    strSortDirection = StringLiterals.ASCENDING;
                }
                else if (rdbDescending.Checked)
                {
                    strSortDirection = StringLiterals.DESCENDING;
                }

                if (lbxAvailableVar.SelectedItem != null)
                {
                    string strAvailableVar = FieldNameNeedsBrackets(lbxAvailableVar.SelectedItem.ToString()) ? Util.InsertInSquareBrackets(lbxAvailableVar.SelectedItem.ToString()) : lbxAvailableVar.SelectedItem.ToString();
        			lbxSortVar.Items.Add(strAvailableVar + strSortDirection);
                    lbxAvailableVar.Items.RemoveAt(lbxAvailableVar.SelectedIndex);
				}
				if (lbxSortVar.Items.Count > 0)
				{
					lbxSortVar.SetSelected(lbxSortVar.Items.Count - 1,true);
				}
			}
			CheckForInputSufficiency();
		}

        /// <summary>
        /// Removes selected value from Sort Variables list box and returns it to the Available Variables list box.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">DoubleClick</param>
        private void lbxSortVar_DoubleClick(object sender, System.EventArgs e)
        {
            if (lbxSortVar.Items.Count > 0)
            {
                int selectedIndex = lbxSortVar.SelectedIndex;
                string selectedText = lbxSortVar.SelectedItem.ToString();
                selectedText = selectedText.Substring(0, selectedText.IndexOf("("));
                //remove square brackets before adding back to DDLs
                char[] cTrimParens = { '[', ']' };
                selectedText = selectedText.Trim(cTrimParens);
                lbxSortVar.Items.RemoveAt(selectedIndex);
                lbxAvailableVar.Items.Add(selectedText);
                rdbAscending.Checked = true;
                if (lbxSortVar.Items.Count == 0)
                {
                    CheckForInputSufficiency();
                    return;
                }
                else
                {
                    SelectListBoxItem(lbxSortVar.Items.Count - 1);
                }
            }
            CheckForInputSufficiency();
        }

		/// <summary>
		/// Common event handler for radio button click event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void RadioButtonClick(object sender, System.EventArgs e)
		{
			if (lbxSortVar.Items.Count > 0 )
			{
				int selectedIndex = lbxSortVar.SelectedIndex;
				string selectedText = lbxSortVar.SelectedItem.ToString();
				if (rdbAscending.Checked)
				{
					lbxSortVar.Items.RemoveAt(selectedIndex);
					lbxSortVar.Items.Insert(selectedIndex, selectedText.Replace(StringLiterals.HYPHEN + StringLiterals.HYPHEN,StringLiterals.PLUS + StringLiterals.PLUS));
					SelectListBoxItem(selectedIndex);
				}
				else if (rdbDescending.Checked)
				{
					lbxSortVar.Items.RemoveAt(selectedIndex);
					lbxSortVar.Items.Insert(selectedIndex, selectedText.Replace(StringLiterals.PLUS + StringLiterals.PLUS,StringLiterals.HYPHEN + StringLiterals.HYPHEN));
					SelectListBoxItem(selectedIndex);
				}
				else if (rdbRemoveSort.Checked)
				{
                    string removeText = lbxSortVar.SelectedItem.ToString();
                    removeText = removeText.Substring(0, removeText.IndexOf("("));
                    //remove square brackets before adding back to DDLs
                    char[] cTrimParens = { '[', ']' };
                    removeText = removeText.Trim(cTrimParens);
                    lbxSortVar.Items.RemoveAt(selectedIndex);
                    lbxAvailableVar.Items.Add(removeText);
					rdbAscending.Checked = true;
					if (lbxSortVar.Items.Count == 0)
					{
						CheckForInputSufficiency();
						return;
					}
					else 
					{
						SelectListBoxItem(lbxSortVar.Items.Count - 1);
					}
						
				}
			}
			CheckForInputSufficiency();
		}
		private void SortDialog_Load(object sender, System.EventArgs e)
		{
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard |
                          VariableType.Global | VariableType.Permanent;
        //Defect 1128
            //scopeWord = VariableType.DataSource;
            FillVariableListBox(lbxAvailableVar, scopeWord);
		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-sort.html");
        }

		#endregion Event Handlers

		#region Private Methods


        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }

        /// <summary>
		/// Selects the specified item in the Listbox
		/// </summary>
		/// <param name="selectIndex">index of the item to select</param>
		private void SelectListBoxItem(int selectIndex)
		{
			#region Input Validation
			if ((selectIndex > lbxSortVar.Items.Count) || (selectIndex < 0))
			{
				throw new ArgumentOutOfRangeException("selectIndex");
			}
			#endregion Input Validation
			lbxSortVar.SetSelected(selectIndex,true);
			
		}

        /*
        private void LoadSortVariables()
        {
            AnalysisWindowsModule module = this.Module as AnalysisWindowsModule;
            SortCriteria sortCriteria = module.Processor.Session.DataSourceInfo.SortCriteria;
            foreach (IVariable var in sortCriteria)
            {
                // TODO: Remove this variable from the master list
                string varDisplayName = var.Name;
                SortOrder sortOrder = sortCriteria.GetSortOrderForVariable(var);
                varDisplayName += (sortOrder == SortOrder.Descending) ? "(--)" : "(++)";
                lbxSortVar.Items.Add(varDisplayName);
            }
        }*/
		#endregion Private Methods

	
	}
}

