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
    /// Dialog for the ROUTEOUT Command that directs output to the named file until the process is terminated by'
    /// CLOSEOUT. Output from commands such as FREQ and LIST is
    /// appended to the same output file as it is produced.
    /// <remarks>If no output is selected Epi Info will create a new file with a sequential number.</remarks>
    /// <example>ROUTEOUT 'D:\EPIInfo\Monthly_Report.htm' REPLACE</example>
    /// </summary>
    public partial class RouteoutDialog : CommandDesignDialog
	{
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public RouteoutDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// RouteoutDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public RouteoutDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

		#region Protected Methods
		/// <summary>
		/// Validates user input
		/// </summary>
		/// <returns>True/False depending upon whether error were derived from validation</returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput ();
            txtOutputFile.Text = txtOutputFile.Text.Trim();
            if( txtOutputFile.Text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
			{
				ErrorMessages.Add(SharedStrings.FILE_NOT_VALID);
			}
			return (ErrorMessages.Count == 0);
		}
		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(CommandNames.ROUTEOUT).Append(StringLiterals.SPACE);
			sb.Append(Epi.Util.InsertInDoubleQuotes(txtOutputFile.Text.Trim()));
            if (cbxReplaceFile.Checked)
            {
                sb.Append(StringLiterals.SPACE).Append(CommandNames.REPLACE);
            }
            else
            {
                sb.Append(StringLiterals.SPACE).Append(CommandNames.APPEND);
            }

			CommandText = sb.ToString();
		}
		/// <summary>
		/// Sets enabled property of OK and Save Only
		/// </summary>
		public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid;
		}
		#endregion //Protected Methods

		#region Event Handlers
		/// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtOutputFile.Text = string.Empty;
            cbxReplaceFile.Checked = false;
            CheckForInputSufficiency();
		}
		/// <summary>
		/// Enables OK and Save Only if validation passes after output file name is provided
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void txtOutputFile_Leave(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();			
		}
		/// <summary>
		/// Displays openfiledialog so user could specify the html file name to route outputs
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
        private void btnEllipse_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "HTML files|*.htm;*.html";
            dialog.Title = "RouteOut";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtOutputFile.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-routeout.html");
        }

		#endregion //Event Handlers

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

