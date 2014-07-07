using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for DataDictionaryControl.xaml
    /// </summary>
    public partial class DataDictionaryControl : UserControl
    {
        #region Private Members
        /// <summary>
        /// The dashboard helper that is attached to this gadget; provides database access and many 
        /// method calls that are needed to populate variable lists and get variable types
        /// </summary>
        private DashboardHelper DashboardHelper { get; set; }

        /// <summary>
        /// The worker thread that the data processing is intended to run on; by using a separate thread,
        /// the user can continue working in the dashboard UI as the gadget is processing the line list.
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// The base worker thread, which calls the 'worker' thread. Required for gadget cancellation.
        /// </summary>
        private BackgroundWorker baseWorker;

        /// <summary>
        /// Required to 'lock' the worker thread so that you can't spawn more than one worker thread
        /// in the gadget at a single time.
        /// </summary>
        private object syncLock = new object();

        #endregion // Private Members

        #region Delegates

        private delegate void AddLineListGridDelegate(DataView dv);
        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void RequestUpdateStatusDelegate(string statusMessage);
        private delegate bool CheckForCancellationDelegate();

        private delegate void RenderFinishWithErrorDelegate(string errorMessage);
        private delegate void RenderFinishWithWarningDelegate(string errorMessage);
        private delegate void SimpleCallback();

        #endregion

        #region Events
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataDictionaryControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        /// <param name="hostedByEnter">Whether the control is hosted by Enter</param>
        public DataDictionaryControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        private void CopyToClipboard()
        {
            if (dgMain.ItemsSource != null && dgMain.Visibility == Visibility.Visible)
            {
                Common.CopyDataViewToClipboard(dgMain.ItemsSource as DataView);
            }
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            dgMain.Visibility = System.Windows.Visibility.Collapsed;
            dgMain.ItemsSource = null;
        }
        #endregion // Public Methods

        #region Event Handlers

        /// <summary>
        /// Fires the 'Close' button event that will remove the gadget from the dashboard
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseGadget();
        }

        /// <summary>
        /// Tells the gadget to begin processing output on a new worker thread
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Execute(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();                
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));     
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddLineListGridDelegate addGrid = new AddLineListGridDelegate(AddLineListGrid);

                Configuration config = DashboardHelper.Config;
                string yesValue = config.Settings.RepresentationOfYes;
                string noValue = config.Settings.RepresentationOfNo;

                try
                {
                    DataTable dictionaryTable = DashboardHelper.FieldTable.Copy();

                    foreach (KeyValuePair<string, string> kvp in DashboardHelper.TableColumnNames)
                    {
                        DataRow row = dictionaryTable.Rows.Find(kvp.Key);
                        if(row == null)
                        {
                            dictionaryTable.Rows.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (DashboardHelper.IsUsingEpiProject)
                    {
                        dictionaryTable.Columns.Add("Page", typeof(int));
                        dictionaryTable.Columns.Add("Tab", typeof(int));
                        dictionaryTable.Columns.Add("Prompt", typeof(string));
                        dictionaryTable.Columns.Add("Items", typeof(string));
                        dictionaryTable.Columns.Add("FieldType", typeof(string));

                        foreach (DataRow fieldRow in dictionaryTable.Rows)
                        {
                            if (fieldRow["epifieldtype"] is RenderableField)
                            {
                                RenderableField renderableField = fieldRow["epifieldtype"] as RenderableField;
                                fieldRow["Page"] = renderableField.Page.Position + 1;
                                fieldRow["Tab"] = renderableField.TabIndex;
                                fieldRow["Prompt"] = renderableField.PromptText;
                                fieldRow["FieldType"] = fieldRow["epifieldtype"].ToString().Replace("Epi.Fields.", String.Empty);
                                if (renderableField is GroupField)
                                {
                                    GroupField groupField = renderableField as GroupField;
                                    fieldRow["Items"] = groupField.ChildFieldNames;
                                }
                                else if (renderableField is OptionField)
                                {
                                    OptionField optionField = renderableField as OptionField;
                                    fieldRow["Items"] = optionField.GetOptionsString();
                                }
                            }
                            fieldRow["datatype"] = fieldRow["datatype"].ToString().Replace("System.", String.Empty);
                        }

                        dictionaryTable.Columns["columnname"].SetOrdinal(0);
                        dictionaryTable.Columns["Prompt"].SetOrdinal(1);
                        dictionaryTable.Columns["formname"].SetOrdinal(2);
                        dictionaryTable.Columns["Page"].SetOrdinal(3);
                        dictionaryTable.Columns["Tab"].SetOrdinal(4);
                        dictionaryTable.Columns["datatype"].SetOrdinal(5);
                        dictionaryTable.Columns["FieldType"].SetOrdinal(6);
                        dictionaryTable.Columns["tablename"].SetOrdinal(7);
                        dictionaryTable.Columns["Items"].SetOrdinal(8);
                    }

                    if (dictionaryTable == null || dictionaryTable.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "There are no valid fields to display."); 
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Data dictionary thread cancelled");
                        return;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(addGrid, dictionaryTable.AsDataView());
                        string formatString = string.Empty;
                    }

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));                    
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {   
                    stopwatch.Stop();
                    Debug.Print("Data dictionary gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this gadget</param>
        private void Construct()
        {
            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);
            
            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            mnuSendDataToHTML.Visibility = System.Windows.Visibility.Visible;
            mnuClose.Visibility = System.Windows.Visibility.Visible;            

            this.IsProcessing = false;            

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            RefreshResults();
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        private void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);
            
            mnuSendToBack.Click -= new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click -= new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click -= new RoutedEventHandler(mnuClose_Click);

            if (GadgetClosing != null)
                GadgetClosing(this);

            GadgetClosing = null;
            GadgetCheckForCancellation = null;
            GadgetProcessingFinished = null;
            GadgetStatusUpdate = null;
        }

        /// <summary>
        /// Sends the gadget to the back of the canvas
        /// </summary>
        private void SendToBack()
        {
            Canvas.SetZIndex(this, -1);
        }

        /// <summary>
        /// Used to add a new Line List grid to the gadget's output
        /// </summary>
        /// <param name="groupVar">The name of the group variable selected, if any</param>
        /// <param name="value">The value by which this grid has been grouped by</param>
        private void AddLineListGrid(DataView dv)
        {
            dgMain.ItemsSource = dv;

            if (System.Windows.SystemParameters.PrimaryScreenWidth <= 1024)
            {
                dgMain.MaxWidth = 900;
            }
            else if (System.Windows.SystemParameters.PrimaryScreenWidth > 1024 && System.Windows.SystemParameters.PrimaryScreenWidth <= 1280)
            {
                dgMain.MaxWidth = 1150;
            }
            else
            {
                dgMain.MaxWidth = Double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Checks the gadget's position on the screen and, if necessary, re-sets it so that it is visible. This is used in 
        /// scenarios where the user has moved the gadget to a specific position where its top and/or left positions are outside 
        /// of the canvas boundaries, adds a filter, and the subsequent gadget output is too short to roll onto the visible 
        /// canvas. The gadget is then hidden entirely from the user's view. This will re-set the gadget so that it is visible.
        /// This should only be called from the RenderFinish series of methods.
        /// </summary>
        private void CheckAndSetPosition()
        {
            double top = Canvas.GetTop(this);
            double left = Canvas.GetLeft(this);

            if (top < 0)
            {
                Canvas.SetTop(this, 0);
            }
            if (left < 0)
            {
                Canvas.SetLeft(this, 0);
            }
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        private void RenderFinish()
        {   
            waitCursor.Visibility = Visibility.Collapsed;
            dgMain.Visibility = System.Windows.Visibility.Visible;

            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed;
            dgMain.Visibility = System.Windows.Visibility.Visible;

            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void RenderFinishWithError(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed; 
            dgMain.Visibility = System.Windows.Visibility.Collapsed;

            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void RequestUpdateStatusMessage(string statusMessage)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
        }

        /// <summary>
        /// Used to sets the gadget's current status, e.g. "Processing results..." or "Displaying output..."
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void SetStatusMessage(string statusMessage)
        {
        }

        /// <summary>
        /// Used to check if the user took an action that effectively cancels the currently-running worker
        /// thread. This could include: Closing the gadget, refreshing the dashboard, or selecting another
        /// means variable from the list of variables.
        /// </summary>
        /// <returns>Bool indicating whether or not the gadget's processing thread has been cancelled by the user</returns>
        private bool IsCancelled()
        {
            if (worker != null && worker.WorkerSupportsCancellation && worker.CancellationPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IGadget Members

        /// <summary>
        /// Gets/sets whether the gadget is processing
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public void RefreshResults()
        {
            waitCursor.Visibility = Visibility.Visible;
            dgMain.Visibility = System.Windows.Visibility.Collapsed;

            baseWorker = new BackgroundWorker();
            baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
            baseWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            RefreshResults();
        }
        #endregion

        
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;

            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Data Dictionary</h2>");

            if (dgMain != null && dgMain.ItemsSource != null)
            {
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(dgMain.ItemsSource as DataView));
                htmlBuilder.AppendLine("</table>");
            }
            return htmlBuilder.ToString();
        }

        void mnuSendToBack_Click(object sender, RoutedEventArgs e)
        {
            SendToBack();
        }

        void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            CloseGadget();
        }

        /// <summary>
        /// Handles the click event for the 'Copy to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSendDataToHTML_Click(object sender, RoutedEventArgs e)
        {
            if (dgMain == null || dgMain.ItemsSource == null || !(dgMain.ItemsSource is DataView))
            {
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head><title>Data Dictionary</title>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(DashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML());
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = fileName;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }
    }
}
