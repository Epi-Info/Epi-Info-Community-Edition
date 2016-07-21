using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Data.Services;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// A dialog box used for renaming pages in a View.
	/// </summary>
    public partial class RenamePageDialog : DialogBase
	{
		private string pageName = string.Empty;
        private PageNode pageNode = null;
        private View parentView = null;

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public RenamePageDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public RenamePageDialog(MainForm frm, View parentView)
            : base(frm)
        {
            this.parentView = parentView;
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The calling form</param>
        /// <param name="node">The original page node</param>        
        public RenamePageDialog(MainForm frm, PageNode node)
            : base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            this.PageNode = node;
            this.parentView = node.Page.view;
        }

        /// <summary>
        /// Handles the text changed event for the Page Name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void txtPageName_TextChanged(object sender, System.EventArgs e)
		{
			PageName = txtPageName.Text.Trim();

            string message;

            if (txtPageName.Text.Length > 0 && ValidatePageName(out message))
            {
                btnOK.Enabled = true;
                txtPageName.ForeColor = Color.Black;
            }
            else
            {
                btnOK.Enabled = false;
                txtPageName.ForeColor = Color.Red;
            }
		}

        /// <summary>
        /// Handles the Click Event for the Cancel button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handles the Click Event for the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, System.EventArgs e)
		{
            string validationMessage;
            if (ValidatePageName(out validationMessage) == false)
            {                
                this.DialogResult = DialogResult.None;                
                txtPageName.Focus();
                return;
            }

			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

        /// <summary>
        /// Handles the Load event for the RenamePage dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void RenamePage_Load(object sender, System.EventArgs e)
		{
            txtPageName.Text = PageName;
			txtPageName.Focus();
		}

        /// <summary>
        /// Provides validation for the page name
        /// </summary>
        private bool ValidatePageName(out string message)
        {
            bool valid = true;
            string validationMessage = string.Empty;

            foreach (Page page in parentView.Pages)
            {
                if (PageName.ToLowerInvariant().Trim().Equals(page.Name.ToLowerInvariant()) || PageName.ToLowerInvariant().Equals(page.Name.ToLowerInvariant()))
                {
                  if (!(PageNode != null && PageNode.Text.ToLowerInvariant() == PageName.ToLowerInvariant()) )
                    
                    {
                        validationMessage = SharedStrings.INVALID_PAGE_NAME_DUPLICATE;
                        valid = false;
                        break;
                    }
                  
                }
            }
            if (PageName.ToLowerInvariant() == "page" && string.IsNullOrEmpty(validationMessage))
            {
                validationMessage = SharedStrings.INVALID_PAGE_NAME_IS_RESERVED;
                valid = false;
               
            }
            if (valid)
            {
                valid = Page.IsValidPageName(PageName, ref validationMessage);
            }

            message = validationMessage;

            return valid;
        }

		/// <summary>
		/// Gets and sets the page name
		/// </summary>
		public string PageName
		{
			get
			{
				return this.pageName;
			}
			set
			{
				this.pageName = value;
			}
		}

        /// <summary>
        /// Gets the original page node
        /// </summary>
        public PageNode PageNode
        {
            get
            {
                return this.pageNode;
            }
            set
            {
                this.pageNode = value;
            }
        }
	}
}

