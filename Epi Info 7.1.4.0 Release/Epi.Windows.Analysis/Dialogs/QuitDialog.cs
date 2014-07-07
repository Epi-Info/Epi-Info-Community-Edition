#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi;

#endregion Namespaces

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Quit command
	/// </summary>
    public partial class QuitDialog : CommandDesignDialog
	{
		#region Constructors	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public QuitDialog()
		{
			InitializeComponent();
		}

		/// <summary>
        /// Constructor for QuitDialog.
		/// </summary>
		/// <param name="frm">The main form</param>
        public QuitDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
        /// <summary>
        /// Constructor for QuitDialog. if showSave is true btnShowSaveOnly is enabled during form load.
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="showSave">Boolean to denote whether to display the Show Only button on the dialog</param>
        public QuitDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            showSaveOnly = showSave;
            if (showSaveOnly)
            {
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            Construct();
        }

		#endregion Constructors

        #region Private Attributes
        private bool showSaveOnly = true;
        #endregion Private Attributes

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
        /// Repositions buttons on dialog
        /// </summary>
        private void RepositionButtons()
        {
            int x = btnCancel.Left;
            int y = btnCancel.Top;
            btnCancel.Location = new Point(btnOK.Left, y);
            btnOK.Location = new Point(btnSaveOnly.Left, y);
            btnSaveOnly.Location = new Point(x, y);         
        }
        #endregion Private Methods

        #region Protected Methods
        /// <summary>
		/// Generate command text
		/// </summary>
		protected override void GenerateCommand()
		{
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.QUIT);
            CommandText = sb.ToString();
		}

		#endregion //Protected Methods		

        #region Event Handlers

        /// <summary>
        /// Loads the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void QuitDialog_Load(object sender, EventArgs e)
        {
            btnSaveOnly.Visible = showSaveOnly;
            if (showSaveOnly)
            {
                RepositionButtons();
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-quit.html");
        }

        #endregion Event Handlers
    }
}