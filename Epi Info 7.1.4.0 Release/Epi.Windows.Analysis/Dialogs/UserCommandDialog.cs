using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Core.AnalysisInterpreter;
using Epi.Data;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog used for RunPGM command
    /// </summary>
    public partial class UserCommandDialog : CommandDesignDialog
    {
        #region Constructors

        /// <summary>
        /// Constructor for dialog
        /// </summary>
        /// <param name="frm"></param>
        public UserCommandDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            construct();
            LoadPGMList();
            CheckForInputSufficiency();
        }
        #endregion

        #region private members

        private DataTable programs;

        #region private methods
        private void construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            
        }

        private void LoadPGMList()
        {

            //if (txtFileName.Text.EndsWith(".prj",StringComparison.CurrentCultureIgnoreCase))
            //{
            //    if (System.IO.File.Exists(txtFileName.Text))
            //    {
            //        Project project = new Project(txtFileName.Text);
            //        programs = project.GetPgms();
            //        cmbCommandName.Enabled = true;
            //        cmbCommandName.DataSource = programs;
            //        cmbCommandName.DisplayMember = ColumnNames.NAME;
            //        cmbCommandName.ValueMember = ColumnNames.PROGRAM_ID;
            //        cmbCommandName.SelectedIndex = -1;
            //        if (cmbCommandName.Items.Count <= 0) 
            //        {
            //            ErrorMessages.Add(SharedStrings.NO_SAVED_PGM);
            //        }
            //    }
            //}
            //else
            //{
            //    cmbCommandName.Enabled = false;
            //    cmbCommandName.SelectedIndex = -1;
            //    this.btnOK.Enabled = true;
            //}
        }
        #endregion

        #endregion

        #region protected methods

        /// <summary>
        /// Validates input
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            //if (String.IsNullOrEmpty(txtFileName.Text.Trim()))
            //{
            //    ErrorMessages.Add(SharedStrings.NO_FILENAME);
            //}
            //else
            //{
            //    //if (!txtFileName.Text.EndsWith(".prj", StringComparison.CurrentCultureIgnoreCase) && !txtFileName.Text.EndsWith(".pgm", StringComparison.CurrentCultureIgnoreCase))
            //    if (!txtFileName.Text.EndsWith(".prj", StringComparison.CurrentCultureIgnoreCase) && !txtFileName.Text.EndsWith(".pgm7", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        ErrorMessages.Add(SharedStrings.INVALID_PROGRAM_FILE);
            //    }
            //}

            if (cmbCommandName.Enabled)
            {
                if (Util.IsEmpty(cmbCommandName.SelectedItem))
                {
                    ErrorMessages.Add(SharedStrings.NO_PROGRAM);
                }
            }

            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Enables/Disables controls
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }

        /// <summary>
        /// Generate Command
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Epi.CommandNames.RUNPGM);
            sb.Append(StringLiterals.SPACE);

            //sb.Append(StringLiterals.DOUBLEQUOTES);
             
            sb.Append(StringLiterals.SINGLEQUOTES);
            //sb.Append(txtFileName.Text);
            //sb.Append(StringLiterals.DOUBLEQUOTES);
            sb.Append(StringLiterals.SINGLEQUOTES);

            if (cmbCommandName.Enabled)
            {
                if (!cmbCommandName.SelectedIndex.Equals(-1))
                {
                    sb.Append(StringLiterals.COLON);
                    sb.Append(cmbCommandName.Text.Trim());
                }
            }

            CommandText = sb.ToString().Trim();
        }
        #endregion

        #region event handlers

        private void txtFileName_Leave(object sender, EventArgs e)
        {
            //txtFileName.Text = txtFileName.Text.Trim();
            LoadPGMList();
            CheckForInputSufficiency();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Program Files|*.prj;*.pgm7|EpiInfo 7 Project|*.prj|PGM|*.pgm7";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                
                LoadPGMList();
            }
        }

        private void cmbProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            LoadPGMList();
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-runpgm.html");
        }

        #endregion


    }
}
