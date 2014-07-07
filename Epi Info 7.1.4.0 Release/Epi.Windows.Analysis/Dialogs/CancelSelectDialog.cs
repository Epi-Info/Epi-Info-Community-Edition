using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Analysis;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for CancelSelect command
	/// </summary>
    public partial class CancelSelect : CommandDesignDialog
	{
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public CancelSelect()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the CancelSeletc dialog
        /// </summary>
        /// <param name="frm"></param>
        public CancelSelect(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

		#region Protected Methods
		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
            //DEFECT 1136 .SELECT to .CANCELSELECT
			sb.Append((CommandNames.CANCELSELECT).ToUpper());
			CommandText = sb.ToString();
		}
		#endregion //Protected Methods

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }
        #endregion Private Methods

        #region Event Handlers
        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-cancel-select-or-sort.html");
        }


        #endregion Event Handlers
    }
}