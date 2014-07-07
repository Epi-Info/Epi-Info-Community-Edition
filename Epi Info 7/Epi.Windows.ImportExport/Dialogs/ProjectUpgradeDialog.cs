using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Epi.Data.Services;

using Epi.Windows;
using Epi.Windows.Dialogs;

namespace Epi.Windows.ImportExport.Dialogs
{
    /// <summary>
    /// The Project Import Dialog
    /// </summary>
    public partial class ProjectUpgradeDialog : ProjectCreationDialog
    {
        #region Private Data Members
        private Epi.Epi2000.Project sourceProject;
        #endregion  //Private Data Members

        #region Constructors

        /// <summary>
        /// Default constructor for Project Import Dialog
        /// </summary>
        public ProjectUpgradeDialog()
        {
            InitializeComponent();            
        }

        /// <summary>
        /// Constructor for Project Import Dialog 
        /// </summary>
        /// <param name="project">The source project</param>
        /// <param name="mainForm">Makeview's main form</param>
        public ProjectUpgradeDialog(Epi.Epi2000.Project project, MainForm mainForm)
            : base(mainForm)
        {
            #region Input Validation
            if (mainForm == null)
            {
                throw new ArgumentNullException("mainform");
            }

            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            #endregion  //Input Validation

            InitializeComponent();
            sourceProject = project;            
        }

        #endregion  Constructors

        #region Private Events

        /// <summary>
        /// Load the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void ProjectImportDialog_Load(object sender, EventArgs e)
        {
            SetUpForm();
        }
        
        #endregion  //Private Events

        #region Private Methods

        /// <summary>
        /// Sets up the Import Project dialog
        /// </summary>
        private void SetUpForm()
        {            
            grpView.Visible = false;
            //this.Controls.Add(grpSourceProject);
            //tableLayoutPanel.Controls.Add(grpSourceProject);
            //tableLayoutPanel.SetRow(grpSourceProject, 0);
            //tableLayoutPanel.SetRow(grpProject, 1);
            //tableLayoutPanel.SetRow(grpData, 2);

            //tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
            //tableLayoutPanel.RowStyles[1].SizeType = SizeType.Absolute;
            //tableLayoutPanel.RowStyles[1].Height = 200;
            //tableLayoutPanel.RowStyles[2].SizeType = SizeType.Absolute;
            //tableLayoutPanel.RowStyles[2].Height = 160;
            //tableLayoutPanel.Dock = DockStyle.Fill;
                        
            txtSourceProject.Text = sourceProject.Name;
            txtProjectName.Text = sourceProject.Name;
            base.PrepopulateDatabaseLocations();
        }

        /// <summary>
        /// Enables or disables the OK button depending on the state of other controls on the dialog
        /// </summary>
        protected override void EnableDisableOkButton()
        {
            // Check state of Project Name and View Name text boxes. If both have values, enable the OK button
            if (txtProjectName.Text.Length > 0 )
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
            }
        }

        #endregion  Private Methods     

        #region Protected Methods

        protected override void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult result = MsgBox.Show(SharedStrings.IMPORT_WARNING_MESSAGE, SharedStrings.WARNING, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Cancel)
            {
                return;
            }
            base.btnOk_Click(sender, e);
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        //protected override bool CreateProject()
        //{
        //    BeginBusy(SharedStrings.UPGRADING_PROJECT);

        //    IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
        //    if (manager == null)
        //    {
        //        throw new GeneralException("Project manager is not registered.");
        //    }

        //    project = manager.CreateNewProject();
        //    project.Name = txtProjectName.Text;
        //    project.Location = txtProjectLocation.Text;
        //    if (File.Exists(project.FilePath))
        //    {
        //        DialogResult dr = MessageBox.Show(SharedStrings.WARNING_Project_Already_Exists + ": " + project.FilePath, "Project Already Exists", MessageBoxButtons.YesNo);
        //        switch (dr)
        //        {
        //            case DialogResult.Yes:
        //                break;
        //            case DialogResult.No:
        //                return false;
        //        }
        //    }
            
        //    project.Description = txtProjectDescription.Text;
            
        //    // Determine metadata source: XML or database
        //    if (cbxMetadataDriver.Text.Trim().Equals(SharedStrings.XML))
        //    {
        //        project.MetadataSource = MetadataSource.Xml;
        //    }
        //    else
        //    {
        //        project.MetadataSource = MetadataSource.SameDb; // TODO: Replace with radio buttons.
        //    }

        //    //project.UseMetadataDbForCollectedData = chkUseSingleDatabase.Checked;
        //    //project.MetadataConnectionString = metadataConnectionString;
        //    //project.MetadataDriver = cbxMetadataDriver.SelectedValue.ToString();
        //    //project.CollectedDataConnectionString = collectedDataConnectionString;
        //    //project.CollectedDataDriver = cbxCollectedDataDriver.SelectedValue.ToString();
        //    //project.MetaDbInfo = this.MetaDBInfo;
        //    //project.CollectedDataDbInfo = this.CollectedDataDBInfo;

        //    if (project.MetadataSource == MetadataSource.Xml) // Metadata is stored in XML.                
        //    {
        //        project.UseMetadataDbForCollectedData = false;
        //        project.MetadataConnectionString = string.Empty;
        //        project.MetadataDriver = string.Empty;
        //        project.MetaDbInfo = null;
        //    }
        //    else // Metadata to be stored in a Relational database                
        //    {
        //        project.UseMetadataDbForCollectedData = rdbSameDb.Checked;
        //        project.MetadataDriver = cbxMetadataDriver.SelectedValue.ToString();
        //        project.MetadataConnectionString = metadataConnectionString;
        //        project.MetaDbInfo = metaDbInfo;

        //        MetadataDbProvider theMetadata = Project.Metadata as MetadataDbProvider;
        //        theMetadata.Initialize(metaDbInfo, cbxMetadataDriver.SelectedValue.ToString(), true);
        //    }

        //    if (project.UseMetadataDbForCollectedData)
        //    {
        //        project.CollectedDataDbInfo = Project.MetaDbInfo;
        //        project.CollectedDataDriver = cbxCollectedDataDriver.SelectedValue.ToString();
        //        project.CollectedDataConnectionString = collectedDataConnectionString;
        //        project.CollectedData.Initialize(metaDbInfo, cbxMetadataDriver.SelectedValue.ToString(), false);
        //    }
        //    else
        //    {
        //        project.CollectedDataDbInfo = this.collectedDataDBInfo;
        //        project.CollectedDataDriver = cbxCollectedDataDriver.SelectedValue.ToString();
        //        project.CollectedDataConnectionString = this.collectedDataDBInfo.DBCnnStringBuilder.ToString();
        //        project.CollectedData.Initialize(collectedDataDBInfo, cbxCollectedDataDriver.SelectedValue.ToString(), true);
        //    }
            
        //    //project.CreateProviders();
        //    project.CreateView(txtViewName.Text);
        //    project.Save();
        //    return true;
        //}

        protected override bool ValidateInput()
        {
            // Validate Project name
            if (txtProjectName.Text.Trim() == string.Empty)
            {
                ErrorMessages.Add("Project name is required"); // TODO: Hard coded string
            }
            else
            {
                // TODO: Validate project name for length and reserved words etc. The code is already there elsewhere in this class.
            }

            // Validate project location
            if (txtProjectLocation.Text.Trim() == string.Empty)
            {
                ErrorMessages.Add("Project location is required"); // TODO: Hard coded string
            }

            // Validate database
            if (txtCollectedData.Text.Trim() == string.Empty)
            {
                ErrorMessages.Add("Database information is required"); // TODO: Hard coded string
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion  Protected Methods
    }
}

