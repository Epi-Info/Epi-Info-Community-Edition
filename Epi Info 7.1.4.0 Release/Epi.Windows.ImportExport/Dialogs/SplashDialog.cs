using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Epi;
using Epi.ImportExport;

namespace Epi.Windows.ImportExport.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SplashDialog : Form
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public SplashDialog(Epi.Epi2000.Project sourceProj, Epi.Project targetProj)
        {
            this.sourceProject = sourceProj;
            this.targetProject = targetProj;
            InitializeComponent();

        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Handles the Import Started event
        /// </summary>
        /// <param name="o">Object that fired the event</param>
        /// <param name="e">Import Started event parameters</param>
        public void OnImportStarted(object o, ImportStartedEventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = e.ObjectCount;
            progressBar1.Step = 1;
        }

        /// <summary>
        /// Handles the messaging of the Import's status
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Import status event parameters</param>
        public void OnStatusUpdate(object sender, MessageEventArgs e)
        {
            this.lblCurrentItem.Text = e.Message + " ...";
            Logger.Log(e.Message);
        }

        /// <summary>
        /// Handles the Import Ended event
        /// </summary>
        public void OnImportEnded()
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 0;
            progressBar1.Step = 0;
        }

        /// <summary>
        /// Handles the reporting of the Import progress of a view 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">View Import event parameters</param>
        public void OnItemImported(object sender, EventArgs e)
        {
            progressBar1.PerformStep();
        }
        #endregion Public Methods

        #region Event handlers
        private void SplashDialog_Load(object sender, EventArgs e)
        {
            try
            {
                ProjectUpgradeManager ImportManager = new ProjectUpgradeManager(sourceProject, targetProject);
                ImportManager.ImportStarted += new ImportStartedEventHandler(this.OnImportStarted);
                ImportManager.ImportStatus += new ImportStatusEventHandler(this.OnStatusUpdate);
                ImportManager.ImportEnded += new SimpleEventHandler(this.OnImportEnded);
                ImportManager.ViewImported += new EventHandler(this.OnItemImported);
                //ImportManager.Import();
            }
            finally
            {
                btnFinish.Enabled = true;
            }
           
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion Event handlers

        #region Fields
        private Epi.Epi2000.Project sourceProject;
        private Epi.Project targetProject;
        #endregion Fields

        

    }
}