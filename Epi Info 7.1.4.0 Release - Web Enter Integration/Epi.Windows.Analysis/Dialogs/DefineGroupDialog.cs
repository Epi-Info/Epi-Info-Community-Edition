using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// The Define Group Dialog
    /// </summary>
    public partial class DefineGroupDialog : CommandDesignDialog
    {
        /// <summary>
        /// Default constructor for define group dialog
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public DefineGroupDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for DefineGroupDialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public DefineGroupDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        #region Public Methods
        /// <summary>
        /// Checks if input is sufficient and Enables control buttons accordingly
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (string.IsNullOrEmpty(txtGroupVar.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_DEFINE_VAR);
            }
            if (lbxVariables.SelectedItems.Count < 1)
            {
                ErrorMessages.Add(SharedStrings.NO_VARS_SELECTED);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generates command text
        /// </summary>
        protected override void GenerateCommand()
        {
            WordBuilder command = new WordBuilder();
            command.Append(CommandNames.DEFINE);
            command.Append(txtGroupVar.Text.Trim());
            command.Append(CommandNames.GROUPVAR);
            foreach (string s in lbxVariables.SelectedItems)
            {
                command.Append(s);
            }
            CommandText = command.ToString();
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Loads the dialog
        /// </summary>
        private void Construct()
        {
            VariableType scopeWord = VariableType.Standard | VariableType.Global | VariableType.Permanent |
                                                 VariableType.DataSource | VariableType.DataSourceRedefined;
            FillVariableListBox(this.lbxVariables, scopeWord);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
        }
        #endregion Private Methods

        #region Event Handlers
        private void Something_Changed(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-define-group-command.html");
        }

        #endregion Event Handlers

    }
}

