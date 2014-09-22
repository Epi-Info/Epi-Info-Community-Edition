using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Epi;
using Epi.Windows.Dialogs;
using Epi.Windows;
using Epi.Core.AnalysisInterpreter;

namespace Epi.Windows
{

	/// <summary>
	/// A Main form is a form that occupies full screen; has a menu bar and a status bar.
	/// Examples include MakeView, Enter and Analysis forms and Check code editor.
	/// </summary>
    public partial class MainForm : FormBase, IServiceProvider 
	{
		#region Private Members
		private string formName = string.Empty;
        private IWindowsModule module = null;
        
        
		#endregion Private Members

        #region Constructor
        /// <summary>
        /// Default constructor - For design mode only
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            

            this.tsslCaps.Enabled = base.IsKeyToggled((int)Keys.CapsLock);
            this.tsslNUM.Enabled = base.IsKeyToggled((int)Keys.NumLock);
            this.tsslINS.Enabled = base.IsKeyToggled((int)Keys.Insert);

            this.KeyUp += new KeyEventHandler(OnKeyUp);
        }

        /// <summary>
		/// Constructor for the class
		/// </summary>
		public MainForm(IWindowsModule mod)
        {
            #region Input Validation
            if (mod == null)
            {
                throw new ArgumentNullException("mod");
            }
            #endregion Input Validation

            InitializeComponent();
            this.module = mod;

            this.tsslLocale.ToolTipText = SharedStrings.CURRENT_LOCALE + "; " + SharedStrings.CLICK_TO_CHANGE;

            this.tsslCaps.Enabled = base.IsKeyToggled((int)Keys.CapsLock);
            this.tsslNUM.Enabled = base.IsKeyToggled((int)Keys.NumLock);
            this.tsslINS.Enabled = base.IsKeyToggled((int)Keys.Insert);

            this.KeyUp += new KeyEventHandler(OnKeyUp);
		}

		#endregion Constructors

        [System.Diagnostics.DebuggerStepThrough()]
        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(serviceType);
        }

        /// <summary>
        /// Gets the service object of the specified type, if it is available
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        protected override object GetService(Type serviceType)
        {
            if (serviceType == this.GetType())
            {
                return this;
            }
            //small windows optimzation to prevent unnecessary call stack execution
            else if (serviceType == typeof(AmbientProperties))
            {
                return null;
            }
            else
            {
                object service = this.module.GetService(serviceType);
                if (service == null)
                {
                    // call into .net framework for resolution
                    service = base.GetService(serviceType);
                }
                return service;
            }
        }

        #region Public Properties

        /// <summary>
        /// Returns the module
        /// </summary>
		public IWindowsModule Module
        {
            get            
            {
                return this.module;
            }

            set { this.module = value; }
        }

        public string TemplateNode
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// Gets and sets the form name
        /// </summary>
		protected string FormName
		{
			get
			{
				return formName;
			}
			set
			{
				formName = value;
			}
		}

        /// <summary>
        /// Obsolete property. Delete it // TODO
        /// </summary>
        [Obsolete("Use FormName Property", false)]
        protected string WindowTitle
        {
            get
            {
                return FormName;
            }
            set
            {
                FormName = value;
            }
        }

        /// <summary>
        /// Retrieves the module's manager
        /// </summary>
		protected IModuleManager ModuleManager
        {
            get
            {
                // service locator pattern
                IModuleManager factory = this.GetService(typeof(IModuleManager)) as IModuleManager;
                if (factory == null)
                {
                    MessageBox.Show("Module factory service not registered."); // TODO: Hard coded string
                    return null;
                }
                return factory;
            }
        }


		#endregion "Protected Properties"
		
		#region Protected Methods
        

		/// <summary>
		/// Sets the cursor to wait and displays a status message
		/// </summary>
		/// <param name="str"></param>
		public void BeginBusy(string str)
		{
			UpdateStatus(str, false);
			this.Cursor = Cursors.WaitCursor;
		}
		/// <summary>
		/// Sets the cursor to normal and all status messages to default
		/// </summary>
        public void EndBusy()
		{
			this.Cursor = Cursors.Default;
            UpdateStatus(SharedStrings.READY, false);
		}

        public void BeginIndeterminate()
        {
            try
            {
                this.BeginInvoke((ThreadStart)delegate()
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                });
            }
            catch (Exception ex)
            {
                //
            }
        }

        public void EndIndeterminate()
        {
            try
            {
                this.BeginInvoke((ThreadStart)delegate()
                {
                    ProgressReportEnd();
                    progressBar.Style = ProgressBarStyle.Continuous;
                });
            }
            catch (Exception ex)
            {
                //
            }
        }

		/// <summary>
		/// Displays text in the info area of the status bar
		/// </summary>
		/// <param name="str"></param>
        public void UpdateStatus(string str)
		{
            this.tsslMessage.Text = str;
            Logger.Log(str);
            Application.DoEvents();
		}

        /// <summary>
        /// Displays text in the info area of the status bar
        /// </summary>
        /// <param name="str">The string to display</param>
        /// <param name="shouldAddLogEntry">Whether or not to create a log entry for this status event</param>
        /// <param name="doEvents">Whether or not to run the DoEvents() method</param>
        public void UpdateStatus(string str, bool shouldAddLogEntry, bool doEvents = false)
        {
            this.tsslMessage.Text = str;
            if (shouldAddLogEntry)
            {
                Logger.Log(DateTime.Now + ":  " + str);
            }
            if (doEvents)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Displays text in the application specific information area of the status bar
        /// </summary>
        /// <param name="str">The application specific information</param>
        /// TODO: Find better terminology to use here (E. Knudsen 9/8/2010) - this maybe should say 'Saved Record' or 'Existing Record', etc
        public void UpdateAppSpecificInfo(string str)
        {
            this.tsslAppSpecificInfo.Text = str;
        }     

        /// <summary>
        /// Sets all status messages to default
        /// </summary>
        protected void UpdateAppSpecificInfoDefault()
        {
            UpdateAppSpecificInfo(string.Empty);
        }

        /// <summary>
        /// Initializes Progress Bar
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="step"></param>
        protected void ProgressReportBegin(int min, int max, int step)
        {
            progressBar.Minimum = min;
            progressBar.Maximum = max;
            progressBar.Step = step;
        }

        /// <summary>
        /// Updates progress on progress bar by incrementing step
        /// </summary>
        protected void ProgressReportUpdate()
        {
            progressBar.PerformStep();
        }

        /// <summary>
        /// Resets the progress bar
        /// </summary>
		protected void ProgressReportEnd()
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = 0;
            progressBar.Step = 0;
        }

		/// <summary>
		/// On Exit of the main MDI window
		/// </summary>
		protected virtual void OnExit()
		{
			this.Close();
		}
        
		/// <summary>
		/// Called when Option settings are changed by the user.
		/// </summary>
		protected virtual void OnOptionsChanged()
		{
		}

		/// <summary>
		///  Sets the window title. The title constists of two parts:
        /// FormName and objectName
		/// </summary>
		/// <param name="objectName">The object's name</param>
		protected void SetWindowTitle(string objectName)
		{
            string windowTitle = string.Empty;
            if (!string.IsNullOrEmpty(FormName))
            {
                windowTitle = FormName;
            }
			if (!string.IsNullOrEmpty(objectName))
			{
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    windowTitle  +=  " - " ;
                }
				//windowTitle  += Util.InsertInSquareBrackets(Localization.LocalizeString(objectName));
                windowTitle += Util.InsertInSquareBrackets(objectName);
			}
            if (!string.IsNullOrEmpty(windowTitle))
            {
                this.Text = windowTitle;
            }

		}

        /// <summary>
        /// Sets the window title 
        /// </summary>
		protected virtual void SetWindowTitle()
		{
			SetWindowTitle(string.Empty);
		}

		/// <summary>
		/// Updates window title
		/// </summary>
		protected void UpdateWindowTitle()
		{
			SetWindowTitle();
		}

        /// <summary>
        /// Wires up event handlers for communication between the main form and the dialog
        /// </summary>
        /// <param name="dlg"></param>
        public void WireUpEventHandlers(DialogBase dlg)
        {
            dlg.BeginBusyEvent += new BeginBusyEventHandler(BeginBusy);
            dlg.UpdateStatusEvent += new UpdateStatusEventHandler(UpdateStatus);
            dlg.EndBusyEvent += new EndBusyEventHandler(EndBusy);

            dlg.ProgressReportBeginEvent += new ProgressReportBeginEventHandler(ProgressReportBegin);
            dlg.ProgressReportUpdateEvent += new ProgressReportUpdateEventHandler(ProgressReportUpdate);
            dlg.ProgressReportEndEvent += new SimpleEventHandler(ProgressReportEnd);
        }
		
		#endregion Protected Methods
       
        #region Event Handlers

        /// <summary>
		/// Handles the load event of the form
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void MainForm_Load(object sender, System.EventArgs e)
		{
			if (!this.DesignMode)
			{
                ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);
                // this.tsslLocale.Text = Thread.CurrentThread.CurrentUICulture.NativeName;
                this.tsslLocale.Text = Thread.CurrentThread.CurrentUICulture.Name;
                this.tsslVersion.Text = appId.Version;
                this.tsslCaps.Enabled = base.IsKeyToggled((int)Keys.CapsLock);
                this.tsslNUM.Enabled = base.IsKeyToggled((int)Keys.NumLock);
                this.tsslINS.Enabled = base.IsKeyToggled((int)Keys.Insert);

                this.KeyUp += new KeyEventHandler(OnKeyUp);
                ShowHideStatusStrip();
				UpdateWindowTitle();
			}
		}

		/// <summary>
		/// Handles the click event of the About Epi Info menu item
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void mniAboutEpiInfo_Activate(object sender, System.EventArgs e)
		{
            OnAboutClicked();
		}

		/// <summary>
		/// Close the form when Exit is clicked.
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void mniExit_Activate(object sender, System.EventArgs e)
		{
			OnExit();
		}

		/// <summary>
		/// Handles the click event of Options menu item
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void mniOptions_Activate(object sender, System.EventArgs e)
		{
            OnOptionsClicked();
		}

        /// <summary>
        /// Handles the Click event of the About Epi Info menu item
        /// </summary>
		protected void OnAboutClicked()
        {
            AboutEpiInfoDialog dlg = new AboutEpiInfoDialog(this);
			dlg.ShowDialog();
        }

        /// <summary>
        /// Sets the form state based on the keys that were pressed by the user
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.CapsLock:
                    this.tsslCaps.Enabled = base.IsKeyToggled((int)Keys.CapsLock);                    
                    break;

                case Keys.NumLock:
                    this.tsslNUM.Enabled = base.IsKeyToggled((int)Keys.NumLock);                    
                    break;

                case Keys.Insert:
                    this.tsslINS.Enabled = base.IsKeyToggled((int)Keys.Insert);                    
                    break;

                default:
                    break;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool retval = base.ProcessCmdKey(ref msg, keyData);
            if ((keyData == Keys.CapsLock) || (keyData == Keys.NumLock) || (keyData == Keys.Insert))
            {
                OnKeyUp(this, new KeyEventArgs(keyData));
            }
            return retval;
        }

        /// <summary>
        /// Handles the Click event of the Options menu item
        /// </summary>
		protected void OnOptionsClicked()
        {
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.General);
        }

        /// <summary>
        /// Handles the Click event of the Options menu item
        /// </summary>
        /// <param name="tabIndex">The index of the options dialog tab </param>
		protected void OnOptionsClicked(OptionsDialog.OptionsTabIndex tabIndex)
        {
			OptionsDialog dlg = new OptionsDialog(this, tabIndex);
            try
            {
                dlg.ApplyChanges += new OptionsDialog.ApplyChangesHandler(OnApplyChanges);
                DialogResult result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    OnOptionsChanged();
                }
            }
            finally
            {
                dlg.Close();
                dlg.Dispose();
            }
        }

        /// <summary>
        /// On Apply Changes
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnApplyChanges(object sender, EventArgs args)
        {
            OnOptionsChanged();
        }

        /// <summary>
        /// Handles the Click event of the Locale label on the form's status bar
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsslLocale_Click(object sender, EventArgs e)
        {
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.Language);
        }

        /// <summary>
        /// Handles the Import Started event
        /// </summary>
        /// <param name="o">Object that fired the event</param>
        /// <param name="e">Import Started event parameters</param>
        public void UpgradeProjectManager_ImportStarted(object o, ImportStartedEventArgs e)
        {
            ProgressReportBegin(0, e.ObjectCount, 1);
        }

        /// <summary>
        /// Handles the messaging of the Import's status
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Import status event parameters</param>
        public void UpgradeProjectManager_ImportStatus(object sender, MessageEventArgs e)
        {
            UpdateStatus(e.Message, e.CreateLogEntry);
        }

        /// <summary>
        /// Handles the Import Ended event
        /// </summary>
        public void UpgradeProjectManager_ImportEnded()
        {
            ProgressReportEnd();
        }

        /// <summary>
        /// Handles the reporting of the Import progress of a view 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">View Import event parameters</param>
        public void UpgradeProjectManager_ViewImported(object sender, EventArgs e)
        {
            ProgressReportUpdate();
        }

        /// <summary>
        /// Handles the reporting of the Import progress
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Import event parameters</param>
        public void UpgradeProjectManager_ProgressReportUpdate(object sender, EventArgs e)
        {
            ProgressReportUpdate();
        }
        
        /// <summary>
        /// Hides / shows status bar.
        /// </summary>
        protected void OnViewStatusBarClicked(ToolStripMenuItem menuItem)
        {            
            // Toggle the checked state
            menuItem.Checked = !(menuItem.Checked);            

            // Update configuration
            Configuration config = Configuration.GetNewInstance();
            config.Settings.ShowStatusBar = menuItem.Checked;
            Configuration.Save(config);

            // Show or hide status strip depending on the setting.
            ShowHideStatusStrip();
        }

        #endregion Event Handlers

        #region Private Methods
        private void ShowHideStatusStrip()
        {
            Configuration Config = Configuration.GetNewInstance();
            this.statusStrip1.Visible = Config.Settings.ShowStatusBar;
        }
        #endregion Private Methods
    }
}