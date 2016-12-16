using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport;
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private object selectedDataSource;
        private string selectedDataProvider;
        private bool controlNeedsRefresh = false;
        private IDbDriver db = null;
        private object syncLock = new object();
        private BackgroundWorker worker;
        private List<string> exportFields;
        private bool allFieldsSelected = true;
        private bool useTabOrder;
        private bool useFieldPrompts;
        private const string SEPARATOR = StringLiterals.COMMA;
        private bool loadingCombos = false;
        #endregion // Private Members

        #region Public Events
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetProgressUpdateHandler GadgetProgressUpdate;
        #endregion // Public Events

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);        
        private delegate bool CheckForCancellationDelegate();        
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view to attach</param>
        /// <param name="db">The database to attach</param>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public ExportWindow(DashboardHelper dashboardHelper)
        {
            InitializeComponent();

            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            messagePanel.HideProgressBar();

            //pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            //pnlError.Visibility = System.Windows.Visibility.Collapsed;
            //pnlProgress.Visibility = System.Windows.Visibility.Collapsed;

            this.dashboardHelper = dashboardHelper;
            lbxExportFields.SelectionChanged += new SelectionChangedEventHandler(lbxExportFields_SelectionChanged);
            FillComboboxes();
            exportFields = new List<string>();
            allFieldsSelected = true;

            cmbDataFormats.Items.Clear();
            this.GadgetProgressUpdate += new GadgetProgressUpdateHandler(RequestUpdateStatusMessage);

            foreach (Epi.DataSets.Config.DataDriverRow row in dashboardHelper.Config.DataDrivers)
            {
                if(row.Type == Configuration.PostgreSQLDriver || row.Type == Configuration.MySQLDriver)
                {
                    continue;
                }

                ComboBoxItem item = new ComboBoxItem();
                item.Content = row.DisplayName;                
                //cmbDataFormats.Items.Add(new ComboBoxItem(row.Type, row.DisplayName, null));
                cmbDataFormats.Items.Add(item);
            }

            cmbDataFormats.SelectionChanged += new SelectionChangedEventHandler(cmbDataFormats_SelectionChanged);

            if (dashboardHelper.IsUsingEpiProject)
            {
                checkboxTabOrder.IsEnabled = true;
                checkboxTabOrder.IsChecked = true;
                checkboxUsePrompts.IsEnabled = true;
            }
            else
            {
                checkboxTabOrder.IsEnabled = false;
                checkboxTabOrder.IsChecked = false;
                checkboxUsePrompts.IsEnabled = false;
                checkboxUsePrompts.IsChecked = false;
            }

            #region Translation
            this.Title = DashboardSharedStrings.EXPORT_WINDOW_TITLE;
            this.tblockOutputFormat.Text = DashboardSharedStrings.EXPORT_OUTPUT_FORMAT;
            this.tblockConnInfo.Text = DashboardSharedStrings.EXPORT_CONN_INFO;
            this.tblockDestinationTable.Text = DashboardSharedStrings.EXPORT_DEST_TABLE;
            this.tblockFields.Text = DashboardSharedStrings.EXPORT_FIELDS_TO_EXPORT;
            this.checkboxTabOrder.Content = DashboardSharedStrings.EXPORT_SORT_BY_TAB_ORDER;
            this.btnCancel.Content = DashboardSharedStrings.EXPORT_BUTTON_CLOSE;
            this.btnExport.Content = DashboardSharedStrings.EXPORT_BUTTON_EXPORT;
            this.btnConnectionBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            #endregion // Translation

            EnableDisableExportButton();
        }
        #endregion // Constructors

        private void cmbDataFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtConnectionInformation.Clear();
            cmbDestinationTable.Items.Clear();

            if (cmbDataFormats.SelectedIndex >= 0)
            {
                if (cmbDataFormats.SelectedItem.ToString().ToLowerInvariant().Contains("ascii") || cmbDataFormats.SelectedItem.ToString().ToLowerInvariant().Contains("csv file")) // CSV File
                {
                    tblockDestinationTable.Text = DashboardSharedStrings.EXPORT_DEST_FILENAME;
                }
                else
                {
                    tblockDestinationTable.Text = DashboardSharedStrings.EXPORT_DEST_TABLE;
                }
            }

            EnableDisableExportButton();
        }

        private void EnableDisableExportButton()
        {
            if (cmbDataFormats.SelectedIndex >= 0 && txtConnectionInformation.Text.Length > 0 && (cmbDestinationTable.Text.Length > 0 || (cmbDestinationTable.SelectedIndex >= 0)))
            {
                btnExport.IsEnabled = true;
            }
            else
            {
                btnExport.IsEnabled = false;
            }
        }

        private void lbxExportFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            tblockFieldCount.Text = string.Format(DashboardSharedStrings.EXPORT_FIELD_COUNT, lbxExportFields.SelectedItems.Count.ToString());
        }

        private void btnConnectionBrowse_Click(object sender, RoutedEventArgs e)
        {
            SetupOutputDataSource();
        }        

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Used to check if the user took an action that effectively cancels the currently-running worker
        /// thread.
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

        private void CheckForProblems()
        {
            //pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Hidden;

            if (cmbDestinationTable.SelectedItem != null)
            {
                foreach (String item in cmbDestinationTable.Items)
                {
                    if (item.Equals(cmbDestinationTable.SelectedItem.ToString()))
                    {
                        messagePanel.Visibility = System.Windows.Visibility.Visible;
                        messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
                        messagePanel.Text = string.Format(SharedStrings.DASHBOARD_EXPORT_WARNING, item);
                        //pnlTableOverwrite.Visibility = System.Windows.Visibility.Visible;
                        //txtTableOverwrite.Text = string.Format(SharedStrings.DASHBOARD_EXPORT_WARNING, item);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            loadingCombos = true;

            List<string> selectedFields = new List<string>();

            if (update)
            {
                foreach (string s in lbxExportFields.SelectedItems)
                {
                    selectedFields.Add(s);
                }
            }

            lbxExportFields.ItemsSource = null;
            lbxExportFields.Items.Clear();
            List<string> fieldNames = new List<string>();
            fieldNames = dashboardHelper.GetFieldsAsList();

            //foreach (DataColumn dc in dashboardHelper.DataSet.Tables[0].Columns)
            //{
            //    System.Diagnostics.Debug.Print(dc.ColumnName);
            //}

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }

            lbxExportFields.ItemsSource = fieldNames;

            if (update)
            {
                foreach (string s in selectedFields)
                {
                    lbxExportFields.SelectedItems.Add(s);
                }
            }
            else
            {
                lbxExportFields.SelectAll();
            }
            loadingCombos = false;
        }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void RequestUpdateStatusMessage(string statusMessage, double progress)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
            if (progress > 0)
            {
                this.Dispatcher.BeginInvoke(new SetProgressBarDelegate(SetProgressBarValue), progress);
            }
        }

        /// <summary>
        /// Used to set the gadget's current status, e.g. "Processing results..." or "Displaying output..."
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void SetStatusMessage(string statusMessage)
        {
            //txtProgress.Text = statusMessage;
            messagePanel.Text = statusMessage;
        }

        /// <summary>
        /// Used to display a warning
        /// </summary>
        /// <param name="warningMessage">The warning message to display</param>
        private void SetWarningMessage(string warningMessage)
        {
            if (string.IsNullOrEmpty(warningMessage))
            {
                messagePanel.Visibility = System.Windows.Visibility.Hidden;
                //pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                messagePanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
                messagePanel.HideProgressBar();
                //pnlTableOverwrite.Visibility = System.Windows.Visibility.Visible;
            }
            //txtTableOverwrite.Text = warningMessage;
            messagePanel.Text = warningMessage;
        }

        /// <summary>
        /// Used to display an error
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void SetErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                messagePanel.Visibility = System.Windows.Visibility.Hidden;
                //pnlError.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                messagePanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
                messagePanel.HideProgressBar();
                //pnlError.Visibility = System.Windows.Visibility.Visible;
                //pnlProgress.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (errorMessage.Equals("Numeric field overflow."))
            {
                errorMessage = SharedStrings.DASHBOARD_EXPORT_EXCEL_NUMERIC_FIELD_OVERFLOW_FAIL;
            }
            //txtError.Text = errorMessage;
            messagePanel.Text = errorMessage;
        }

        /// <summary>
        /// Used to set the value of the progress bar.
        /// </summary>
        /// <param name="progress">The progress value to use</param>
        private void SetProgressBarValue(double progress)
        {
            messagePanel.SetProgressBarValue(progress);
            //progressBar.Value = progress;
        }

        private void SetupOutputDataSource()
        {
            try
            {
                ComboBoxItem selectedPlugIn = cmbDataFormats.SelectedItem as ComboBoxItem;
                IDbDriverFactory dbFactory = null;

                string plugin = string.Empty;
                foreach (Epi.DataSets.Config.DataDriverRow row in dashboardHelper.Config.DataDrivers)
                {
                    if (row.Type == Configuration.PostgreSQLDriver || row.Type == Configuration.MySQLDriver)
                    {
                        continue;
                    }

                    string content = selectedPlugIn.Content.ToString();
                    if (content.Equals(row.DisplayName))
                    {
                        plugin = row.Type;
                    }
                }

                selectedDataProvider = plugin;
                bool isFlatFile = false;
                dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(plugin);
                if (dbFactory.ArePrerequisitesMet())
                {
                    DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                    db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

                    IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();
                    dialog.ShouldIgnoreNonExistance = true;

                    System.Windows.Forms.DialogResult result = ((System.Windows.Forms.Form)dialog).ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        //this.savedConnectionStringDescription = dialog.ConnectionStringDescription;
                        bool success = false;
                        db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();
                        DbDriverInfo dbInfo = new DbDriverInfo();
                        dbInfo.DBCnnStringBuilder = dbFactory.RequestNewConnection(db.DataSource);

                        if (db.ConnectionDescription.ToLowerInvariant().Contains("csv file:"))
                        {
                            isFlatFile = true;
                        }

                        if (!isFlatFile)
                        {
                            if (!db.CheckDatabaseExistance(db.ConnectionString, "", true))
                            {
                                dbFactory.CreatePhysicalDatabase(dbInfo);
                            }

                            try
                            {
                                success = db.TestConnection();
                            }
                            catch
                            {
                                success = false;
                                MessageBox.Show("Could not connect to selected data source.");
                            }
                        }
                        else
                        {
                            success = true;
                        }

                        if (success)
                        {
                            this.SelectedDataSource = db;
                            controlNeedsRefresh = true;
                            txtConnectionInformation.Text = db.ConnectionString;
                        }
                        else
                        {
                            this.SelectedDataSource = null;
                        }
                    }
                    else
                    {
                        this.SelectedDataSource = null;
                    }
                }
                else
                {
                    MessageBox.Show(dbFactory.PrerequisiteMessage, "Prerequisites not found");
                }

                if (selectedDataSource is IDbDriver)
                {
                    db = selectedDataSource as IDbDriver;
                    //this.txtDataSource.Text = db.ConnectionString;

                    if (!isFlatFile)
                    {
                        System.Collections.Generic.List<string> tableNames = db.GetTableNames();
                        foreach (string tableName in tableNames)
                        {
                            ComboBoxItem newItem = new ComboBoxItem();//tableName, tableName, tableName);
                            newItem.Content = tableName;
                            cmbDestinationTable.Items.Add(tableName);
                            //this.cmbDataTable.Items.Add(newItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex.Message);
            }
            finally
            {
                messagePanel.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        #region Event Handlers
        #endregion // Event Handlers

        #region Private Methods

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
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(datadriverWorker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(datadriverWorker_WorkerCompleted);
            }

            GadgetProgressUpdate = null;
            GadgetStatusUpdate = null;
        }

        #endregion // Private Methods

        #region Private Properties
        private object SelectedDataSource
        {
            get
            {
                return this.selectedDataSource;
            }
            set
            {
                selectedDataSource = value;
            }
        }

        private object SelectedDataProvider
        {
            get
            {
                return this.selectedDataProvider;
            }
        }

        private bool ControlNeedsRefresh
        {
            get
            {
                return this.controlNeedsRefresh;
            }
            set
            {
                this.controlNeedsRefresh = value;
            }
        }

        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void datadriverWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.Print("Background worker thread for export gadget was cancelled or ran to completion.");
            btnExport.IsEnabled = true;
            cmbDestinationTable.IsEnabled = true;
            cmbDataFormats.IsEnabled = true;
            btnConnectionBrowse.IsEnabled = true;
            messagePanel.SetProgressBarValue(0);
            messagePanel.HideProgressBar();

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            //if (worker != null)
            //{
            //    worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(datadriverWorker_DoWork);
            //    worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(datadriverWorker_WorkerCompleted);
            //}
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the file I/O worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void fileIOWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.Print("Background file I/O worker thread for export gadget was cancelled or ran to completion.");
            btnExport.IsEnabled = true;
            cmbDestinationTable.IsEnabled = true;
            cmbDataFormats.IsEnabled = true;
            btnConnectionBrowse.IsEnabled = true;
            messagePanel.SetProgressBarValue(0);
            messagePanel.HideProgressBar();

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            //if (worker != null)
            //{
            //    worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(datadriverWorker_DoWork);
            //    worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(datadriverWorker_WorkerCompleted);
            //}
        }

        /// <summary>
        /// Handles the DoWorker event for the file I/O worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void fileIOWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                try
                {
                    string fileName = (string)e.Argument;

                    SetGadgetStatusHandler requestUpdateStatus = new SetGadgetStatusHandler(RequestUpdateStatusMessage);
                    CheckForCancellationHandler checkForCancellation = new CheckForCancellationHandler(IsCancelled);

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    dashboardHelper.PopulateDataSet(); // the only reason to call this is to see if any new user-defined vars have been added and apply them.
                    List<Epi.Data.TableColumn> tcList = dashboardHelper.GetFieldsAsListOfEpiTableColumns(useTabOrder);
                    DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;

                    if (!allFieldsSelected)
                    {
                        List<Epi.Data.TableColumn> tcFilteredList = new List<Epi.Data.TableColumn>();
                        foreach (string columnName in exportFields)
                        {
                            foreach (Epi.Data.TableColumn tc in tcList)
                            {
                                if (tc.Name.Equals(columnName) && !tcFilteredList.Contains(tc))
                                {
                                    tcFilteredList.Add(tc);
                                }
                            }
                        }

                        tcList = tcFilteredList;
                    }

                    Epi.ImportExport.CSVExporter csvExporter = new Epi.ImportExport.CSVExporter(
                        dv,
                        tcList,
                        fileName);

                    if (dashboardHelper.IsUsingEpiProject)
                    {
                        csvExporter.IncludeDeletedRecords = false;
                    }

                    if (this.useTabOrder)
                    {
                        csvExporter.ColumnSortOrder = Epi.ImportExport.ColumnSortOrder.TabOrder;
                    }
                    else
                    {
                        csvExporter.ColumnSortOrder = Epi.ImportExport.ColumnSortOrder.None;
                    }

                    csvExporter.SetProgressBar += new SetProgressBarDelegate(CallbackIncrementProgressBar);
                    csvExporter.SetStatus += new UpdateStatusEventHandler(CallbackSetStatusMessage);
                    csvExporter.SetProgressAndStatus += new SetProgressAndStatusHandler(RequestUpdateStatusMessage);
                    csvExporter.CheckForCancellation += new CheckForCancellationHandler(IsCancelled);
                    if (dashboardHelper.IsUsingEpiProject)
                    {
                        csvExporter.AttachView(dashboardHelper.View);
                    }
                    csvExporter.Export();
                }

                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
                }
                finally
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.Print("File I/O Export thread finished in " + stopWatch.Elapsed.ToString());
                }
            }
        }

        /// <summary>
        /// Provides callback to the UI thread for incrementing the progress bar.
        /// </summary>
        /// <param name="value">The value to set.</param>
        private void CallbackIncrementProgressBar(double value)
        {
            this.Dispatcher.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), value);
        }

        /// <summary>
        /// Provides callback to the UI thread for setting the current status message.
        /// </summary>
        /// <param name="message">The message to send</param>
        private void CallbackSetStatusMessage(string message)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), message);
        }

        /// <summary>
        /// Increments the progress bar by a given value
        /// </summary>
        /// <param name="value">The value by which to increment</param>
        private void IncrementProgressBarValue(double value)
        {            
            //progressBar.Style = ProgressBarStyle.Continuous;
            //progressBar.Increment((int)value);
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void datadriverWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                try
                {
                    string tableName = (string)e.Argument;

                    SetGadgetStatusHandler requestUpdateStatus = new SetGadgetStatusHandler(RequestUpdateStatusMessage);
                    CheckForCancellationHandler checkForCancellation = new CheckForCancellationHandler(IsCancelled);

                    if (db.TableExists(tableName) && !db.ConnectionDescription.ToLowerInvariant().Contains("excel"))
                    {
                        db.DeleteTable(tableName);
                    }
                    else if (db.TableExists(tableName))
                    {
                        this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_EXCEL_TABLE_OVERWRITE_FAIL, tableName));
                        stopWatch.Stop();
                        return;
                    }

                    dashboardHelper.PopulateDataSet(); // the only reason to call this is to see if any new user-defined vars have been added and apply them.
                    List<Epi.Data.TableColumn> tcList = dashboardHelper.GetFieldsAsListOfEpiTableColumns(useTabOrder);
                    allFieldsSelected = false; // TODO: Fix?

                    if (allFieldsSelected)
                    {
                        if (tcList.Count <= 250)
                        {
                            db.CreateTable(tableName, tcList);
                            DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;
                            DataTable table = dv.ToTable(false);
                            if (useTabOrder)
                            {
                                dashboardHelper.OrderColumns(table, true);
                            }
                            System.Data.Common.DbDataReader dataReader = table.CreateDataReader();
                            db.InsertBulkRows("Select * From [" + tableName + "]", dataReader, requestUpdateStatus, checkForCancellation);
                        }
                        else
                        {
                            Dictionary<string, List<Epi.Data.TableColumn>> fieldTableDictionary = new Dictionary<string, List<Epi.Data.TableColumn>>();
                            int totalTablesNeeded = (tcList.Count / 250) + 1;
                            int tableNumber = 0;
                            List<Epi.Data.TableColumn> tableTcList = new List<Epi.Data.TableColumn>();
                            for (int i = 0; i < tcList.Count; i++)
                            {
                                if (i % 250 == 0)
                                {
                                    if (tableNumber != 0)
                                    {
                                        fieldTableDictionary.Add(tableName + tableNumber.ToString(), tableTcList);
                                    }
                                    tableTcList = new List<Epi.Data.TableColumn>();
                                    tableNumber++;
                                }

                                tableTcList.Add(tcList[i]);

                                if (i == tcList.Count - 1)
                                {
                                    fieldTableDictionary.Add(tableName + tableNumber.ToString(), tableTcList);
                                }
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                if (db.TableExists(kvp.Key))
                                {
                                    this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_WIDE_TABLE_FAIL, totalTablesNeeded.ToString()));
                                    return;
                                }
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                db.CreateTable(kvp.Key, kvp.Value);
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                List<string> exportTableFields = new List<string>();
                                foreach (Epi.Data.TableColumn tc in kvp.Value)
                                {
                                    exportTableFields.Add(tc.Name);
                                }

                                DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;
                                DataTable table = dv.ToTable(false, exportTableFields.ToArray());
                                if (useTabOrder)
                                {
                                    dashboardHelper.OrderColumns(table, true);
                                }
                                System.Data.Common.DbDataReader dataReader = table.CreateDataReader(); //dv.ToTable().CreateDataReader();
                                db.InsertBulkRows("Select * From [" + kvp.Key + "]", dataReader, requestUpdateStatus, checkForCancellation);
                            }
                        }
                    }
                    else
                    {
                        List<Epi.Data.TableColumn> tcFilteredListUnsorted = new List<Epi.Data.TableColumn>();

                        foreach (string columnName in exportFields)
                        {
                            foreach (Epi.Data.TableColumn tc in tcList)
                            {
                                if (tc.Name.Equals(columnName) && !tcFilteredListUnsorted.Contains(tc))
                                {
                                    tcFilteredListUnsorted.Add(tc);
                                }
                            }
                        }

                        List<Epi.Data.TableColumn> tcFilteredList = new List<Epi.Data.TableColumn>();

                        foreach (Epi.Data.TableColumn tc in tcList)
                        {
                            if (tcFilteredListUnsorted.Contains(tc))
                            {                            
                                tcFilteredList.Add(tc);
                            }
                        }

                        if (tcFilteredList.Count <= 250)
                        {
                            db.CreateTable(tableName, tcFilteredList);
                            DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;
                            DataTable table = dv.ToTable(false, exportFields.ToArray());
                            if (useTabOrder)
                            {
                                dashboardHelper.OrderColumns(table, true);
                            }
                            System.Data.Common.DbDataReader dataReader = table.CreateDataReader(); //dv.ToTable().CreateDataReader();
                            db.InsertBulkRows("Select * From [" + tableName + "]", dataReader, requestUpdateStatus, checkForCancellation);
                        }
                        else
                        {
                            Dictionary<string, List<Epi.Data.TableColumn>> fieldTableDictionary = new Dictionary<string, List<Epi.Data.TableColumn>>();
                            int totalTablesNeeded = (tcFilteredList.Count / 250) + 1;
                            int tableNumber = 0;
                            List<Epi.Data.TableColumn> tableTcList = new List<Epi.Data.TableColumn>();
                            for (int i = 0; i < tcFilteredList.Count; i++)
                            {
                                if (i % 250 == 0)
                                {
                                    if (tableNumber != 0)
                                    {
                                        foreach (Epi.Data.TableColumn tc in tcFilteredList)
                                        {
                                            if (tc.Name == "GlobalRecordId")
                                            {
                                                if (!tableTcList.Contains(tc))
                                                {
                                                    tableTcList.Add(tc);
                                                }
                                            }
                                        }

                                        fieldTableDictionary.Add(tableName + tableNumber.ToString(), tableTcList);
                                    }
                                    tableTcList = new List<Epi.Data.TableColumn>();
                                    tableNumber++;
                                }

                                tableTcList.Add(tcFilteredList[i]);

                                if (i == tcFilteredList.Count - 1)
                                {
                                    foreach (Epi.Data.TableColumn tc in tcFilteredList)
                                    {
                                        if (tc.Name == "GlobalRecordId")
                                        {
                                            if (!tableTcList.Contains(tc))
                                            {
                                                tableTcList.Add(tc);
                                            }
                                        }
                                    }

                                    fieldTableDictionary.Add(tableName + tableNumber.ToString(), tableTcList);
                                }
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                if (db.TableExists(kvp.Key))
                                {
                                    this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_WIDE_TABLE_FAIL, totalTablesNeeded.ToString()));
                                    return;
                                }
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                db.CreateTable(kvp.Key, kvp.Value);
                            }

                            foreach (KeyValuePair<string, List<Epi.Data.TableColumn>> kvp in fieldTableDictionary)
                            {
                                List<string> exportTableFields = new List<string>();
                                foreach (Epi.Data.TableColumn tc in kvp.Value)
                                {
                                    exportTableFields.Add(tc.Name);
                                }

                                DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;

                                if (!dashboardHelper.DataSet.Tables[0].Columns.Contains("FKEY") && exportTableFields.Contains("FKEY"))
                                {
                                    exportTableFields.Remove("FKEY");
                                }
                                DataTable table = dv.ToTable(false, exportTableFields.ToArray());
                                if (useTabOrder)
                                {
                                    dashboardHelper.OrderColumns(table, true);
                                }
                                System.Data.Common.DbDataReader dataReader = table.CreateDataReader(); //dv.ToTable().CreateDataReader();
                                db.InsertBulkRows("Select * From [" + kvp.Key + "]", dataReader, requestUpdateStatus, checkForCancellation);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
                }
                finally
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.Print("Export thread finished in " + stopWatch.Elapsed.ToString());
                }
            }
        }

        private void Export()
        {
            if (string.IsNullOrEmpty(txtConnectionInformation.Text))
            {
                return;
            }

            string tableName = string.Empty;

            if (cmbDestinationTable.SelectedItem != null)
            {
                tableName = cmbDestinationTable.SelectedItem.ToString();
            }
            else
            {
                tableName = cmbDestinationTable.Text;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                return;
            }

            if (
                cmbDataFormats.SelectedItem != null && 
                (cmbDataFormats.SelectedItem.ToString().Contains(".mdb") || 
                cmbDataFormats.SelectedItem.ToString().Contains("SQL Server")) 
                && db.TableExists("metaViews") 
                && db.ColumnExists("metaViews", "Name")
                && db.TableExists("metaFields") 
                && db.ColumnExists("metaFields", "DataTableName")
                )
            {
                List<string> columnNames = new List<string>();
                columnNames.Add("Name");
                DataTable dt = db.GetTableData("metaViews", columnNames);

                foreach (DataRow row in dt.Rows)
                {
                    string rowDataTable = row["Name"].ToString();
                    if (rowDataTable.StartsWith(tableName) || rowDataTable.ToLowerInvariant().StartsWith(tableName.ToLowerInvariant()))
                    {
                        Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_EXPORT_CANNOT_USE_EXISTING_FORM_NAME);
                        return;
                    }
                }

                columnNames = new List<string>();
                columnNames.Add("DataTableName");
                dt = db.GetTableData("metaFields", columnNames);

                foreach (DataRow row in dt.Rows)
                {
                    string rowDataTable = row["DataTableName"].ToString();
                    if (!string.IsNullOrEmpty(rowDataTable) && (tableName.StartsWith(rowDataTable) || tableName.ToLowerInvariant().StartsWith(rowDataTable.ToLowerInvariant())))
                    {
                        string diff = tableName.Replace(rowDataTable, "");
                        int num = -1;
                        bool success = int.TryParse(diff, out num);
                        if (success)
                        {
                            Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_EXPORT_CANNOT_OVERWRITE_FORM_TABLE);
                            return;
                        }                        
                    }
                }

                List<string> protectedTableNames = new List<string>();
                protectedTableNames.Add("metaBackgrounds");
                protectedTableNames.Add("metaDataTypes");
                protectedTableNames.Add("metaDbInfo");
                protectedTableNames.Add("metaFields");
                protectedTableNames.Add("metaFieldTypes");
                protectedTableNames.Add("metaImages");
                protectedTableNames.Add("metaLinks");
                protectedTableNames.Add("metaPages");
                protectedTableNames.Add("metaViews");

                foreach (string protectedTableName in protectedTableNames)
                {
                    if (protectedTableName.Equals(tableName) || protectedTableName.ToLowerInvariant().Equals(tableName.ToLowerInvariant()))
                    {
                        Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_EXPORT_CANNOT_OVERWRITE_META);
                        return;
                    }
                }
            }

            List<string> badFieldNames = new List<string>();
            foreach (string fieldName in lbxExportFields.SelectedItems)
            {
                if (fieldName.StartsWith(" ") || fieldName.EndsWith(" "))
                {
                    badFieldNames.Add(fieldName);
                }
            }            

            string fileName = db.DataSource + "\\" + tableName;
            
            if (!fileName.ToLowerInvariant().EndsWith(".csv") && !fileName.ToLowerInvariant().EndsWith(".txt"))
            {
                fileName = fileName + ".csv";
            }

            bool writeToFlatFile = false;
            if (cmbDataFormats.SelectedItem.ToString().ToLowerInvariant().Contains("ascii") || cmbDataFormats.SelectedItem.ToString().ToLowerInvariant().Contains("csv file"))
            {
                writeToFlatFile = true;
            }

            if (!writeToFlatFile)
            {
                if (badFieldNames.Count > 0)
                {
                    string problemFields = string.Empty;
                    int count = 0;
                    foreach (string fieldName in badFieldNames)
                    {
                        problemFields = problemFields + fieldName + ", ";
                        count++;
                    }
                    problemFields = problemFields.TrimEnd().TrimEnd(',');
                    problemFields = problemFields + ".";
                    Epi.Windows.MsgBox.ShowError(string.Format(SharedStrings.EXPORT_CANNOT_PROCEED_LEADING_TRAILING_SPACES, problemFields));
                    return;
                }
            }
            
            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Visible;
            messagePanel.SetProgressBarMax(dashboardHelper.RecordCount);
            messagePanel.SetProgressBarValue(0);
            messagePanel.ShowProgressBar();

            exportFields.Clear();
            if (lbxExportFields.SelectedItems.Count > 0)
            {
                foreach (string item in lbxExportFields.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        exportFields.Add(item);
                    }
                }
            }

            if (lbxExportFields.Items.Count == lbxExportFields.SelectedItems.Count)
            {
                allFieldsSelected = true;
            }
            else
            {
                allFieldsSelected = false;
            }

            if (checkboxUsePrompts.IsChecked == true)
                useFieldPrompts = true;
            else
                useFieldPrompts = false;

            if (checkboxTabOrder.IsChecked == true)
                useTabOrder = true;
            else
                useTabOrder = false;

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                btnExport.IsEnabled = false;
                cmbDestinationTable.IsEnabled = false;
                cmbDataFormats.IsEnabled = false;
                btnConnectionBrowse.IsEnabled = false;

                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                if (!writeToFlatFile)
                {
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(datadriverWorker_DoWork);
                    worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(datadriverWorker_WorkerCompleted);
                    worker.RunWorkerAsync(tableName);
                }
                else
                {
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(fileIOWorker_DoWork);
                    worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(fileIOWorker_WorkerCompleted);
                    worker.RunWorkerAsync(fileName);
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (!loadingCombos)
            {
                Export();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (worker == null || !worker.IsBusy)
            {
                this.Close();
            }
        }        

        private void cmbDestinationTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckForProblems();
            EnableDisableExportButton();
        }

        private void txtConnectionInformation_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableDisableExportButton();
        }

        private void cmbDestinationTable_TextInput(object sender, TextCompositionEventArgs e)
        {
            EnableDisableExportButton();
        }

        private void cmbDestinationTable_KeyDown(object sender, KeyEventArgs e)
        {
            EnableDisableExportButton();
        }

        private void cmbDestinationTable_KeyUp(object sender, KeyEventArgs e)
        {
            EnableDisableExportButton();
        }
    }
}
