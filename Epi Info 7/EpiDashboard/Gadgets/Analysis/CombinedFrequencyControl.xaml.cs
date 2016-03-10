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
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for CombinedFrequencyControl.xaml
    /// </summary>
    public partial class CombinedFrequencyControl : GadgetBase
    {
        #region Private Variables
        /// <summary>
        /// Used for maintaining the width when collapsing output
        /// </summary>
        private double currentWidth;

        #endregion // Private Variables

        #region Delegates

        private new delegate void AddGridFooterDelegate(string strataValue, int denominator, int fields = -1, bool isBoolean = false);

        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CombinedFrequencyControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this gadget</param>
        public CombinedFrequencyControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        /// <summary>
        /// Construct
        /// </summary>
        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }
           
            mnuCopyData.Click += new RoutedEventHandler(mnuCopyData_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

#if LINUX_BUILD
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
#else
            mnuSendDataToExcel.Visibility = Visibility.Visible;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            }
#endif

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            this.IsProcessing = false;

            base.Construct();
            this.Parameters = new CombinedFrequencyParameters();

        }  
        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Clears all of the gadget's output and sets it to its beginning state
        /// </summary>
        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            panelMain.Children.Clear();
        }
        
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the click event for the 'Copy data to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopyData_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }


        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //Debug.Print("Background worker thread for combined frequency gadget was cancelled or ran to completion.");
            System.Threading.Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);
                AddDataGridDelegate addDataGrid = new AddDataGridDelegate(AddDataGrid);

                System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();

                try
                {
                    DataTable dt = new DataTable();
                    bool booleanResults = false;
                    int fields = -1;

                    if (Parameters.ColumnNames.Count > 0)
                    {
                        //if (DashboardHelper.GetAllGroupsAsList().Contains(freqVar))
                        //{
                        dt = DashboardHelper.GenerateCombinedFrequencyTable(Parameters as CombinedFrequencyParameters, ref booleanResults, ref fields);
                        //fields = DashboardHelper.GetVariablesInGroup(freqVar).Count;
                        //}
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "Something that should never fail has failed.");
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Combined frequency thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;
                        double count = Convert.ToDouble(dt.Compute("sum([count])", string.Empty)); 
                        string strataValue = dt.TableName;
                        int denominator = DashboardHelper.RecordCount;

                        if (!string.IsNullOrEmpty(Parameters.CustomFilter.Trim()))
                        {
                            denominator = DashboardHelper.DataSet.Tables[0].Select(Parameters.CustomFilter.Trim()).Count();
                        }
                        
                        if (!booleanResults)
                        {
                            denominator = denominator * fields;
                        }

                        dt.Columns[0].ColumnName = DashboardSharedStrings.COL_HEADER_VALUE;
                        dt.Columns[1].ColumnName = DashboardSharedStrings.COL_HEADER_FREQUENCY;
                        dt.Columns.Add(new DataColumn(DashboardSharedStrings.COL_HEADER_PERCENT, typeof(double)));

                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            if (!row[DashboardSharedStrings.COL_HEADER_VALUE].Equals(DBNull.Value))
                            {   
                                double pct = 0;
                                if (count > 0)
                                {
                                    pct = Convert.ToDouble(row[DashboardSharedStrings.COL_HEADER_FREQUENCY]) / (double)denominator;
                                    row[DashboardSharedStrings.COL_HEADER_PERCENT] = pct;
                                }
                            }
                        }

                        this.Dispatcher.BeginInvoke(addDataGrid, dt.AsDataView(), dt.TableName);
                        this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, denominator, fields, booleanResults);
                        //this.Dispatcher.BeginInvoke(drawBorders, strataValue);

                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    //stopwatch.Stop();
                    //Debug.Print("Combined Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    //Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods

        private void AddDataGrid(DataView dv, string strataValue)
        {
            DataGrid dg = new DataGrid();
            dg.Style = this.Resources["LineListDataGridStyle"] as Style;

            CombinedFrequencyParameters CombinedFrequencyParameters = (this.Parameters) as CombinedFrequencyParameters;

            FrameworkElementFactory datagridRowsPresenter = new FrameworkElementFactory(typeof(DataGridRowsPresenter));
            ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate();
            itemsPanelTemplate.VisualTree = datagridRowsPresenter;
            GroupStyle groupStyle = new GroupStyle();
            groupStyle.ContainerStyle = this.Resources["DefaultGroupItemStyle"] as Style;
            groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle);

            GroupStyle groupStyle2 = new GroupStyle();
            groupStyle2.HeaderTemplate = this.Resources["GroupDataTemplate"] as DataTemplate;
            //groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle2);

            string groupVar = String.Empty;

            //if (!String.IsNullOrEmpty(ListParameters.PrimaryGroupField.Trim()))
            //{
            //    groupVar = ListParameters.PrimaryGroupField.Trim();
            //    ListCollectionView lcv = new ListCollectionView(dv);
            //    lcv.GroupDescriptions.Add(new PropertyGroupDescription(groupVar));
            //    if (!String.IsNullOrEmpty(ListParameters.SecondaryGroupField.Trim()) && !ListParameters.SecondaryGroupField.Trim().Equals(groupVar))
            //    {
            //        lcv.GroupDescriptions.Add(new PropertyGroupDescription(ListParameters.SecondaryGroupField.Trim())); // for second category
            //    }
            //    dg.ItemsSource = lcv;
            //}
            //else
            //{
                dg.ItemsSource = dv;
            //}


            if (Parameters.Height.HasValue)
            {
                dg.MaxHeight = Parameters.Height.Value;
            }
            else
            {
                dg.MaxHeight = 700;
            }

            if (Parameters.Width.HasValue)
            {
                dg.MaxWidth = Parameters.Width.Value;
            }
            else
            {
                dg.MaxWidth = 900;
            }

            dg.AutoGeneratedColumns += new EventHandler(dg_AutoGeneratedColumns);
            dg.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(dg_AutoGeneratingColumn);

            panelMain.Children.Add(dg);
        }

        void dg_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;

            DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
            if (dataGridTextColumn != null)
            {
                if (e.PropertyType == typeof(DateTime))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                    Field field = DashboardHelper.GetAssociatedField(e.Column.Header.ToString());
                    if (field != null && field is DateField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:d}";
                    }
                    else if (field != null && field is TimeField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:t}";
                    }
                }
                else if (e.PropertyType == typeof(int) ||
                    e.PropertyType == typeof(byte) ||
                    e.PropertyType == typeof(decimal) ||
                    e.PropertyType == typeof(double) ||
                    e.PropertyType == typeof(float))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                }
            }

            if (e.PropertyName == DashboardSharedStrings.COL_HEADER_PERCENT || e.PropertyName == "Percent")
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
            }
        }

        void dg_AutoGeneratedColumns(object sender, EventArgs e)
        {
        }

        private DataGrid GetDataGrid()
        {
            DataGrid dg = null;
            foreach (UIElement element in panelMain.Children)
            {
                if (element is DataGrid)
                {
                    dg = element as DataGrid;
                }
            }

            return dg;
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            DataGrid dg = GetDataGrid();

            if (dg != null)
            {
                Common.CopyDataViewToClipboard(dg.ItemsSource as DataView);
            }
        }

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Combined Frequency Gadget";
        }

        public override void CollapseOutput()
        {
            panelMain.Visibility = System.Windows.Visibility.Collapsed;
            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            panelMain.Visibility = System.Windows.Visibility.Visible;

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }
            IsCollapsed = false;
        }

        private void RenderFrequencyFooter(string strataValue, int denominator, int fields = -1, bool isBoolean = false)
        {
            if (!((Parameters as CombinedFrequencyParameters).ShowDenominator))
            {
                return;
            }

            TextBlock txtDenom = new TextBlock();
            txtDenom.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            txtDenom.FontWeight = FontWeights.Bold;
            txtDenom.Margin = (Thickness)this.Resources["genericTextMargin"];

            if (isBoolean == true)
            {
                txtDenom.Text = string.Format(DashboardSharedStrings.DENOMINATOR_DESCRIPTION_BOOLEAN, denominator);
            }
            else
            {
                txtDenom.Text = string.Format(DashboardSharedStrings.DENOMINATOR_DESCRIPTION_NON_BOOLEAN, denominator);
            }

            panelMain.Children.Add(txtDenom);
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            base.CloseGadget();
        }

        /// <summary>
        /// Finishes the drawing process
        /// </summary>
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Finishes the drawing process with a warning message
        /// </summary>
        /// <param name="errorMessage">The message to display</param>
        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Finishes the drawing process with an error message
        /// </summary>
        /// <param name="errorMessage">The message to display</param>
        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        #endregion // Private Methods

        #region Private Properties
        private View View
        {
            get
            {
                return this.DashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.DashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            currentWidth = borderAll.ActualWidth;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Forces a refresh
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && Parameters != null && Parameters.ColumnNames.Count > 0)
            {
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
        }

        Controls.GadgetProperties.CombinedFrequencyProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.CombinedFrequencyProperties(this.DashboardHelper, this, (CombinedFrequencyParameters)Parameters);

            if (DashboardHelper.ResizedWidth != 0 & DashboardHelper.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)DashboardHelper.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)DashboardHelper.ResizedWidth / (float)i_StandardWidth);

                properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        public override void GadgetBase_SizeChanged(double width, double height)
        {
            double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
            double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
            float f_HeightRatio = new float();
            float f_WidthRatio = new float();
            f_HeightRatio = (float)((float)height / (float)i_StandardHeight);
            f_WidthRatio = (float)((float)width / (float)i_StandardWidth);

            if (properties == null)
                properties = new Controls.GadgetProperties.CombinedFrequencyProperties(this.DashboardHelper, this, (CombinedFrequencyParameters)Parameters);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.CombinedFrequencyProperties properties = Popup.Content as Controls.GadgetProperties.CombinedFrequencyProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            CombinedFrequencyParameters combFreqParameters = (CombinedFrequencyParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("combinedFrequencyGadget");
            //element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");
            System.Xml.XmlAttribute actualHeight = doc.CreateAttribute("actualHeight");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.CombinedFrequencyControl";
            actualHeight.Value = this.ActualHeight.ToString();

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);
            if (IsCollapsed == false)
            {
                element.Attributes.Append(actualHeight);
            }
            
            XmlElement combineModeElement = doc.CreateElement("combineMode");
            combineModeElement.InnerText = combFreqParameters.CombineMode.ToString();
            element.AppendChild(combineModeElement);

            XmlElement trueValueElement = doc.CreateElement("trueValue");
            trueValueElement.InnerText = combFreqParameters.TrueValue.ToString();
            element.AppendChild(trueValueElement);

            XmlElement sortElement = doc.CreateElement("sort");
            if (combFreqParameters.SortHighToLow) sortElement.InnerText = "hightolow";
            element.AppendChild(sortElement);

            XmlElement showDenominatorElement = doc.CreateElement("showDenominator");
            showDenominatorElement.InnerText = combFreqParameters.ShowDenominator.ToString();
            element.AppendChild(showDenominatorElement);

            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = combFreqParameters.GadgetTitle.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            XmlElement customDescElement = doc.CreateElement("customDescription");
            customDescElement.InnerText = combFreqParameters.GadgetDescription.Replace("<", "&lt;");
            element.AppendChild(customDescElement);

            SerializeAnchors(element);

            XmlElement groupItemElement = doc.CreateElement("groupFields");

            string xmlGroupItemString = string.Empty;

            foreach (string columnName in combFreqParameters.ColumnNames)
            {
                xmlGroupItemString = xmlGroupItemString + "<groupField>" + columnName.Replace("<", "&lt;") + "</groupField>";
            }

            groupItemElement.InnerXml = xmlGroupItemString;

            if (!String.IsNullOrEmpty(xmlGroupItemString))
            {
                element.AppendChild(groupItemElement);
            }

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new CombinedFrequencyParameters();

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "actualheight":
                        string actualHeight = attribute.Value.Replace(',', '.');
                        double controlheight = 0.0;
                        double.TryParse(actualHeight, out controlheight);
                        this.Height = controlheight;
                        break;
                }
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        ((CombinedFrequencyParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "groupfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLower().Equals("groupfield"))
                            {
                                ((CombinedFrequencyParameters)Parameters).ColumnNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "combinemode":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            switch (child.InnerText.ToString().ToLower())
                            {
                                case "boolean":
                                    ((CombinedFrequencyParameters)Parameters).CombineMode = CombineModeTypes.Boolean;
                                    break;
                                case "categorical":
                                    ((CombinedFrequencyParameters)Parameters).CombineMode = CombineModeTypes.Categorical;
                                    break;
                                case "automatic":
                                default:
                                    ((CombinedFrequencyParameters)Parameters).CombineMode = CombineModeTypes.Automatic;
                                    break;
                            }
                        }
                        break;
                    case "truevalue":
                        //txtTrueValue.Text = child.InnerText;
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((CombinedFrequencyParameters)Parameters).TrueValue = child.InnerText.Replace("&lt;", "<").ToString();
                        }
                        break;
                    case "sort":
                        if (child.InnerText.ToLower().Equals("hightolow") || child.InnerText.ToLower().Equals("highlow"))
                        {
                            ((CombinedFrequencyParameters)Parameters).SortHighToLow = true;
                        }
                        else
                        {
                            ((CombinedFrequencyParameters)Parameters).SortHighToLow = false;
                        }
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetTitle = CustomOutputHeading;                           
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetDescription = CustomOutputDescription;
                            if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                            {
                                descriptionPanel.Text = CustomOutputDescription;
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                            }
                            else
                            {
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                            }
                        }
                        break;
                    case "showdenominator":
                        bool showDenom = false;
                        bool.TryParse(child.InnerText, out showDenom);
                        ((CombinedFrequencyParameters)Parameters).ShowDenominator = showDenom;
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;                   
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Combined Frequency</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            // TODO: Update this with columns selected
            //htmlBuilder.AppendLine("<em>Group variable:</em> <strong>" + GadgetOptions.MainVariableName + "</strong>");
            htmlBuilder.AppendLine("<br />");

            //if (cmbFieldStrata.SelectedIndex >= 0)
            //{
            //    htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cmbFieldStrata.Text + "</strong>");
            //    htmlBuilder.AppendLine("<br />");
            //}

            //htmlBuilder.AppendLine("<em>Combine mode:</em> <strong>" + cmbCombineMode.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == System.Windows.Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
            }

            DataGrid dg = GetDataGrid();

            if (dg != null && dg.ItemsSource != null)
            {
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(dg.ItemsSource as DataView));

                htmlBuilder.AppendLine("</table>");
            }
            // TODO: Update if this gadget ever implements stratas
            foreach (UIElement element in panelMain.Children)
            {
                if (element is TextBlock)
                {
                    TextBlock tblock = element as TextBlock;
                    htmlBuilder.AppendLine("<p>");
                    htmlBuilder.AppendLine(tblock.Text);
                    htmlBuilder.AppendLine("</p>");
                }
            }

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
                headerPanel.Text = CustomOutputHeading;
            }
        }

        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                descriptionPanel.Text = CustomOutputDescription;
            }
        }
        #endregion // IGadget Members
    }
}
