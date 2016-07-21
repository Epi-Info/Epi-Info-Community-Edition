#region Namespaces
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Data.Services;
#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Form for creating a view
	/// </summary>
    public partial class CreateViewDialog : DialogBase, IWizardStep
	{
		#region Private Members
		private int viewID;
        private Project project;
        private string defaultViewName = string.Empty;
		#endregion  //Private Members

        #region Events
        /// <summary>
        /// Event raised to indicate that the user clicked on 'Back' button in a wizard
        /// </summary>
        public event EventHandler BackRequested;
        /// <summary>
        /// Event raised to indicate that the user clicked on 'Next' button in a wizard
        /// </summary>
        public event EventHandler NextRequested;
        /// <summary>
        /// Event raised to indicate that the user clicked on 'Cancel' button in a wizard
        /// </summary>
        public event EventHandler CancelRequested;
        #endregion Events

        #region Constructors
        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public CreateViewDialog()
		{
			InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="currentProject">The current project</param>
        public CreateViewDialog(MainForm frm, Project currentProject) : base(frm)
        {
            InitializeComponent();
            project = currentProject;
        }


        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="currentProject">The current project</param>
        /// <param name="initialViewName">A user-specified default view name.</param>
        public CreateViewDialog(MainForm frm, Project currentProject, string initialViewName)
            : base(frm)
        {
            InitializeComponent();
            project = currentProject;
            defaultViewName = initialViewName;
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
		/// Provides validation for the view name
		/// </summary>
		private bool ValidateViewName()
		{
            bool valid = true;
            string validationMessage = string.Empty;

            string trimmedFormNameCandidate = ViewName.Trim(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

            foreach (View view in project.Views)
            {
                if (ViewName.ToLowerInvariant() == view.Name.ToLowerInvariant())
                {
                    validationMessage = SharedStrings.INVALID_VIEW_NAME_DUPLICATE;
                    valid = false;
                    break;
                }

                if (trimmedFormNameCandidate.ToLowerInvariant() == view.Name.ToLowerInvariant())
                {
                    validationMessage = SharedStrings.INVALID_VIEW_NAME_DUPLICATE_PREFIX;
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                valid = View.IsValidViewName(ViewName, ref validationMessage);
            }

            if (!valid)
            {
                MsgBox.ShowError(validationMessage);
                txtViewName.Focus();
            }

            return valid;
		}

		#endregion  //Private Methods

		#region Event Handlers
		/// <summary>
		/// Reset the view name and set the focus
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void CreateView_Load(object sender, System.EventArgs e)
		{
            // If this dialog is part of a wizard, show back button. Otherwise, hide it.
            this.btnBack.Visible = this.IsInWizard;
			txtViewName.Text = defaultViewName;
			txtViewName.Focus();
            btnOK.Enabled = true;
		}

		/// <summary>
		/// Store the view name in a variable
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void txtViewName_TextChanged(object sender, System.EventArgs e)
		{
            //CheckForInputSufficiency();
		}

		/// <summary>
		/// Handles the click event of the OK button
		/// </summary>
		/// <param name="sender">Obejct that fired the event</param>
		/// <param name="e">.NET supplied event args</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{            
            if (ValidateViewName())
            {
                this.DialogResult = DialogResult.OK;
                if (this.IsInWizard)
                {
                    //Raise event NextRequested
                    if (this.NextRequested != null)
                    {
                        this.NextRequested(this, EventArgs.Empty);
                    }
                }
                else
                {
                    this.Hide();
                }
            }
		}

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.IsInWizard)
            {
                //Raise event BackRequested
                if (this.BackRequested != null)
                {
                    this.BackRequested(this, EventArgs.Empty);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.IsInWizard)
            {
                //Raise event BackRequested
                if (this.CancelRequested != null)
                {
                    this.CancelRequested(this, EventArgs.Empty);
                }
            }
            else
            {
                this.Close();
            }
        }

        private void InputDataChanged(object sender, EventArgs e)
        {
            //CheckForInputSufficiency();
        }
		#endregion Event Handlers

		#region Public Properties
		/// <summary>
		/// Gets/sets the view name
		/// </summary>
		public string ViewName
		{
            get
            {
                return txtViewName.Text;
            }
			set
			{


				txtViewName.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the View ID
		/// </summary>
		public int ViewId
		{
			get
			{
				return this.viewID;
			}
			set
			{
				this.viewID = value;
			}
		}

        /// <summary>
        /// The Back button
        /// </summary>
		public Button BtnBack
        {
            get
            {
                return btnBack;
            }
        }

        /// <summary>
        /// The Next button
        /// </summary>
		public Button BtnNext
        {
            get
            {
                return this.btnOK;
            }
        }

        /// <summary>
        /// The Cancel button
        /// </summary>
		public Button BtnCancel
        {
            get
            {
                return btnCancel;
            }
        }
		#endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Validates input on dialog
        /// </summary>
		public override void CheckForInputSufficiency()
        {
            Regex ViewNameCheck = new Regex("^[a-zA-z][a-zA-Z0-9]*$");

            if(! string.IsNullOrEmpty(this.txtViewName.Text.Trim()) && ViewNameCheck.IsMatch(this.txtViewName.Text.Trim()))
            {
                this.btnOK.Enabled = true;
            }
            else
            {
                this.btnOK.Enabled = false;
            }
        }
        #endregion Public Methods
    }	
}

