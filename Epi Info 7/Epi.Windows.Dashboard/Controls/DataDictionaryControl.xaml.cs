using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi.Fields;
using Epi.WPF;
using Epi.WPF.Dashboard;

namespace Epi.WPF.Dashboard.Controls
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
        private DashboardHelper dashboardHelper;

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

        /// <summary>
        /// Bool used to determine if the gadget is currently processing results/calculating statistics
        /// </summary>
        private bool isProcessing;

        GridViewColumnHeader lastHeaderClicked = null;
        ListSortDirection lastDirection = ListSortDirection.Ascending;

        #endregion // Private Members

        #region Delegates
        private delegate void LoadDataDelegate(DataTable dictionaryTable);
        #endregion //Delegates

        #region Events
        public event GadgetProcessingFinishedHandler ProcessingFinished;
        #endregion // Events

        #region Constructors
        public DataDictionaryControl()
        {
            InitializeComponent();
        }
        #endregion // Constructors

        #region Properties
        public bool IsProcessing
        {
            get
            {
                return this.isProcessing;
            }
            set
            {
                this.isProcessing = value;
            }
        }
        #endregion // Properties

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public void Refresh(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
            baseWorker = new BackgroundWorker();
            baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
            baseWorker.RunWorkerAsync();
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
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetToProcessingState()
        {
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetToFinishedState()
        {
            this.IsProcessing = false;

            if (ProcessingFinished != null)
                ProcessingFinished(this);
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            lvMain.DataContext = null;
        }
        
        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result is DataTable)
            {
                DataTable dictionaryTable = e.Result as DataTable;
                this.Dispatcher.BeginInvoke(new LoadDataDelegate(LoadData), dictionaryTable); 
            }

            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
        }

        void GridViewColumnHeaderClickedHandler(object sender,
                                            RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort(header, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header 
                    if (lastHeaderClicked != null && lastHeaderClicked != headerClicked)
                    {
                        lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    lastHeaderClicked = headerClicked;
                    lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lvMain.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void LoadData(DataTable dictionaryTable)
        {
            GridView view = new GridView();

            dictionaryTable.Columns["columnname"].ColumnName = "Name";
            dictionaryTable.Columns["formname"].ColumnName = "Form";
            dictionaryTable.Columns["datatype"].ColumnName = "Data Type";
            dictionaryTable.Columns["epifieldtype"].ColumnName = "Epi Field Type";
            dictionaryTable.Columns["tablename"].ColumnName = "Table";

            GridViewColumn c1 = new GridViewColumn();
            c1.Header = "Name";
            c1.DisplayMemberBinding = new Binding("Name");            

            GridViewColumn c2 = new GridViewColumn();
            c2.Header = "Prompt";
            c2.DisplayMemberBinding = new Binding("Prompt");

            GridViewColumn c3 = new GridViewColumn();
            c3.Header = "Form";
            c3.DisplayMemberBinding = new Binding("Form");

            GridViewColumn c4 = new GridViewColumn();
            c4.Header = "Page";
            c4.DisplayMemberBinding = new Binding("Page");

            GridViewColumn c5 = new GridViewColumn();
            c5.Header = "Tab";
            c5.DisplayMemberBinding = new Binding("Tab");

            GridViewColumn c6 = new GridViewColumn();
            c6.Header = "Data Type";
            c6.DisplayMemberBinding = new Binding("Data Type");

            GridViewColumn c7 = new GridViewColumn();
            c7.Header = "Epi Field Type";
            c7.DisplayMemberBinding = new Binding("Epi Field Type");

            GridViewColumn c8 = new GridViewColumn();
            c8.Header = "Table";
            c8.DisplayMemberBinding = new Binding("Table");

            GridViewColumn c9 = new GridViewColumn();
            c9.Header = "Items";
            c9.DisplayMemberBinding = new Binding("Items");
            
            view.Columns.Add(c1);
            view.Columns.Add(c2);
            view.Columns.Add(c3);
            view.Columns.Add(c4);
            view.Columns.Add(c5);
            view.Columns.Add(c6);
            view.Columns.Add(c7);
            view.Columns.Add(c8);
            view.Columns.Add(c9);

            //lvMain.MouseRightButtonUp += new MouseButtonEventHandler(lvMain_MouseRightButtonUp);                

            view.AllowsColumnReorder = true;
            lvMain.BorderThickness = new Thickness(0);
            
            this.lvMain.View = view; 
            lvMain.DataContext = dictionaryTable;
            lvMain.SetBinding(ListView.ItemsSourceProperty, new Binding());            
        }

        //void lvMain_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if(lvMain.SelectedItems.Count == 1) 
        //    {

        //    }            
        //}

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

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));                

                Configuration config = dashboardHelper.Config;
                string yesValue = config.Settings.RepresentationOfYes;
                string noValue = config.Settings.RepresentationOfNo;

                try
                {
                    DataTable dictionaryTable = dashboardHelper.FieldTable.Copy();

                    foreach (KeyValuePair<string, string> kvp in dashboardHelper.TableColumnNames)
                    {
                        DataRow row = dictionaryTable.Rows.Find(kvp.Key);
                        if (row == null)
                        {
                            dictionaryTable.Rows.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (dashboardHelper.IsUsingEpiProject)
                    {
                        dictionaryTable.Columns.Add("Page", typeof(int));
                        dictionaryTable.Columns.Add("Tab", typeof(int));
                        dictionaryTable.Columns.Add("Prompt", typeof(string));
                        dictionaryTable.Columns.Add("Items", typeof(string));

                        foreach (DataRow fieldRow in dictionaryTable.Rows)
                        {
                            if (fieldRow["epifieldtype"] is RenderableField)
                            {
                                RenderableField renderableField = fieldRow["epifieldtype"] as RenderableField;
                                fieldRow["Page"] = renderableField.Page.Position + 1;
                                fieldRow["Tab"] = renderableField.TabIndex;
                                fieldRow["Prompt"] = renderableField.PromptText;
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
                        }

                        dictionaryTable.Columns["columnname"].SetOrdinal(0);
                        dictionaryTable.Columns["Prompt"].SetOrdinal(1);
                        dictionaryTable.Columns["formname"].SetOrdinal(2);
                        dictionaryTable.Columns["Page"].SetOrdinal(3);
                        dictionaryTable.Columns["Tab"].SetOrdinal(4);
                        dictionaryTable.Columns["datatype"].SetOrdinal(5);
                        dictionaryTable.Columns["epifieldtype"].SetOrdinal(6);
                        dictionaryTable.Columns["tablename"].SetOrdinal(7);
                        dictionaryTable.Columns["Items"].SetOrdinal(8);
                    }

                    if (dictionaryTable == null || dictionaryTable.Rows.Count == 0)
                    {
                        //this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "There are no valid fields to display.");
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetToFinishedState));
                        Debug.Print("Data dictionary thread cancelled");
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        //this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetToFinishedState));
                        Debug.Print("Data dictionary thread cancelled");
                        return;
                    }
                    else
                    {
                        e.Result = dictionaryTable;                        

                        //this.Dispatcher.BeginInvoke(addGrid, "", "", dictionaryTable.Columns.Count);
                        //string formatString = string.Empty;
                        //this.Dispatcher.BeginInvoke(renderHeader, "", "", dictionaryTable.Columns);

                        //int rowCount = 1;
                        //int columnCount = 1;

                        //foreach (System.Data.DataRow row in dictionaryTable.Rows)
                        //{
                        //    bool isGroup = false;

                        //    this.Dispatcher.Invoke(addRow, "", 30);
                        //    this.Dispatcher.BeginInvoke(setText, "", new TextBlockConfig(StringLiterals.SPACE + rowCount.ToString() + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Center, rowCount, 0, Visibility.Visible), FontWeights.Normal);

                        //    columnCount = 1;
                        //    foreach (DataColumn column in dictionaryTable.Columns)
                        //    {
                        //        string displayValue = row[column.ColumnName].ToString();
                        //        if (column.ColumnName.Equals("epifieldtype"))
                        //        {
                        //            displayValue = displayValue.Replace("Epi.Fields.", "");
                        //            if (isGroup)
                        //            {
                        //                displayValue = "GroupField";
                        //            }
                        //        }
                        //        else if (column.ColumnName.Equals("columnname"))
                        //        {
                        //            isGroup = dashboardHelper.GetGroupFieldsAsList().Contains(displayValue);
                        //        }
                        //        this.Dispatcher.BeginInvoke(setText, "", new TextBlockConfig(displayValue, new Thickness(8, 8, 8, 8), VerticalAlignment.Center, HorizontalAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                        //        columnCount++;
                        //    }

                        //    rowCount++;
                        //}

                        //this.Dispatcher.BeginInvoke(drawBorders, "");
                    }

                    //this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetToFinishedState));
                }
                catch (Exception ex)
                {
                    //this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Data dictionary took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }
    }
}
