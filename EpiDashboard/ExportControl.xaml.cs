using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
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
using Microsoft.Windows.Controls;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard
{    
    /// <summary>
    /// Interaction logic for ExportControl.xaml
    /// </summary>
    public partial class ExportControl : UserControl
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
        public event GadgetClosingHandler GadgetClosing;
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
        public ExportControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();

            pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            pnlError.Visibility = System.Windows.Visibility.Collapsed;
            pnlProgress.Visibility = System.Windows.Visibility.Collapsed;

            this.dashboardHelper = dashboardHelper;
            lbxExportFields.SelectionChanged += new SelectionChangedEventHandler(lbxExportFields_SelectionChanged);
            FillComboboxes();
            exportFields = new List<string>();
            allFieldsSelected = true;

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            cmbDataFormats.Items.Clear();
            this.GadgetProgressUpdate += new GadgetProgressUpdateHandler(RequestUpdateStatusMessage);

            foreach (Epi.DataSets.Config.DataDriverRow row in dashboardHelper.Config.DataDrivers)
            {
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
        }
        #endregion // Constructors

        private void cmbDataFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtConnectionInformation.Clear();
            cmbDestinationTable.Items.Clear();

            if (cmbDataFormats.SelectedIndex >= 0)
            {
                if (cmbDataFormats.SelectedItem.ToString().ToLower().Contains("ascii") || cmbDataFormats.SelectedItem.ToString().ToLower().Contains("csv file"))
                {
                    tblockDestinationTable.Text = "File name";
                }
                else
                {
                    tblockDestinationTable.Text = "Destination table";
                }
            }
        }

        private void lbxExportFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tblockFieldCount.Text = lbxExportFields.SelectedItems.Count.ToString() + " fields selected";
        }

        private void btnConnectionBrowse_Click(object sender, RoutedEventArgs e)
        {
            SetupOutputDataSource();
        }
        
        public void MinimizeGadget()
        {
            ConfigGrid.Height = 50;
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
            pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;

            if (cmbDestinationTable.SelectedItem != null)
            {
                foreach (String item in cmbDestinationTable.Items)
                {
                    if (item.Equals(cmbDestinationTable.SelectedItem.ToString()))
                    {
                        pnlTableOverwrite.Visibility = System.Windows.Visibility.Visible;
                        txtTableOverwrite.Text = string.Format(SharedStrings.DASHBOARD_EXPORT_WARNING, item);
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
            txtProgress.Text = statusMessage;
        }

        /// <summary>
        /// Used to display a warning
        /// </summary>
        /// <param name="warningMessage">The warning message to display</param>
        private void SetWarningMessage(string warningMessage)
        {
            if (string.IsNullOrEmpty(warningMessage))
            {
                pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pnlTableOverwrite.Visibility = System.Windows.Visibility.Visible;
            }
            txtTableOverwrite.Text = warningMessage;
        }

        /// <summary>
        /// Used to display an error
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void SetErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                pnlError.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pnlError.Visibility = System.Windows.Visibility.Visible;
                pnlProgress.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (errorMessage.Equals("Numeric field overflow."))
            {
                errorMessage = SharedStrings.DASHBOARD_EXPORT_EXCEL_NUMERIC_FIELD_OVERFLOW_FAIL;
            }
            txtError.Text = errorMessage;
        }

        /// <summary>
        /// Used to set the value of the progress bar.
        /// </summary>
        /// <param name="progress">The progress value to use</param>
        private void SetProgressBarValue(double progress)
        {
            progressBar.Value = progress;
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
                        
                        if (db.ConnectionDescription.ToLower().Contains("csv file:"))
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
                pnlProgress.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        #region Event Handlers
        /// <summary>
        /// Fires the 'Close' button event that will remove the gadget from the dashboard
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (worker == null || !worker.IsBusy)
            {
                this.IsEnabled = false;
                int speed = 400;
                Storyboard storyboard = new Storyboard();
                storyboard.Completed += new EventHandler(storyboard_Completed);
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //            

                DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
                if (this.Opacity == 0.0)
                {
                    animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
                }

                Storyboard.SetTargetName(animation, this.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
                storyboard.Children.Add(animation);

                storyboard.Begin(this);
            }
        }

        /// <summary>
        /// Fired when the user removes the mouse cursor from inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Fired when the user places the mouse cursor inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }
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

            if (GadgetClosing != null)
                GadgetClosing(this);

            GadgetClosing = null;
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
            progressBar.Value = 0;
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
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
            progressBar.Value = 0;
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
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

                    //StreamWriter sw = File.CreateText(fileName);

                    

                    //DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;
                    //WordBuilder wb = new WordBuilder(SEPARATOR);
                    //DataTable table = dashboardHelper.DataSet.Tables[0];
                    //if (useTabOrder || !allFieldsSelected)
                    //{
                    //    table = dv.ToTable(false);
                    //    dashboardHelper.OrderColumns(table, useTabOrder);

                    //    List<DataColumn> columnsToRemove = new List<DataColumn>();

                    //    foreach (DataColumn dc in table.Columns)
                    //    {
                    //        bool found = false;
                    //        foreach (Epi.Data.TableColumn tc in tcList)
                    //        {
                    //            if (tc.Name.Equals(dc.ColumnName))
                    //            {
                    //                found = true;
                    //                break;
                    //            }
                    //        }

                    //        if (!found)
                    //        {
                    //            columnsToRemove.Add(dc);
                    //        }
                    //    }

                    //    foreach (DataColumn dc in columnsToRemove)
                    //    {
                    //        table.Columns.Remove(dc);
                    //    }
                    //}

                    //foreach (DataColumn dc in table.Columns)
                    //{
                    //    wb.Add(dc.ColumnName);
                    //}

                    //sw.WriteLine(wb.ToString());
                    //int rowsExported = 0;
                    //int totalRows = 0;

                    //if (useTabOrder || !allFieldsSelected)
                    //{
                    //    totalRows = table.Rows.Count;
                    //    foreach (DataRow row in table.Rows)
                    //    {
                    //        wb = new WordBuilder(SEPARATOR);
                    //        for (int i = 0; i < table.Columns.Count; i++)
                    //        {
                    //            string rowValue = row[i].ToString().Replace("\r\n", " ");
                    //            if (rowValue.Contains(",") || rowValue.Contains("\""))
                    //            {
                    //                rowValue = rowValue.Replace("\"", "\"\"");
                    //                rowValue = Util.InsertIn(rowValue, "\"");
                    //            }
                    //            wb.Add(rowValue);
                    //        }
                    //        sw.WriteLine(wb);
                    //        rowsExported++;
                    //        if (rowsExported % 500 == 0)
                    //        {
                    //            //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    //            RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    totalRows = dv.Count;
                    //    foreach (DataRowView rowView in dv)
                    //    {
                    //        wb = new WordBuilder(SEPARATOR);
                    //        for (int i = 0; i < table.Columns.Count; i++)
                    //        {
                    //            DataRow row = rowView.Row;
                    //            string rowValue = row[i].ToString().Replace("\r\n", " ");
                    //            if (rowValue.Contains(",") || rowValue.Contains("\""))
                    //            {
                    //                rowValue = rowValue.Replace("\"", "\"\"");
                    //                rowValue = Util.InsertIn(rowValue, "\"");
                    //            }
                    //            wb.Add(rowValue);
                    //        }
                    //        sw.WriteLine(wb);
                    //        rowsExported++;
                    //        if (rowsExported % 500 == 0)
                    //        {
                    //            //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    //            RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    //        }
                    //    }
                    //}

                    //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));

                    //sw.Close();
                    //sw.Dispose();
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

                    if (db.TableExists(tableName) && !db.ConnectionDescription.ToLower().Contains("excel"))
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

            string fileName = db.DataSource + "\\" + tableName;

            if (!fileName.ToLower().EndsWith(".csv") && !fileName.ToLower().EndsWith(".txt"))
            {
                fileName = fileName + ".csv";
            }

            pnlError.Visibility = System.Windows.Visibility.Collapsed;

            pnlProgress.Visibility = System.Windows.Visibility.Visible;
            progressBar.Visibility = System.Windows.Visibility.Visible;
            progressBar.Minimum = 0;
            progressBar.Maximum = dashboardHelper.RecordCount;
            progressBar.Value = 0;

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

            bool writeToFlatFile = false;
            if (cmbDataFormats.SelectedItem.ToString().ToLower().Contains("ascii") || cmbDataFormats.SelectedItem.ToString().ToLower().Contains("csv file"))
            {
                writeToFlatFile = true;
            }

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
                this.IsEnabled = false;
                int speed = 400;
                Storyboard storyboard = new Storyboard();
                storyboard.Completed += new EventHandler(storyboard_Completed);
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //            

                DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
                if (this.Opacity == 0.0)
                {
                    animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
                }                

                Storyboard.SetTargetName(animation, this.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
                storyboard.Children.Add(animation);

                storyboard.Begin(this);   
            }
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            this.CloseGadget();
        }

        private void cmbDestinationTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckForProblems();            
        }
    }
}
