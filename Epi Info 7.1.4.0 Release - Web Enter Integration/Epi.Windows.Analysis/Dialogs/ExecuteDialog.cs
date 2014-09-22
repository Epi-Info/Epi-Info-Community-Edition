using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Collections;
using Epi.Windows;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog used for EXECUTE command
    /// </summary>
    public partial class ExecuteDialog : CommandDesignDialog
    {
        #region Constructors
        /// <summary>
        /// ExecuteDialog Constructor
        /// </summary>
        /// <param name="frm">MainForm that the dialog is being called from.</param>
        public ExecuteDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Check for input Sufficiency
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }

        #endregion

        #region Private Methods

        private void Construct()
        {
            btnOK.Click += new EventHandler(btnOK_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnSaveOnly.Click += new EventHandler(btnSaveOnly_Click);
            btnHelp.Click += new EventHandler(btnHelp_Click);
            txtFilename.TextChanged += new EventHandler(txtFilename_TextChanged);
            chkWaitForExit.Checked = true;
            btnOK.Enabled = false;
            btnSaveOnly.Enabled = false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generate Command
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("EXECUTE ");
            if (!chkWaitForExit.Checked)
            {
                sb.Append("NOWAITFOREXIT ");
            }

            sb.Append(Util.InsertInDoubleQuotes(txtFilename.Text.Trim()));
            this.CommandText = sb.ToString();
        }

        /// <summary>
        /// ValidateInput
        /// </summary>
        /// <returns>bool isValid</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (String.IsNullOrEmpty(txtFilename.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_FILENAME_COMMAND);
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion

        #region Event Handlers

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFilename.Text = String.Empty;
            chkWaitForExit.Checked = true;
            txtFilename.Focus();
        }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                txtFilename.Text = openFileDialog1.FileName;
                CheckForInputSufficiency();
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-execute.html");
        }

        #endregion

    }
}
