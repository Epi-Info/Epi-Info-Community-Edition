using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
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
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Mapping;
using System.Net;
using System.Windows.Forms;
namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DotDensityProperties.xaml
    /// </summary>
    public partial class DotDensityProperties : System.Windows.Controls.UserControl
    {      
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        private EpiDashboard.Mapping.DotDensityLayerProvider provider;       
        public RowFilterControl rowFilterControl { get; protected set; }                  
        public DataFilters dataFilters;
        private System.Xml.XmlElement currentElement;
        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        private EpiDashboard.Mapping.DotDensityKmlLayerProvider KMLprovider;
        private EpiDashboard.Mapping.DotDensityServerLayerProvider Mapprovider;
        public EpiDashboard.Mapping.DotDensityLayerProperties layerprop;
        public EpiDashboard.Mapping.DotDensityServerLayerProperties serverlayerprop;
        public EpiDashboard.Mapping.DotDensityKmlLayerProperties kmllayerprop;       
        private string shapeFilePath;
        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        # region Public Properties
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

        public string MapServerName { get; set; }
        public int MapVisibleLayer { get; set; }
        public string KMLMapServerName { get; set; }
        public int KMLMapVisibleLayer { get; set; }
        #endregion

        # region Constructors
        public DotDensityProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            this.mapControl = mapControl;
            this.myMap = myMap;           
            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;               
        }

        #endregion

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

        private HttpWebRequest CreateWebRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            return request;
        }

        public string GetMessage(string endPoint)
        {
            HttpWebRequest request = CreateWebRequest(endPoint);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        private void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();
                if (radShapeFile.IsChecked == true && provider!=null)
                    provider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));
                else if (radMapServer.IsChecked == true && Mapprovider!=null)
                    Mapprovider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));
                else if (radKML.IsChecked == true && KMLprovider!=null)
                    KMLprovider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));
            }
        }
       
        private void Addfilters()
        {
            string sfilterOperand = string.Empty;
            string[] shilowvars;
            string svarname;

            this.dataFilters = rowFilterControl.DataFilters;

            List<string> sconditionval = dataFilters.GetFilterConditionsAsList();
            string strreadablecondition = dataFilters.GenerateReadableDataFilterString().Trim();
            if (!(string.IsNullOrEmpty(strreadablecondition)))
            {
                if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_MISSING;
                }

                if (!(strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN)))
                {
                    svarname = strreadablecondition.Substring(strreadablecondition.IndexOf("[") + 1, strreadablecondition.IndexOf("]") - strreadablecondition.IndexOf("[") - 1);
                    dashboardHelper.AddDataFilterCondition(sfilterOperand, sconditionval[0].ToString(), svarname, ConditionJoinType.And);
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_BETWEEN;
                    string strcondition = strreadablecondition.Substring(0, strreadablecondition.IndexOf(sfilterOperand)).Trim();
                    string[] strVarstrings = strcondition.Split(' ');
                    svarname = strVarstrings[3].ToString();
                    string sValues = strreadablecondition.ToString().Substring(strreadablecondition.IndexOf(sfilterOperand) + sfilterOperand.Length, (strreadablecondition.ToString().Length) - (strreadablecondition.ToString().IndexOf(sfilterOperand) + sfilterOperand.Length)).Trim();
                    shilowvars = sValues.Split(' ');
                    dashboardHelper.AddDataFilterCondition(sfilterOperand, shilowvars[0].ToString(), shilowvars[2].ToString(), svarname, ConditionJoinType.And);
                }
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;                          
            panelFilters.Visibility = System.Windows.Visibility.Visible;           
        }
               
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Addfilters();
            RenderMap();
            if (radShapeFile.IsChecked==true && provider!=null)
            layerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            else if (radMapServer.IsChecked == true && Mapprovider!=null)
                serverlayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            else if (radKML.IsChecked == true && KMLprovider!=null)
            {
                kmllayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            }
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
            this.DashboardHelper = dashboardHelper;                       
            if (dashboardHelper != null)
            {
                txtProjectPath.Text = dashboardHelper.Database.DbName;
                FillComboBoxes();
                panelBoundaries.IsEnabled = true;
                this.dataFilters = new DataFilters(dashboardHelper);
                rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dataFilters, true);
                rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; rowFilterControl.FillSelectionComboboxes();
                panelFilters.Children.Add(rowFilterControl);
                txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
            }
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
            provider = new Mapping.DotDensityLayerProvider(myMap);           
            object[] shapeFileProperties = provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
            ILayerProperties layerProperties = null;
            layerProperties = new DotDensityLayerProperties(myMap, dashboardHelper, this.mapControl);
            layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
            this.layerprop = (DotDensityLayerProperties)layerProperties;
            layerprop.provider = provider;
            if (this.DashboardHelper != null)
                layerprop.SetdashboardHelper(DashboardHelper);
            this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
           
                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    layerprop.shapeFilePath = shapeFileProperties[0].ToString();
                    IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear(); 
                        layerprop.cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            layerprop.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
           
        }     

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }     

        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {          
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML Files (*.kml)|*.kml|KMZ Files (*.kmz)|*.kmz";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
                if (KMLprovider == null)
                {
                    KMLprovider = new Mapping.DotDensityKmlLayerProvider(myMap);
                    KMLprovider.FeatureLoaded += new FeatureLoadedHandler(KMLprovider_FeatureLoaded);
                }               
                object[] kmlFileProperties = KMLprovider.LoadKml(KMLMapServerName);
                if (kmlFileProperties != null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new DotDensityKmlLayerProperties(myMap, dashboardHelper, this.mapControl);
                    layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                    this.kmllayerprop = (DotDensityKmlLayerProperties)layerProperties;
                    kmllayerprop.shapeFilePath = KMLMapServerName;
                    kmllayerprop.provider = KMLprovider;
                    kmllayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(kmllayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        kmllayerprop.SetdashboardHelper(DashboardHelper);
                    this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
            }

        }

        private void btnMapFile_Click(object sender, RoutedEventArgs e)
        {
          /*  Mapprovider = new Mapping.DotDensityServerLayerProvider(myMap);
            Mapprovider.FeatureLoaded += new FeatureLoadedHandler(Mapprovider_FeatureLoaded);
              object[] mapFileProperties= Mapprovider.LoadShapeFile();
              if (mapFileProperties != null)
              {
                  ILayerProperties layerProperties = null;
                  layerProperties = new DotDensityServerLayerProperties(myMap, dashboardHelper, this.mapControl);
                  layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                  layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                  this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
                  if (this.DashboardHelper != null)
                      serverlayerprop.SetdashboardHelper(DashboardHelper);
                  this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
              }*/
        }

        void Mapprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    serverlayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        serverlayerprop.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                       cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                       rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                RenderMap();
            }
        }

        void KMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    kmllayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        kmllayerprop.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                RenderMap();
            }
        }

        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (Mapprovider != null)
            {
                Mapprovider.FeatureLoaded -= new FeatureLoadedHandler(Mapprovider_FeatureLoaded);               
                Mapprovider = null;
            }
            if (KMLprovider != null)
            {
                KMLprovider.FeatureLoaded -= new FeatureLoadedHandler(KMLprovider_FeatureLoaded);               
                KMLprovider = null;
            }
        }                   

        private void radKML_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (Mapprovider != null)
            {
                Mapprovider.FeatureLoaded -= new FeatureLoadedHandler(Mapprovider_FeatureLoaded);              
                Mapprovider = null;
            }
            if (provider != null)
            {
               // provider.FeatureLoaded -= new FeatureLoadedHandler(provider_FeatureLoaded);
                provider = null;               
            }
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (KMLprovider != null)
            {
                KMLprovider.FeatureLoaded -= new FeatureLoadedHandler(KMLprovider_FeatureLoaded);
                KMLprovider = null;
            }
            if (provider != null)
            {              
                provider = null;
            }
        }
           
        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        private void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxmapserver.SelectedIndex > -1)
            {
                MapServerName = ((ComboBoxItem)cbxmapserver.SelectedItem).Content.ToString();
                if (Mapprovider == null)
                {
                    Mapprovider = new Mapping.DotDensityServerLayerProvider(myMap);
                    Mapprovider.FeatureLoaded += new FeatureLoadedHandler(Mapprovider_FeatureLoaded);
                }
                object[] mapFileProperties = Mapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);
                if (mapFileProperties != null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new DotDensityServerLayerProperties(myMap, dashboardHelper, this.mapControl);
                    layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                    this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
                    serverlayerprop.shapeFilePath = MapServerName;
                    serverlayerprop.provider = Mapprovider;
                    serverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(serverlayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        serverlayerprop.SetdashboardHelper(DashboardHelper);
                    this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
            }
        }

        private void radlocatemapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
            cbxmapserver.SelectedIndex = -1;
        }

        private void btnMapserverlocate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                cbxmapfeature.ItemsSource = rest.layers;
            }
            catch (Exception ex)
            {
                cbxmapfeature.DataContext = null;
                System.Windows.Forms.MessageBox.Show("Invalid map server");
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        private void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxmapfeature.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                if (cbxmapfeature.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxmapfeature.SelectedItem).id;
                    MapVisibleLayer = visibleLayer;
                    if (Mapprovider == null)
                    {
                        Mapprovider = new Mapping.DotDensityServerLayerProvider(myMap);
                        Mapprovider.FeatureLoaded += new FeatureLoadedHandler(Mapprovider_FeatureLoaded);
                    }
                    object[] mapFileProperties = Mapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);                           
                    if (mapFileProperties != null)
                    {
                        ILayerProperties layerProperties = null;
                        layerProperties = new DotDensityServerLayerProperties(myMap, dashboardHelper, this.mapControl);
                        layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                        layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                        this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
                        serverlayerprop.shapeFilePath = MapServerName;
                        serverlayerprop.provider = Mapprovider;
                        serverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(serverlayerprop.provider_FeatureLoaded);
                        if (this.DashboardHelper != null)
                            serverlayerprop.SetdashboardHelper(DashboardHelper);
                        this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }
                }
                else
                {
                    MapVisibleLayer = -1;
                }
            }
        }
              
    }
}
