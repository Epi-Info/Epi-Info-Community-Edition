using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
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
    /// Interaction logic for MergeControl.xaml
    /// </summary>
    public partial class MergeControl : UserControl
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private object selectedDataSource;
        private string selectedDataProvider;
        private bool controlNeedsRefresh = false;
        private IDbDriver db = null;
        private object syncLock = new object();
        private BackgroundWorker worker;
        #endregion // Private Members

        #region Public Events
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetProgressUpdateHandler GadgetProgressUpdate;
        #endregion // Public Events

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void SetProgressBarDelegate(double progress);
        private delegate bool CheckForCancellationDelegate();        
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view to attach</param>
        /// <param name="db">The database to attach</param>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public MergeControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();

            pnlTableOverwrite.Visibility = System.Windows.Visibility.Collapsed;
            pnlError.Visibility = System.Windows.Visibility.Collapsed;
            pnlProgress.Visibility = System.Windows.Visibility.Collapsed;

            this.dashboardHelper = dashboardHelper;

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            cmbSourceDataFormat.Items.Clear();
            this.GadgetProgressUpdate += new GadgetProgressUpdateHandler(RequestUpdateStatusMessage);

            foreach (Epi.DataSets.Config.DataDriverRow row in dashboardHelper.Config.DataDrivers)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = row.DisplayName;                
                //cmbDataFormats.Items.Add(new ComboBoxItem(row.Type, row.DisplayName, null));
                cmbSourceDataFormat.Items.Add(item);
            }
        }
        #endregion // Constructors

        private void btnConnectionBrowse_Click(object sender, RoutedEventArgs e)
        {
            SetupOutputDataSource();
        }
        
        public void MinimizeGadget()
        {
            ConfigGrid.Height = 50;
            //triangleCollapsed = true;
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

            if (cmbSourceTable.SelectedItem != null)
            {
                foreach (String item in cmbSourceTable.Items)
                {
                    if (item.Equals(cmbSourceTable.SelectedItem.ToString()))
                    {
                        pnlTableOverwrite.Visibility = System.Windows.Visibility.Visible;
                        txtTableOverwrite.Text = string.Format(SharedStrings.DASHBOARD_EXPORT_WARNING, item);
                    }
                }
            }
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
            ComboBoxItem selectedPlugIn = cmbSourceDataFormat.SelectedItem as ComboBoxItem;
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

                    try
                    {
                        success = db.TestConnection();
                    }
                    catch
                    {
                        success = false;
                        MessageBox.Show("Could not connect to selected data source.");
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

                System.Collections.Generic.List<string> tableNames = db.GetTableNames();

                foreach (string tableName in tableNames)
                {
                    ComboBoxItem newItem = new ComboBoxItem();//tableName, tableName, tableName);
                    newItem.Content = tableName;
                    cmbSourceTable.Items.Add(tableName);
                    //this.cmbDataTable.Items.Add(newItem);
                }
            }

            pnlProgress.Visibility = System.Windows.Visibility.Collapsed;
        }

        #region Event Handlers
        /// <summary>
        /// Fires the 'Close' button event that will remove the gadget from the dashboard
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseGadget();
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
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
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
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.Print("Background worker thread for export gadget was cancelled or ran to completion.");
            btnMerge.IsEnabled = true;
            cmbSourceTable.IsEnabled = true;
            cmbSourceDataFormat.IsEnabled = true;
            btnConnectionBrowse.IsEnabled = true;
            progressBar.Value = 0;
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
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
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                try
                {
                    string tableName = (string)e.Argument;

                    SetGadgetStatusHandler requestUpdateStatus = new SetGadgetStatusHandler(RequestUpdateStatusMessage);
                    CheckForCancellationHandler checkForCancellation = new CheckForCancellationHandler(IsCancelled);

                    if (db.TableExists(tableName))
                    {
                        db.DeleteTable(tableName);
                    }

                    List<Epi.Data.TableColumn> tcList = dashboardHelper.GetFieldsAsListOfEpiTableColumns();

                    db.CreateTable(tableName, tcList);

                    DataView dv = dashboardHelper.DataSet.Tables[0].DefaultView;

                    System.Data.Common.DbDataReader dataReader = dv.ToTable().CreateDataReader();
                    db.InsertBulkRows("Select * From [" + tableName + "]", dataReader, requestUpdateStatus, checkForCancellation);
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetWarningMessage), ex.Message);
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

            if (cmbSourceTable.SelectedItem != null)
            {
                tableName = cmbSourceTable.SelectedItem.ToString();
            }
            else
            {
                tableName = cmbSourceTable.Text;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                return;
            }

            pnlProgress.Visibility = System.Windows.Visibility.Visible;
            progressBar.Visibility = System.Windows.Visibility.Visible;
            progressBar.Minimum = 0;
            progressBar.Maximum = dashboardHelper.RecordCount;
            progressBar.Value = 0;

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                btnMerge.IsEnabled = false;
                cmbSourceTable.IsEnabled = false;
                cmbSourceDataFormat.IsEnabled = false;
                btnConnectionBrowse.IsEnabled = false;

                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync(tableName);
            }
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            Export();
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
