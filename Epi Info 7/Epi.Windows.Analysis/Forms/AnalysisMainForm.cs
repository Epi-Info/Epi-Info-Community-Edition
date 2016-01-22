#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Epi.Analysis;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Docking;
using Epi.Windows.Analysis;
using System.Xml;
using Epi.Core.AnalysisInterpreter;

#endregion

namespace Epi.Windows.Analysis.Forms
{
    /// <summary>
    /// The main Analysis window
    /// </summary>
    public partial class AnalysisMainForm : Epi.Windows.MainForm
    {

        #region Private Attributes

        private CommandExplorer commandExplorer;
        private OutputWindow outputWindow;
        private ProgramEditor programEditor;
        //private EpiInterpreterParser EIParser;
        private EpiInterpreterParser mEpiInterpreter;
        private Epi.Project currentProject;

        private string commandText;
        private System.ComponentModel.BackgroundWorker CurrentBackGroundWorker;


        #endregion Private Attributes

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AnalysisMainForm()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);
            InitializeComponent();
            Construct();
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mod">Module</param>
        public AnalysisMainForm(AnalysisWindowsModule mod)
            : base(mod)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);
            InitializeComponent();
            Construct();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Processes the results of the command - PostProcess is necessary
        /// </summary>
        /// <param name="results"></param>
        public void ProcessCommandResults(CommandProcessorResults results)
        {
            if (results != null)
            {
                PostProcess(results);
                // 12/09/2008 Don't show  gridtable in output window.
                if (!results.Actions.Contains(Action.GridTable) && !results.Actions.Contains(Action.Update))
                {
                    //ROUTEOUT & CLOSEOUT commands do not have HTML output. They update the Output file.
                    if (results.Actions.Contains(Action.OutputFileName))
                        if ((results.HtmlOutput.Equals(string.Empty)))
                            outputWindow.SetOutputFile(results.FileNameOutput);

                    outputWindow.SendToOutput(results);
                }
            }
        }

        /// <summary>
        /// Sets the project host's current project
        /// </summary>
        public Project GetHostCurrentProject()
        {
            IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            if (host != null)
            {
                return host.CurrentProject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the project host's current project
        /// </summary>
        public void SetHostCurrentProject(Project project)
        {
            IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            if (host != null)
            {
                host.CurrentProject = project;
            }
            else
            {
                throw new GeneralException("Project host is not registered.");
            }
        }

        /// <summary>
        /// Shows or Hides all Analysis Tool Windows
        /// </summary>
        /// <param name="show">Set visibility state of Tool Windows.</param>
        public void ToggleToolWindows(bool show)
        {
            programEditor.IsVisible = show;
            commandExplorer.IsVisible = show;
            if (show)
            {
                dockManager1.DockWindow(commandExplorer, DockStyle.Left);
                dockManager1.DockWindow(programEditor, DockStyle.Bottom);
            }
        }
        #endregion Public Methods

        #region Public Properties
        /// <summary>
        /// Returns AnalysisEngine reference
        /// </summary>
        public new AnalysisWindowsModule Module
        {
            get
            {
                return (AnalysisWindowsModule)base.Module;
            }
        }

        /// <summary>
        ///  returns the MemoryRegion
        /// </summary>
        public IMemoryRegion MemoryRegion
        {
            get
            {
                return this.EpiInterpreter.Context.MemoryRegion;
            }
        }

        /// <summary>
        ///  returns the EpiInterpreter
        /// </summary>
        public EpiInterpreterParser EpiInterpreter
        {
            get
            {
                return this.mEpiInterpreter;
            }

            set { this.mEpiInterpreter = value; }

        }

        public ProgramEditor ProgramEditor
        {
            get { return this.programEditor; }
        }

        public Project CurrentProject
        {
            get { return this.currentProject; }
            set { this.currentProject = value; }
        }

        #endregion Public Properties

        #region Private Methods

        private void Construct()
        {
            DockManager.FastMoveDraw = false;
            DockManager.Style = DockVisualStyle.VS2005;

            commandExplorer = new CommandExplorer(this);
            commandExplorer.AllowClose = false;
            commandExplorer.AllowUnDock = false;
            commandExplorer.HideOnClose = true;
            commandExplorer.CommandGenerated += new CommandGenerationEventHandler(commandExplorer_CommandGenerated);
            outputWindow = new OutputWindow(this);
            outputWindow.AllowClose = false;
            outputWindow.AllowUnDock = false;
            outputWindow.IndeterminateTaskStarted += new BeginBusyEventHandler(outputWindow_IndeterminateTaskStarted);
            outputWindow.IndeterminateTaskEnded += new EndBusyEventHandler(outputWindow_IndeterminateTaskEnded);
            programEditor = new ProgramEditor(this);
            programEditor.AllowClose = false;
            programEditor.AllowUnDock = false;
            programEditor.HideOnClose = true;
            programEditor.RunCommand += new RunCommandEventHandler(programEditor_RunCommand);
            programEditor.RunPGM += new RunPGMEventHandler(programEditor_RunPGM);

            dockManager1.DockWindow(outputWindow, DockStyle.Fill);
            dockManager1.DockWindow(commandExplorer, DockStyle.Left);
            dockManager1.DockWindow(programEditor, DockStyle.Bottom);
            //Reduction.MemoryRegion = new MemoryRegion();
            
            
            //Module.Processor.ProcessResultsHandler = new ProcessCommandResultsHandler(ProcessCommandResults);
            //this.EpiInterpreter = new EpiInterpreterParser(Epi.Resources.ResourceLoader.GetCompiledGrammarTable(), (IAnalysisCheckCode)outputWindow, Rule_Context.eRunMode.Analysis);
            this.mEpiInterpreter = new EpiInterpreterParser((IAnalysisCheckCode)outputWindow.eventQueue);
        }

        void outputWindow_IndeterminateTaskEnded()
        {
            UpdateStatus(SharedStrings.READY, false);
            EndIndeterminate();
        }

        void outputWindow_IndeterminateTaskStarted(string message)
        {
            UpdateStatus(message, false);
            BeginIndeterminate();
        }

        
        /// <remarks>
        /// Numeric, date and Yes/no variables values are assigned without quotes.
        /// If an assignment is made to nothing (not even empty quotes) make is missing
        /// </remarks>
        /// <param name="el"></param>
        private void ExecAssign(XmlElement el)
        {
            string name = el.GetAttribute("VarName");
            string value = el.GetAttribute("VarValue");
            /*
            Epi.Commands.AssignCommand cmd = new Epi.Commands.AssignCommand(Module.Processor);
            if (cmd != null)
            {
                cmd.Execute(name, value);
            }*/
            //StringBuilder sb = new StringBuilder("ASSIGN ");
            //string doubleQuote = el.GetAttribute("DoubleQuote");
            //sb.Append(el.GetAttribute("VarName")).Append(StringLiterals.EQUAL);
            //string value = doubleQuote + el.GetAttribute("VarValue") + doubleQuote;
            //sb.Append( (string.IsNullOrEmpty(value)) ? StringLiterals.EPI_REPRESENTATION_OF_MISSING : value);
            //Epi.Analysis.Commands.AssignCommandSimple cmd;
            // DONE: make a generic assign statement that just takes a name and a variable.
            // Itshould not be done he, but must be done in business logic.
            //cmd = new Epi.Analysis.Commands.AssignCommandSimple(sb.ToString(), Module.Processor);
            //Module.Processor.RunCommand(sb.ToString());
        }

        /// <summary>
        /// Method to run a single command
        /// </summary>
        /// <param name="commandText">Command to run</param>
        internal void RunCommand(string pCommandText)
        {
            try
            {
                if (!String.IsNullOrEmpty(pCommandText))
                {
                    this.commandText = pCommandText.Trim();

                    this.CurrentBackGroundWorker = new System.ComponentModel.BackgroundWorker();
                    this.CurrentBackGroundWorker.WorkerSupportsCancellation = true;
                    this.CurrentBackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                    this.CurrentBackGroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                    this.CurrentBackGroundWorker.RunWorkerAsync();

                }
            }
            catch (ParseException args)
            {
                string message = string.Format(
                                    "Parse error caused by token: {0}",
                                    args.UnexpectedToken.Text);
                this.programEditor.ShowErrorMessage(message);
                this.programEditor.SetCursor(args);
            }
            catch (Exception ex)
            {
                this.programEditor.ShowErrorMessage(ex.ToString());
            }
        }


        /// <summary>
        /// Method to cancel running commands
        /// </summary>
        internal void CancelRunningCommand()
        {
            if (this.CurrentBackGroundWorker != null)
            {
                try
                {
                    this.CurrentBackGroundWorker.CancelAsync();
                }
                catch (Exception ex)
                {
                    // do nothing for now
                }
            }

            this.BeginInvoke(new MethodInvoker(delegate()
            {
                this.programEditor.txtTextArea.Enabled = true;
                this.programEditor.btnRun.Enabled = true;
                this.programEditor.btnCancelRunningCommands.Enabled = false;
                this.programEditor.txtTextArea.SelectionStart = 0;
                this.programEditor.txtTextArea.SelectionLength = 0;
                this.UpdateStatus("Ready");
            }));
        }

        /// <summary>
        /// Executes output actions specified in Analysis output.
        /// </summary>
        /// <param name="results"></param>
        private void PostProcess(Epi.CommandProcessorResults results)
        {
            if (results != null)
            {
                foreach (Action actionId in results.Actions)
                {
                    switch (actionId)
                    {
                        case Action.Beep:
                            Helpers.MessageBeep(0x0);
                            break;
                        case Action.Quit:
                            OnExit();
                            break;
                        case Action.GridTable:
                        case Action.Update:
                            DataGridForm gridForm = new DataGridForm(this);
                            dockManager1.DockWindow(gridForm, DockStyle.Fill);
                            gridForm.SendToOutput(results, actionId);
                            gridForm.TopMost = true;
                            gridForm.ReadOnly = (actionId == Action.GridTable);
                            break;
                        case Action.SimpleDialog:
                            {
                                UserDialog dlg = new UserDialog(this, results.XmlOutput);
                                /*void*/
                                dlg.ShowDialog(this);
                            }
                            break;
                        case Action.UserDialog:
                            {
                                using (UserDialog dlg = new UserDialog(this, results.XmlOutput))
                                {
                                    if (dlg.ShowDialog(this) != DialogResult.Cancel)
                                    {
                                        ExecAssign(results.XmlOutput.DocumentElement);
                                    }
                                }
                            }
                            break;
                        case Action.FileOpenDialog:
                        case Action.DatabasesDialog:
                            {
                                FileDialog dlg = new OpenFileDialog();
                                dlg.Title = results.XmlOutput.DocumentElement.GetAttribute("Title");
                                dlg.Filter = "All files (*.*)|*.*";
                                if (actionId == Action.DatabasesDialog)
                                {
                                    dlg.Filter = "Access Databases (*.mdb)|*.mdb|" + dlg.Filter;
                                }

                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    results.XmlOutput.DocumentElement.SetAttribute("VarValue", dlg.FileName);
                                    ExecAssign(results.XmlOutput.DocumentElement);
                                }
                                dlg.Dispose();
                            }
                            break;
                        case Action.FileSaveDialog:
                            {
                                FileDialog dlg = new SaveFileDialog();
                                dlg.Title = results.XmlOutput.DocumentElement.GetAttribute("Title");
                                dlg.ShowDialog(this);
                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    results.XmlOutput.DocumentElement.SetAttribute("VarValue", dlg.FileName);
                                    ExecAssign(results.XmlOutput.DocumentElement);
                                }
                                dlg.Dispose();
                            }
                            break;
                        case Action.OutTable:
                            {
                                try
                                {
                                    if (results.XmlOutput == null)
                                    {
                                        IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
                                        Project currentProject = host.CurrentProject;

                                        DataTable outTable = results.DsOutput.Tables["OutTable"];

                                        if (outTable != null)
                                        {
                                            if (currentProject.CollectedData.TableExists(results.OutTableName))
                                            {
                                                currentProject.CollectedData.DeleteTable(results.OutTableName);
                                            }
                                            currentProject.CollectedData.CreateTable(results.OutTableName, outTable);
                                        }
                                    }
                                }
                                catch (NotImplementedException ex)
                                {
                                    Epi.Windows.MsgBox.ShowError(ex.Message);
                                }
                            }
                            break;
                        case Action.Print:
                            outputWindow.Printout();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region Event Handlers

        private void programEditor_RunPGM(string command)
        {
            try
            {
                UpdateStatus("Running PGM...", false);
                this.EpiInterpreter.Context.ClearState();
                RunCommand(command.Trim());
            }
            catch (Exception ex)
            {

            }
            finally
            {
                UpdateStatus("Ready", false);
            }

        }

        private void programEditor_RunCommand(string command)
        {
            try
            {
                UpdateStatus("Running command...", false);
                this.EpiInterpreter.Context.ResetWhileSelected();
                this.EpiInterpreter.Context.SetOneCommandMode();
                RunCommand(command.Trim());
            }
            finally
            {
                UpdateStatus("Ready", false);
            }
            
   
        }

        private void commandExplorer_CommandGenerated(string commandSyntax, CommandProcessingMode processingMode)
        {
            programEditor.AddCommand(commandSyntax);
            programEditor.Focus();
            if (processingMode == CommandProcessingMode.Save_And_Execute)
            {
                this.EpiInterpreter.Context.SetOneCommandMode();
                this.EpiInterpreter.Context.ResetWhileSelected();
                try
                {
                    RunCommand(commandSyntax);

                }
                catch (Exception ex)
                {

                }
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnOptionsClicked();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnExit();
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnViewStatusBarClicked(sender as ToolStripMenuItem);
        }

        private void epiInfoLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WinUtil.OpenTextFile(Logger.GetLogFilePath());
        }

        private void aboutEpiInfo7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnAboutClicked();
        }

        private void Form_Load(object sender, EventArgs e)
        {

        }


        public void LoadProgramFromCommandLine(string programPath)
        {
            programEditor.LoadProgramFromCommandLine(programPath);
        }

        /// <summary>
        /// Wires up event handlers for communication between the main form and the dialog
        /// </summary>
        /// <param name="dlg"></param>
        public void WireUpEventHandlers(Epi.Windows.Analysis.Dialogs.DialogBase dlg)
        {
            dlg.BeginBusyEvent += new BeginBusyEventHandler(BeginBusy);
            dlg.UpdateStatusEvent += new UpdateStatusEventHandler(UpdateStatus);
            dlg.EndBusyEvent += new EndBusyEventHandler(EndBusy);

            dlg.ProgressReportBeginEvent += new ProgressReportBeginEventHandler(ProgressReportBegin);
            dlg.ProgressReportUpdateEvent += new ProgressReportUpdateEventHandler(ProgressReportUpdate);
            dlg.ProgressReportEndEvent += new SimpleEventHandler(ProgressReportEnd);
        }

        #endregion Event Handlers

        private void AnalysisMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.mEpiInterpreter.Context.ClearState();
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }
            
                try
                {
                    this.BeginInvoke(new MethodInvoker(delegate() 
                    {
                        this.programEditor.txtTextArea.Enabled = true;
                        ///////put focus back on txtTextArea: Issue 442/////////////////////////////////
                     //   this.programEditor.txtTextArea.Focus();

                        this.programEditor.SetLineFocus();
                       
                       ///////put focus back on txtTextArea/////////////////////////////////
                        this.programEditor.btnRun.Enabled = true;
                        this.programEditor.btnCancelRunningCommands.Enabled = false;
                        //this.programEditor.txtTextArea.SelectionStart = 0;
                        //this.programEditor.txtTextArea.SelectionLength = 0;
                        this.UpdateStatus("Ready");
                    }));
                  
                  
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new MethodInvoker(delegate()
                   {
                       this.programEditor.ShowErrorMessage(ex.ToString());
                       this.UpdateStatus("Ready");
                   }));
                }
            

            if (this.CurrentBackGroundWorker != null)
            {
                this.CurrentBackGroundWorker = null;
            }
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.programEditor.txtTextArea.Enabled = false;
                    this.programEditor.btnRun.Enabled = false;
                }));

                this.EpiInterpreter.Execute(this.commandText);

                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.UpdateStatus("Ready");
                }));

            }
            catch (GeneralException ex)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.programEditor.ShowErrorMessage(ex.Message);
                    this.UpdateStatus("Ready");
                }));
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.programEditor.ShowErrorMessage(ex.ToString());
                    this.UpdateStatus("Ready");
                }));
            }
            finally
            {
                if (this.CurrentBackGroundWorker != null)
                {
                   this.CurrentBackGroundWorker = null;
                }

            }
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/classic-analysis/Introduction.html");
        }
    }

    #region Internal Class

    /// <summary>
    /// Helper class
    /// </summary>
    internal class Helpers
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]

        public static extern int MessageBeep(uint n);
    }

    #endregion





}
