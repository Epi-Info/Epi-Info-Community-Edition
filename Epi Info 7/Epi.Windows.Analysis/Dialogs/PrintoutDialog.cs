using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Printout command
	/// </summary>
    public partial class PrintoutDialog : CommandDesignDialog
	{
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public PrintoutDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Printout dialog
        /// </summary>
        /// <param name="frm"></param>
        public PrintoutDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

		#region Event Handlers
		/// <summary>
		/// Display OpenFileDialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnEllipse_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "HTML Files(*.HTM)|" + "*.HTM|All files (*.*)|*.*";
			dialog.Title = "PrintOut";
			if(dialog.ShowDialog() == DialogResult.OK)
			{
		
				txtFilename.Text = dialog.FileName;
					
			
			}
		}
		/// <summary>
		/// Clear button clears the text
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtFilename.Text = string.Empty;
            txtFilename.Focus();
		
		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-printout.html");
        }

		#endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Generate command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(CommandNames.PRINTOUT).Append(StringLiterals.SPACE);
            if (txtFilename.Text.Trim().Length != 0)
                sb.Append(Util.InsertInSingleQuotes(txtFilename.Text.Trim()));
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
    }
}