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
    public partial class ChoroplethProperties : UserControl
    {

        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        private EpiDashboard.Mapping.ChoroplethLayerProvider provider;

        public ChoroplethProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            this.mapControl = mapControl;
            this.myMap = myMap;
            ////this.DashboardHelper = dashboardHelper;

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    //txtProjectPath.Text = dashboardHelper.View.Project.FilePath;

            //    if (System.IO.File.Exists(txtProjectPath.Text))
            //    {
            //        cmbFormName.Items.Clear();
            //        Project project = new Project(txtProjectPath.Text);
            //        foreach (View view in project.Views)
            //        {
            //            cmbFormName.Items.Add(view.Name);
            //        }
            //    }

            //    //cmbFormName.Text = dashboardHelper.View.Name;
            //}
            //else
            //{
            //    //if (!string.IsNullOrEmpty(dashboardHelper.CustomQuery))
            //    //{
            //    //    SqlQuery = DashboardHelper.CustomQuery;
            //    //}
            //}

            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + " unfiltered rows";
            //tblockColumns.Text = dashboardHelper.DataSet.Tables[0].Columns.Count.ToString() + " columns";
            //tblockCacheDateTime.Text = "Data last cached at " + dashboardHelper.LastCacheTime.ToShortDateString() + " " + dashboardHelper.LastCacheTime.ToShortTimeString();
            //tblockCacheTimeElapsed.Text = "Took " + dashboardHelper.TimeToCache + " to locally cache data";

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            //lbxRelatedDataSources.Items.Clear();
            //if (dashboardHelper.ConnectionsForRelate.Count > 0)
            //{
            //    // Related Data
            //    foreach (RelatedConnection rConn in dashboardHelper.ConnectionsForRelate)
            //    {
            //        lbxRelatedDataSources.Items.Add(rConn.db.ConnectionString);
            //    }
            //}
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
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }
        

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
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
            dashboardHelper = mapControl.GetNewDashboardHelper();
            if (dashboardHelper != null)
            {
                txtProjectPath.Text = dashboardHelper.Database.DbName;
                FillComboBoxes();
            }
        }

        private void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                cmbDataKey.Items.Add(field);
            }
            foreach (string field in numericFields)
            {
                cmbValue.Items.Add(field);
            }
            cmbValue.Items.Insert(0, "{Record Count}");
        }

        private void cmbFormName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cmbFormName.SelectedIndex >= 0)
            //{
            //    cmbFormName.Text = cmbFormName.SelectedItem.ToString();
            //}
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //cmbFormName.Items.Clear();
            //if (System.IO.File.Exists(txtProjectPath.Text))
            //{
            //    Project project = new Project(txtProjectPath.Text);
            //    foreach (View view in project.Views)
            //    {
            //        cmbFormName.Items.Add(view.Name);
            //    }
            //}
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            provider = new Mapping.ChoroplethLayerProvider(myMap);
            object[] shapeFileProperties = provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                provider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), new List<SolidColorBrush>() { (SolidColorBrush)rctColor1.Fill, (SolidColorBrush)rctColor2.Fill, (SolidColorBrush)rctColor3.Fill, (SolidColorBrush)rctColor4.Fill, (SolidColorBrush)rctColor5.Fill, (SolidColorBrush)rctColor6.Fill }, int.Parse(cmbClasses.Text.ToString()));
                //if (MapGenerated != null)
                //{
                //    MapGenerated(this, new EventArgs());
                //}
            }
        }

        private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor1.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor2.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor3.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor4.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor5.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor6.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }
    }
}
