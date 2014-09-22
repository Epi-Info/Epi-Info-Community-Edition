using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Windows;

namespace Epi.Windows.Dialogs
{
	/// <summary>
	/// Base class for all dialogs
	/// </summary>
    public partial class DialogBase : FormBase
	{
        #region Constructors

        /// <summary>
        /// Default Constructor for exclusive use by the designer.
        /// </summary>        
        public DialogBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor Initializes the dialog and sets the link to main form.
        /// </summary>
        /// <param name="frm">The main form</param>          
        public DialogBase(MainForm frm)
        {
            #region Input Validation
            if (frm == null)
            {
                //throw new ArgumentNullException("frm");
            }
            #endregion Input Validation

            InitializeComponent();
            errorMessages = new List<string>();
            if (frm != null)
            {
                //TODO: FieldDefinitionDialogFactory does not send MainForms to this constructor
                this.mainForm = frm;
                mainForm.WireUpEventHandlers(this);
            }
        }
        #endregion Constructors

        #region Private Attributes
        private List<string> errorMessages = new List<string>();
        private bool isInWizard = false;
        /// <summary>
        /// a reference to the Main Form
        /// </summary>
        protected  MainForm mainForm = null; //zack changed 8/8/08
        #endregion Private Attributes        
		
		#region Protected Class Members
		#endregion

        #region Protected Properties

        /// <summary>
        /// Gets the list of error messages
        /// </summary>
        protected List<string> ErrorMessages
        {
            get
            {
                return errorMessages;
            }
        }

        #endregion Protected Properties

        #region Protected Methods

        /// <summary>
        /// Checks whether input on the dialog is valid
        /// </summary>
        /// <returns>Boolean</returns>
        protected bool IsInputValid()
        {
            return ValidateInput();
        }

        /// <summary>
        /// Validates the input on the dialog
        /// </summary>
        /// <returns>Boolean</returns>
        protected virtual bool ValidateInput()
        {
            errorMessages.Clear();
            return (errorMessages.Count == 0); // Always returns true
        }

        /// <summary>
        /// Checks whether input is sufficient
        /// </summary>
        public virtual void CheckForInputSufficiency()
        {
        }

        /// <summary>
        /// Method to show error messages
        /// </summary>
        protected void ShowErrorMessages()
        {
            string errorMessagesString = String.Empty;
            foreach (string str in ErrorMessages)
            {
                if (!String.IsNullOrEmpty(errorMessagesString))
                {
                    errorMessagesString += Environment.NewLine;
                }
                errorMessagesString += str;
            }

            if (!String.IsNullOrEmpty(errorMessagesString))
            {
                MsgBox.ShowError(errorMessagesString);
            }

            ErrorMessages.Clear();
        }

        /// <summary>
        /// Raises BeginBusy event
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        protected void BeginBusy(string message)
        {
            if (this.BeginBusyEvent != null)
            {
                BeginBusyEvent(message);
            }
        }

        /// <summary>
        /// Raises EndBusyEvent
        /// </summary>
        protected void EndBusy()
        {
            if (this.EndBusyEvent != null)
            {
                EndBusyEvent();
            }
        }

        /// <summary>
        /// Raises UpdateStatusEvent
        /// </summary>
        /// <param name="message">The message to be raised</param>
        protected void UpdateStatus(string message)
        {
            if (this.UpdateStatusEvent != null)
            {
                UpdateStatusEvent(message);
            }
        }

        /// <summary>
        /// Raises the ProgressReportBeginEvent
        /// </summary>
        /// <param name="min">Minimum value for progress bar's progress range</param>
        /// <param name="max">Maximum value for progress bar's progress range</param>
        /// <param name="step">The increment which increases the current position of the progress bar</param>
        protected void ProgressReportBegin(int min, int max, int step)
        {
            if (this.ProgressReportBeginEvent != null)
            {
                ProgressReportBeginEvent(min, max, step);
            }
        }

        /// <summary>
        /// Raises the ProgressReportEndEvent
        /// </summary>
        protected void ProgressReportEnd()
        {
            if (this.ProgressReportEndEvent != null)
            {
                ProgressReportEndEvent();
            }
        }

        /// <summary>
        /// Raises the ProgressReportUpdateEvent
        /// </summary>
        protected void ProgressReportUpdate()
        {
            if (this.ProgressReportUpdateEvent != null)
            {
                ProgressReportUpdateEvent();
            }
        }

        /// <summary>
        ///  Allows the user to select a project
        /// <returns>the full path of a project file</returns>
        /// </summary>
        protected virtual string SelectProjectPath()
        {
            string path = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = SharedStrings.SELECT_PROJECT;
            openFileDialog.Filter = "Epi7 " + SharedStrings.PROJECT_FILE + " ( *" + Epi.FileExtensions.EPI_PROJ + ")|*" + Epi.FileExtensions.EPI_PROJ;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName.Trim();
            }
            return path;
        }



        /// <summary>
        ///  Allows the user to select a project
        /// <returns>Instance of project, null if error</returns>
        /// </summary>
        // TODO: dcs0 - the actual change of the currentproject SHOULD NOT be in the UI.
        //              This select project should just get the path name.  Business logic should
        //              create the project
        protected virtual Project SelectProject()
        {
            try
            {
                Project project = null;

                //OpenFileDialog openFileDialog = new OpenFileDialog();
                //openFileDialog.Title = "Select a project";
                //openFileDialog.Filter = "Epi7 Project Files (*.prj)|*.prj";
                // dcs0 7/10/2008 we don't support other than .prj at this time
                //      so don't let the user think he can open something else.
                //openFileDialog.Filter = "Epi2000 and Epi7 Project Files (*.mdb;*.prj)|" +
                //    "*.mdb;*.prj|" +
                //    "Epi2000 Project files (*.mdb)|*.mdb |" +
                //    "Epi7 Project Files (*.prj)|*.prj";
                //openFileDialog.FilterIndex = 1;
                //openFileDialog.Multiselect = false;
                //DialogResult result = openFileDialog.ShowDialog();
                string projectFilePath = this.SelectProjectPath();
                if (!string.IsNullOrEmpty(projectFilePath))
                //if (result == DialogResult.OK)
                //{
                //    string projectFilePath = openFileDialog.FileName.Trim();
                //    if (System.IO.File.Exists(projectFilePath))
                {
                    IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
                    if (manager == null)
                    {
                        throw new GeneralException("Project manager is not registered.");
                    }
                    if (projectFilePath.EndsWith(".mdb"))
                    {
                        MessageBox.Show("The functionality to process MS Access files will be available in a future release.", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        project = manager.OpenProject(projectFilePath);
                        if (project == null)
                        {
                            MsgBox.ShowError(SharedStrings.INVALID_PROJECT_FILE);
                        }
                    }
                }
                //else
                //{
                //    MsgBox.ShowError(SharedStrings.CANNOT_OPEN_PROJECT_FILE +
                //                    SharedStrings.FILE_NOT_FOUND + ".\n" +
                //                    projectFilePath);
                //}
                //}
                return project;
            }
            finally
            {
            }
        }

        #endregion Protected Methods

        #region Public Properties
        /// <summary>
        /// Specifies if this dialog is part of a wizard.
        /// </summary>
        public bool IsInWizard
        {
            get
            {
                return isInWizard;
            }
            set
            {
                this.isInWizard = value;
            }
        }

        /// <summary>
        /// Shortcut to get the current Module
        /// </summary>
        public IWindowsModule Module
        {
            get
            {
                if (mainForm != null)
                {
                    return mainForm.Module;
                }
                else
                {
                    throw new GeneralException("Dialog main form could not be accessed.");
                    //return null; // TODO: Throw exception here
                }
            }
        }

        /// <summary>
        /// Retrieves the Main Form
        /// </summary>
        public MainForm MainForm
        {
            get
            {
                return mainForm;
            }
        }

        #endregion Public Properties

        #region Public Events

        /// <summary>
        /// Declaration for the Begin Busy event
        /// </summary>
        public event BeginBusyEventHandler BeginBusyEvent;

        /// <summary>
        /// Declaration for the End Busy event
        /// </summary>
        public event EndBusyEventHandler EndBusyEvent;

        /// <summary>
        /// Declaration for the Update Status Event
        /// </summary>
        public event UpdateStatusEventHandler UpdateStatusEvent;

        /// <summary>
        /// Declaration for the Progress Report Begin event
        /// </summary>
        public event ProgressReportBeginEventHandler ProgressReportBeginEvent;

        /// <summary>
        /// Declaration for the Progress Report Update event
        /// </summary>
        public event ProgressReportUpdateEventHandler ProgressReportUpdateEvent;

        /// <summary>
        /// Declaration for the Progress Report End event
        /// </summary>
        public event SimpleEventHandler ProgressReportEndEvent;
        #endregion Public Events
    }
}
