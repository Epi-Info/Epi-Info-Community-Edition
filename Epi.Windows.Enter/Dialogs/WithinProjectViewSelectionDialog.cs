using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Enter;

namespace Epi.Enter.Dialogs
{
    public partial class WithinProjectViewSelectionDialog : Form
    {

        #region Private Class Members
		private int viewId;
		private Project currentProject = null;
		private bool includeRelatedViews;
		#endregion

		#region Constructors

        public WithinProjectViewSelectionDialog()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The main form</param>
		/// <param name="project">Project with views to be loaded</param>
        public WithinProjectViewSelectionDialog(MainForm frm, Project project)
		{
            InitializeComponent();
            currentProject = project;            
			LoadProject();
        }
		#endregion Constructors
 
		#region Private Methods

		/// <summary>
		/// Loads the views on the screen
		/// </summary>
		private void LoadViews()
		{
            try
            {
                if (currentProject.GetParentViewNames().Count == 0)
                {
                    currentProject = new Project();
                    return;
                }
                lbxViews.DataSource = currentProject.GetParentViewNames();             
            }
            catch (ApplicationException) 
            {
                MsgBox.ShowError("There was a problem loading the project \"" + currentProject.Name + "\". Please make sure the database for this project exists.");
            }                      
		}

		/// <summary>
		/// Loads a project
		/// </summary>
		private void LoadProject()
		{
            if (currentProject != null)
            {
                LoadViews();
            }
		}       

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles click event of OK button.
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
            btnOK_Click();
		}

        private void btnOK_Click()
        {
            string viewName = lbxViews.SelectedValue.ToString();
            View view = CurrentProject.Metadata.GetViewByFullName(":" + viewName);
            ViewId = view.Id;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a value indicating whether to get related views
		/// </summary>
		public bool IncludeRelatedViews
		{
			get
			{
				return (includeRelatedViews);
			}
			set
			{
				includeRelatedViews = value;
			}
		}

		/// <summary>
		/// Gets or sets the View Id
		/// </summary>
		public int ViewId
		{
			get
			{
				return (viewId);
			}
			set
			{
				viewId = value;
			}
		}      

        /// <summary>
        /// Get the current project
        /// </summary>
        public Project CurrentProject
        {
            get
            {
                return (currentProject);
            }
        }
		#endregion      

        private void lbxViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = true;
            btnOk.Focus();
        }

        private void LbxViews_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            btnOK_Click();
        }
    }
}
