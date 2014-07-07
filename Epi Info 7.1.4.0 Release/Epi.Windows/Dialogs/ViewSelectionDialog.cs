
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace Epi.Windows.Dialogs
{
	/// <summary>
	/// Dialog for selecting views
	/// </summary>
    public partial class ViewSelectionDialog : DialogBase
	{
		#region Private Class Members
		private int viewId;
		private Project currentProject = null;
		private bool includeRelatedViews;
        // private IProjectManager projectManager = null; 
		#endregion

		#region Constructors

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public ViewSelectionDialog()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The main form</param>
		/// <param name="project">Project with views to be loaded</param>
        public ViewSelectionDialog(MainForm frm, Project project) : base(frm)
		{
            InitializeComponent();
            currentProject = project;            
			LoadProject();
            if ((String.IsNullOrEmpty(txtCurrentProj.Text.Trim())))
            {
                btnOK.Enabled = false;
            }
            btnFindProject.Focus();
        }
		#endregion Constructors
 
		#region Private Methods

		/// <summary>
		/// Loads the views on the screen
		/// </summary>
		private void LoadViews()
		{
//            DataTable views = currentProject.GetViewsAsDataTable();     dcs0 7/7/2008 
			//lbxViews.DataSource = currentProject.GetViewNames();
            try
            {
                if (currentProject.GetParentViewNames().Count == 0)
                {
                    //Setting project to new if no rows returned from the SQL Select
                    //to reset the dialog to its original state -- den4 11/23/2010
                    currentProject = new Project();
                    return;
                }
                lbxViews.DataSource = currentProject.GetParentViewNames();             
            }
            catch (ApplicationException) 
            {
                MsgBox.ShowError("There was a problem loading the project \"" + currentProject.Name + "\". Please make sure the database for this project exists.");
            }
            //lbxViews.DisplayMember = ColumnNames.NAME;
            //lbxViews.ValueMember = ColumnNames.VIEW_ID;                        
		}

		/// <summary>
		/// Opens a dialog for project selection
		/// </summary>
		//private void SelectProject()
        private bool SetCurrentProject()
		{
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = SharedStrings.SELECT_PROJECT;
                openFileDialog.Filter = "Project Files ( *" + Epi.FileExtensions.EPI_PROJ + ")|*" + Epi.FileExtensions.EPI_PROJ;
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                openFileDialog.InitialDirectory = Configuration.GetNewInstance().Directories.Project;
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (openFileDialog.FileName.ToLower().Trim().EndsWith(Epi.FileExtensions.EPI_PROJ))
                    {
                        if (System.IO.File.Exists(openFileDialog.FileName))
                        {
                            /*
                            IProjectManager manager = MainForm.Module.GetService(typeof(IProjectManager)) as IProjectManager;
                            if (manager == null)
                            {
                                throw new GeneralException("Project manager is not registered.");
                            }*/

                            currentProject = new Project(openFileDialog.FileName);
                            if (currentProject.CollectedData.FullName.Contains("MS Access"))
                            {
                                string databaseFileName = currentProject.CollectedData.DataSource.Replace("Data Source=".ToLower(), string.Empty);
                                if (System.IO.File.Exists(databaseFileName) == false)
                                {
                                    MsgBox.ShowError(string.Format(SharedStrings.DATASOURCE_NOT_FOUND, databaseFileName));
                                    return false;
                                }
                            }

                            /*
                            IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
                            if (host == null)
                            {
                                throw new GeneralException("Project host is not registered.");
                            }
                            else
                            {
                                host.CurrentProject = currentProject;
                            }*/
                        }
                        else
                        {
                            throw new System.IO.FileNotFoundException(string.Empty, openFileDialog.FileName);
                        }
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    currentProject = null;
                }
            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                return false;
                //throw;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex.Message));
                return false;
            }
			finally
			{
			}

            return true;
		}

		/// <summary>
		/// Loads a project
		/// </summary>
		private void LoadProject()
		{
            if (currentProject != null)
            {
                LoadViews();
                txtCurrentProj.Text = currentProject.FilePath;
            }
		}

        /// <summary>
        /// Saves the user's selection
        /// </summary>
        private void SaveSettings()
        {
            string viewName = lbxViews.SelectedValue.ToString();
            View view = CurrentProject.Metadata.GetViewByFullName(":" + viewName);
            ViewId = view.Id;	
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
            //ViewId = int.Parse(lbxViews.SelectedValue.ToString());                        
            SaveSettings();            		
		}

		/// <summary>
		/// Handles click event of Find Project button.
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnFindProject_Click(object sender, System.EventArgs e)
		{
            if (SetCurrentProject() == false)
            {
                return;
            }

            LoadProject();
            if (!(String.IsNullOrEmpty(txtCurrentProj.Text.Trim())))
            {
                btnOK.Enabled = true;
                btnOK.Focus();
		    }
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
            if (!(String.IsNullOrEmpty(txtCurrentProj.Text.Trim())))
            {
                btnOK.Enabled = true;
                btnOK.Focus();
            }
        }

        private void lbxViews_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCurrentProj.Text) && lbxViews.SelectedItems.Count > 0)
            {
                SaveSettings();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
	}
}

