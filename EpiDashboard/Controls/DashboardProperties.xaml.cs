using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class DashboardProperties : UserControl
    {
        public DashboardProperties(DashboardHelper dashboardHelper)
        {
            #region Input Validation
            if (dashboardHelper == null) { throw new ArgumentNullException("dashboardHelper"); }
            #endregion // Input Validation

            InitializeComponent();
            this.DashboardHelper = dashboardHelper;

            DataContext = this;

            if (DashboardHelper.IsUsingEpiProject)
            {
                txtProjectPath.Text = dashboardHelper.View.Project.FilePath;

                if (System.IO.File.Exists(txtProjectPath.Text))
                {
                    cmbFormName.Items.Clear();
                    Project project = new Project(txtProjectPath.Text);
                    foreach (View view in project.Views)
                    {
                        cmbFormName.Items.Add(view.Name);
                    }
                }

                cmbFormName.Text = dashboardHelper.View.Name;

                panelDataSourceProject.Visibility = System.Windows.Visibility.Visible;
                panelDataSourceOther.Visibility = System.Windows.Visibility.Collapsed;
                panelDataSourceSql.Visibility = System.Windows.Visibility.Collapsed;
                panelAdvancedSQLQuery.Visibility = System.Windows.Visibility.Collapsed;
            }
            else 
            {
                if (dashboardHelper.Database.ToString().Contains("SqlDatabase"))
                {
                    txtSQLConnectionString.Text = DashboardHelper.Database.ConnectionString;

                    Epi.Data.IDbDriver database = Epi.Data.DBReadExecute.GetDataDriver(txtSQLConnectionString.Text);
                    List<string> tableNames = database.GetTableNames();
                    cmbSqlTable.Items.Clear();
                    cmbSqlTable.Items.Add("");
                        
                    foreach (string tableName in tableNames)
                    {
                        cmbSqlTable.Items.Add(tableName);
                    }

                    cmbSqlTable.SelectedItem = DashboardHelper.TableName;

                    if (!String.IsNullOrEmpty(dashboardHelper.CustomQuery))
                    {
                        CustomQuery = DashboardHelper.CustomQuery;
                    }

                    panelDataSourceProject.Visibility = System.Windows.Visibility.Collapsed;
                    panelDataSourceOther.Visibility = System.Windows.Visibility.Collapsed;
                    panelDataSourceSql.Visibility = System.Windows.Visibility.Visible;
                    panelAdvancedSQLQuery.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txtStandalonePath.Text = DashboardHelper.Database.DataSource;

                    Epi.Data.IDbDriver database = Epi.Data.DBReadExecute.GetDataDriver(txtStandalonePath.Text);
                    List<string> tableNames = database.GetTableNames();
                    cmbStandaloneFormName.Items.Clear();
                    cmbStandaloneFormName.Items.Add(""); 
                    foreach (string tableName in tableNames)
                    {
                        cmbStandaloneFormName.Items.Add(tableName);
                    }

                    cmbStandaloneFormName.SelectedItem = DashboardHelper.TableName;

                    if (!String.IsNullOrEmpty(dashboardHelper.CustomQuery))
                    {
                        CustomQuery = DashboardHelper.CustomQuery;
                    }

                    panelDataSourceProject.Visibility = System.Windows.Visibility.Collapsed;
                    panelDataSourceOther.Visibility = System.Windows.Visibility.Visible;
                    panelDataSourceSql.Visibility = System.Windows.Visibility.Collapsed;
                    panelAdvancedSQLQuery.Visibility = System.Windows.Visibility.Visible;
                }
            }

            tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + DashboardSharedStrings.GADGET_INFO_UNFILTERED_ROWS;
            tblockColumns.Text = dashboardHelper.DataSet.Tables[0].Columns.Count.ToString() + DashboardSharedStrings.CANVAS_COLUMNS;
            tblockCacheDateTime.Text = DashboardSharedStrings.CANVAS_DATA_LAST_CACHED + dashboardHelper.LastCacheTime.ToShortDateString() + " " + dashboardHelper.LastCacheTime.ToShortTimeString();
            tblockCacheTimeElapsed.Text = DashboardSharedStrings.CANVAS_INFO_TOOK + dashboardHelper.TimeToCache + DashboardSharedStrings.CANVAS_INFO_LOCALLY_CACHE_DATA;

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            lbxRelatedDataSources.Items.Clear();
            if (dashboardHelper.ConnectionsForRelate.Count > 0)
            {
                // Related Data
                foreach (RelatedConnection rConn in dashboardHelper.ConnectionsForRelate)
                {
                    lbxRelatedDataSources.Items.Add(rConn.db.ConnectionString);
                }
            }


            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.CMENU_PROPERTIES;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.CANVASPROPERTIES_TABDESC_DATASOURCE;
            tbtnHTML.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnHTML.Description = DashboardSharedStrings.CANVASPROPERTIES_DESC_HTMLOUTPUT;
            //tbtnCharts.Title = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            //tbtnCharts.Description = DashboardSharedStrings.CANVASPROPERTIES_DESC_CHARTS;
            tbtnInfo.Title = DashboardSharedStrings.CANVASPROPERTIES_TITLE_INFORMATION;
            tbtnInfo.Description = DashboardSharedStrings.CANVASPROPERTIES_DESC_INFORMATION;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockEpiInfoPro.Content = DashboardSharedStrings.CANVAS_EPIINFOPRO;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            tblockForm.Content = DashboardSharedStrings.CANVAS_FORM;
            lblRelatedDataSources.Content = DashboardSharedStrings.RALATED_DATASOURCE;
            
            //Display Panel
            tblockHTMLOutput.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockHTMLSetting.Content = DashboardSharedStrings.CANVAS_HTMLOUTPUT_SETTINGS;
            txtTitle.Text = DashboardSharedStrings.CANVAS_TITLE;
            txtSummary.Text = DashboardSharedStrings.CANVAS_SUMMARY;
            txtConclusion.Text = DashboardSharedStrings.CANVAS_CONCLUSION;
            checkboxGadgetHeadings.Content = DashboardSharedStrings.CANVAS_GADGET_HEADING;
            checkboxGadgetSettings.Content = DashboardSharedStrings.CANVAS_GADGET_SETTINGS;
            checkboxCanvasSummary.Content = DashboardSharedStrings.CANVAS_CANVAS_SUMMARY;
            checkboxAlternateColors.Content = DashboardSharedStrings.CANVAS_ALTERNATING_COLORS;
            checkboxTopToBottom.Content = DashboardSharedStrings.CANVAS_DISPLAY_TOPTO_BOTTOM;
            tblockChartSetting.Content = DashboardSharedStrings.CANVAS_CHART_SETTINGS;
            lbldefaultwidth.Content = DashboardSharedStrings.CANVAS_DEFAULT_WIDTH;
            lbldefaultheight.Content = DashboardSharedStrings.CANVAS_DEFAULT_HEIGHT;

            //Information Panel
            lblCanvasInformation.Content = DashboardSharedStrings.GADGET_CANVAS_INFO;

            #endregion // Translation
        }

        public string Conclusion
        {
            get
            {
                return txtConclusion.Text;
            }
            set
            {
                txtConclusion.Text = value;
            }
        }

        public string Title
        {
            get
            {
                return txtTitle.Text;
            }
            set
            {
                txtTitle.Text = value;
            }
        }

        public string Summary
        {
            get
            {
                return txtSummary.Text;
            }
            set
            {
                txtSummary.Text = value;
            }
        }

        public bool ShowGadgetHeadings
        {
            get
            {
                return (bool)checkboxGadgetHeadings.IsChecked;
            }
            set
            {
                checkboxGadgetHeadings.IsChecked = value;
            }
        }

        public bool ShowGadgetSettings
        {
            get
            {
                return (bool)checkboxGadgetSettings.IsChecked;
            }
            set
            {
                checkboxGadgetSettings.IsChecked = value;
            }
        }

        public Visibility StandaloneData
        {
            get 
            {
                return Visibility.Collapsed;
            }
        }

        public Visibility IsProjectDataSource
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        public bool ShowCanvasSummary
        {
            get
            {
                return (bool)checkboxCanvasSummary.IsChecked;
            }
            set
            {
                checkboxCanvasSummary.IsChecked = value;
            }
        }

        public bool UseAlternatingColors
        {
            get
            {
                return (bool)checkboxAlternateColors.IsChecked;
            }
            set
            {
                checkboxAlternateColors.IsChecked = value;
            }
        }

        public bool UseTopToBottomGadgetOrdering
        {
            get
            {
                return (bool)checkboxTopToBottom.IsChecked;
            }
            set
            {
                checkboxTopToBottom.IsChecked = value;
            }
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }

        public FileInfo ProjectFileInfo
        {
            get 
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceSql.Visibility = Visibility.Collapsed;
                panelAdvancedSQLQuery.Visibility = Visibility.Collapsed;
            }
        }

        public double? DefaultChartHeight
        {
            get
            {
                double value = -1;
                bool success = Double.TryParse(txtDefaultChartHeight.Text, out value);
                if (success)
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    txtDefaultChartHeight.Text = value.ToString();
                }
                else
                {
                    txtDefaultChartHeight.Text = String.Empty;
                }
            }
        }

        public double? DefaultChartWidth
        {
            get
            {
                double value = -1;
                bool success = Double.TryParse(txtDefaultChartWidth.Text, out value);
                if (success)
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    txtDefaultChartWidth.Text = value.ToString();
                }
                else
                {
                    txtDefaultChartWidth.Text = String.Empty;
                }
            }
        }

        public string FormName
        {
            get
            {
                return cmbFormName.SelectedItem.ToString();
            }
            set
            {
                cmbFormName.SelectedItem = value;
            }
        }

        public string TableName
        {
            get
            {
                if (this.DashboardHelper.Database.ToString().Contains("SqlDatabase"))
                {
                    return cmbSqlTable.Text;
                }
                else
                {
                    return cmbStandaloneFormName.Text;
                }
            }
            set
            {
                if (this.DashboardHelper.Database.ToString().Contains("SqlDatabase"))
                {
                    cmbSqlTable.Text = value;
                }
                else
                {
                    cmbStandaloneFormName.Text = value;
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                return txtSQLConnectionString.Text;
            }
            set
            {
                txtSQLConnectionString.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceSql.Visibility = Visibility.Visible;
                panelAdvancedSQLQuery.Visibility = Visibility.Visible;
            }
        }

        public string CustomQuery
        {
            get
            {
                return txtAdvancedSQLQuery.Text;
            }
            set
            {
                txtAdvancedSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceSql.Visibility = Visibility.Visible;
                panelAdvancedSQLQuery.Visibility = Visibility.Visible;
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            if (sender != null)
            {
                CheckButtonStates(tb);
                panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
                panelHTML.Visibility = System.Windows.Visibility.Collapsed;
                //panelCharts.Visibility = System.Windows.Visibility.Collapsed;
                panelInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            if (sender != null)
            {
                CheckButtonStates(tb);
                panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
                panelHTML.Visibility = System.Windows.Visibility.Collapsed;
                //panelCharts.Visibility = System.Windows.Visibility.Visible;
                panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            if (sender != null)
            {
                CheckButtonStates(tb);
                panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
                panelHTML.Visibility = System.Windows.Visibility.Visible;
                //panelCharts.Visibility = System.Windows.Visibility.Collapsed;
                panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            ToggleButton tb = sender as ToggleButton;
            if (sender != null)
            {
                CheckButtonStates(tb);
                panelDataSource.Visibility = System.Windows.Visibility.Visible;
                panelHTML.Visibility = System.Windows.Visibility.Collapsed;
                //panelCharts.Visibility = System.Windows.Visibility.Collapsed;
                panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                ToggleButton tbtn = element as ToggleButton;
                if (tbtn != null && tbtn != sender)
                {
                    tbtn.IsChecked = false;
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".prj";
            dlg.Filter = "Epi Info 7 Project|*.prj";
            if (!String.IsNullOrEmpty(txtProjectPath.Text))
            {
                dlg.FileName = txtProjectPath.Text;
            }
            else
            {
                dlg.InitialDirectory = this.DashboardHelper.Config.Directories.Project;
            }

            if (dlg.ShowDialog().Value)
            {
                txtProjectPath.Text = dlg.FileName;
                btnOK.IsEnabled = false;
            }          
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmbFormName.Items.Clear();
            if (System.IO.File.Exists(txtProjectPath.Text) && txtProjectPath.Text.ToLowerInvariant().EndsWith("prj"))
            {
                Project project = new Project(txtProjectPath.Text);
                cmbFormName.Items.Clear();
                cmbFormName.Items.Add(""); 
                foreach (View view in project.Views)
                {
                    cmbFormName.Items.Add(view.Name);
            }
        }
        }

        private void txtStandalonePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmbStandaloneFormName.Items.Clear();
            if (System.IO.File.Exists(txtStandalonePath.Text))
            {
                Epi.Data.IDbDriver database = Epi.Data.DBReadExecute.GetDataDriver(txtStandalonePath.Text);

                List<string> tableNames = database.GetTableNames();
                cmbStandaloneFormName.Items.Clear();
                foreach (string tableName in tableNames)
                {
                    cmbStandaloneFormName.Items.Add(tableName);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnStandaloneBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            string fileExten = System.IO.Path.GetExtension(DashboardHelper.Database.DataSource);
            openFileDialog.DefaultExt = fileExten;

            switch (fileExten.ToLowerInvariant())
            {
                case ".xlsx":
                    openFileDialog.Filter = "Microsoft Excel 2007 (*.xlsx)|*.xlsx";
                    break;
                case ".xls":
                    openFileDialog.Filter = "Microsoft Excel 97-2003 Workbook (*.xls)|*.xls";
                    break;
                case ".accdb":
                    openFileDialog.Filter = "Microsoft Access 2007 (*.accdb)|*.accdb";
                    break;
                case ".mdb":
                    openFileDialog.Filter = "Microsoft Access 2002-2003 (*.mdb)|*.mdb";
                    break;
                default:
                    openFileDialog.Filter = "All Files (*.*)|*.*";
                    break;
            }

            if (!String.IsNullOrEmpty(txtProjectPath.Text))
            {
                openFileDialog.FileName = txtProjectPath.Text;
            }
            else
            {
                openFileDialog.InitialDirectory = this.DashboardHelper.Config.Directories.Project;
            }

            if (openFileDialog.ShowDialog().Value)
            {
                txtStandalonePath.Text = openFileDialog.FileName;
            }
        }

        private void cmbFormName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFormName.SelectedIndex >= 0)
            {
                cmbFormName.Text = cmbFormName.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(cmbFormName.Text))
                btnOK.IsEnabled = true;
            }
        }

        private void cmbStandaloneFormName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStandaloneFormName.SelectedIndex >= 0)
            {
                cmbStandaloneFormName.Text = cmbStandaloneFormName.SelectedItem.ToString();
            }
        }

        private void cmbSqlTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSqlTable.SelectedIndex >= 0)
            {
                cmbSqlTable.Text = cmbSqlTable.SelectedItem.ToString();
            }
        }

    }
}
