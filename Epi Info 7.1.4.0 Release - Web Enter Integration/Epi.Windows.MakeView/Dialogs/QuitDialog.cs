#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;

#endregion Namespaces

namespace Epi.Windows.MakeView.Dialogs
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
        public QuitDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
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
        public QuitDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm, bool showSave)
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
        private bool showSaveOnly = false;
        #endregion Private Attributes

        #region Private Methods
        private void Construct()
        {
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);            
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
			CommandText = CommandNames.QUIT;
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
        
        #endregion Event Handlers
    }
}