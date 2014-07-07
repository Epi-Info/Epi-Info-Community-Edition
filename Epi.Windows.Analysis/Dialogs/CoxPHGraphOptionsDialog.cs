using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog for CoxPH command
    /// </summary>
    public partial class CoxPHGraphOptionsDialog : CommandDesignDialog
    {
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CoxPHGraphOptionsDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// CoxPHGraphOptionsDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public CoxPHGraphOptionsDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// CoxPHGraphOptionsDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public CoxPHGraphOptionsDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, List<string> lstGraphVariables, string txtGraph, bool bolCustGraph)
            : base(frm)
        {
            
            InitializeComponent();
            lstGraphVars = lstGraphVariables;
            txtGraphType = txtGraph;
            bolCustomizeGraph = bolCustGraph;
            Construct();
        }
        #endregion Constructors

        #region Private Properties
        
        #endregion Private Properties

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }

        private void CoxPHGraphOptionsDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;
            FillVariableListBox(lbxGraphVars, scopeWord);
            foreach (string varname in lstGraphVars)
            {
                int index = lbxGraphVars.FindString(varname);
                lbxGraphVars.SetSelected(index, true);
            }
            
            cmbGraphType.Items.Add("None");
            cmbGraphType.Items.Add("Hazard Function"); 
            cmbGraphType.Items.Add("Log-Log Survival");
            cmbGraphType.Items.Add("Log-Log-Observed"); 
            cmbGraphType.Items.Add("Observed");
            cmbGraphType.Items.Add("Survival Probability");
            cmbGraphType.Items.Add("Survival-Observed");

            cmbGraphType.Text = "Survival Probability";
            
        }

        #endregion Private Methods

        #region Public Methods

        public List<string> lstGraphVars = null;
        public bool bolCustomizeGraph = false;
        public string txtGraphType = "Survival Probability";

        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
        }
        #endregion Public Methods

        #region Protected Methods
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (lbxGraphVars.SelectedItems.Count == 0)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_GRAPH_VAR);
            }
            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Event Handlers

        /// <summary>
        /// Handles the btnClear_Click event
        /// </summary>
        /// <remarks>Removes all items from comboboxes and other dialog controls</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbGraphType.Items.Clear();
            lbxGraphVars.Items.Clear();
            chkCustomGraph.Checked = false;  
            CoxPHGraphOptionsDialog_Load(this, null);
        }

        private void lbxGraphVars_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        #endregion Event Handlers

        private void lbxGraphVars_Leave(object sender, EventArgs e)
        {
            lstGraphVars.Clear();
            if (lbxGraphVars.SelectedItems.Count > 0)
            {
                foreach (string varname in lbxGraphVars.SelectedItems)
                {
                    lstGraphVars.Add(varname);
                }
            }
        }

        private void chkCustomGraph_Click(object sender, EventArgs e)
        {
            bolCustomizeGraph = chkCustomGraph.Checked;
        }

        private void cmbGraphType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtGraphType = cmbGraphType.Text;
        }
    }
}