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
        public EpiDashboard.Mapping.DotDensityLayerProvider provider;       
        //public RowFilterControl rowFilterControl { get; protected set; }                  
        public RowFilterControl rowFilterControl { get; set; }  
        public DataFilters dataFilters;
        private System.Xml.XmlElement currentElement;
        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        public EpiDashboard.Mapping.DotDensityKmlLayerProvider KMLprovider;
        public EpiDashboard.Mapping.DotDensityServerLayerProvider Mapprovider;
        public EpiDashboard.Mapping.DotDensityLayerProperties layerprop;
        public EpiDashboard.Mapping.DotDensityServerLayerProperties serverlayerprop;
        public EpiDashboard.Mapping.DotDensityKmlLayerProperties kmllayerprop;       
        private string shapeFilePath;
        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;
        public  IDictionary<string, object> shapeAttributes;
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

            #region Translation

            //Dot Density Left Panel
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_DOTDENSITY;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.GADGET_TABDESC_DATASOURCE;
            tbtnHTML.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnHTML.Description = DashboardSharedStrings.GADGET_TABDESC_MAP_VARIABLES;
            tbtnCharts.Title = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            tbtnCharts.Description = DashboardSharedStrings.GADGET_TABDESC_DOT_COLOR;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblBoundaries.Content = DashboardSharedStrings.GADGET_BOUNDARIES;
            radShapeFile.Content = DashboardSharedStrings.GADGET_SHAPEFILE;
            btnBrowseShape.Content = DashboardSharedStrings.BUTTON_BROWSE;
            radMapServer.Content = DashboardSharedStrings.GADGET_MAPSERVER;
            radconnectmapserver.Content = DashboardSharedStrings.GADGET_CONNECT_MAPSERVER;
            radlocatemapserver.Content = DashboardSharedStrings.GADGET_OTHER_MAPSERVER;
            lblURL.Content = DashboardSharedStrings.GADGET_URL;
            btnMapserverlocate.Content = DashboardSharedStrings.BUTTON_CONNECT;
            lblExampleMapServerURL.Content = DashboardSharedStrings.GADGET_EXAMPLE_MAPSERVER;
            lblSelectFeature.Content = DashboardSharedStrings.GADGET_SELECT_FEATURE;
            radKML.Content = DashboardSharedStrings.GADGET_KMLFILE;
            lblURLOfKMLFile.Content = DashboardSharedStrings.GADGET_KMLFILE_LOCATION;



            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;

            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tblockSelectVarData.Content = DashboardSharedStrings.GADGET_VARIABLES_CHORPLTH;
            lblFeature.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblData.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValue.Content = DashboardSharedStrings.GADGET_MAP_VALUE;


            //Colors and Styles Panel
            lblPanelHdrColorsAndStyles.Content = DashboardSharedStrings.GADGET_TAB_COLORS_RANGES;

            tblcolor.Content = DashboardSharedStrings.GADGET_SELECT_COLOR;
            lblDotValue.Content = DashboardSharedStrings.GADGET_MAP_DOT_VALUE;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_MAP_SELECT_DATASOURCE;

            //Info Panel
            //lblCanvasInfo.Content = DashboardSharedStrings.GADGET_CANVAS_INFO;
            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + DashboardSharedStrings.GADGET_INFO_UNFILTERED_ROWS;


            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion // Translation
         
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

        public void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = this.dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "FKEY" || field.ToUpper() == "GLOBALRECORDID" || field.ToUpper() == "UNIQUEKEY" || field.ToUpper()== "FIRSTSAVETIME" || field.ToUpper() == "LASTSAVETIME" || field.ToUpper() == "SYSTEMDATE"))
                { cmbDataKey.Items.Add(field);}
            }
            foreach (string field in numericFields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "UNIQUEKEY"))
                { cmbValue.Items.Add(field); }
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
                    provider.SetShapeRangeValues(this.DashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctDotColor.Fill).Color, int.Parse(txtDotValue.Text));
                else if (radMapServer.IsChecked == true && Mapprovider!=null)
                    Mapprovider.SetShapeRangeValues(this.DashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctDotColor.Fill).Color, int.Parse(txtDotValue.Text));
                else if (radKML.IsChecked == true && KMLprovider!=null)
                    KMLprovider.SetShapeRangeValues(this.DashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctDotColor.Fill).Color, int.Parse(txtDotValue.Text));
            }
        }
       
        private void Addfilters()
        {
            this.dataFilters = rowFilterControl.DataFilters;
            if (radShapeFile.IsChecked == true && this.layerprop != null) { layerprop.datafilters = rowFilterControl.DataFilters; }
            if (radMapServer.IsChecked == true && this.serverlayerprop != null) {serverlayerprop.datafilters = rowFilterControl.DataFilters; }
            if (radKML.IsChecked == true && kmllayerprop != null) { kmllayerprop.datafilters = rowFilterControl.DataFilters; }


            this.dataFilters = rowFilterControl.DataFilters;
            dashboardHelper.SetDatafilters(this.dataFilters);

            /*
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
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO;
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_OR))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_OR;
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LIKE))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LIKE;
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_AND))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_AND;
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF;
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF;
                    }
                    if (!(strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN)))
                    {
                        svarname = strreadablecondition.Substring(strreadablecondition.IndexOf("[") + 1, strreadablecondition.IndexOf("]") - strreadablecondition.IndexOf("[") - 1);
                        this.dashboardHelper.AddDataFilterCondition(sfilterOperand, sconditionval[0].ToString(), svarname, ConditionJoinType.And);
                    }
                    else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                    {
                        sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_BETWEEN;
                        string strcondition = strreadablecondition.Substring(0, strreadablecondition.IndexOf(sfilterOperand)).Trim();
                        string[] strVarstrings = strcondition.Split(' ');
                        svarname = strVarstrings[3].ToString();
                        string sValues = strreadablecondition.ToString().Substring(strreadablecondition.IndexOf(sfilterOperand) + sfilterOperand.Length, (strreadablecondition.ToString().Length) - (strreadablecondition.ToString().IndexOf(sfilterOperand) + sfilterOperand.Length)).Trim();
                        shilowvars = sValues.Split(' ');
                        this.dashboardHelper.AddDataFilterCondition(sfilterOperand, shilowvars[0].ToString(), shilowvars[2].ToString(), svarname, ConditionJoinType.And);
                    }
              } */
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
            if (serverlayerprop != null)
                if (serverlayerprop.provider.FlagUpdateToGLFailed) { ResetShapeCombo(); }
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
            if (cmbDataKey.SelectedIndex > -1 && cmbShapeKey.SelectedIndex > -1 && cmbValue.SelectedIndex > -1)
            {
                if (radShapeFile.IsChecked == true && provider != null)
                {
                    Addfilters();
                    RenderMap();
                    layerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctDotColor.Fill));
                }
                else if (radMapServer.IsChecked == true && Mapprovider != null)
                {
                    Addfilters();
                    RenderMap();
                    serverlayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctDotColor.Fill));
                    serverlayerprop.cbxMapserverText = cbxmapserver.Text;
                    serverlayerprop.txtMapserverText = txtMapSeverpath.Text;
                    serverlayerprop.cbxMapFeatureText = cbxmapfeature.Text;
                }
                else if (radKML.IsChecked == true && KMLprovider != null)
                {
                    Addfilters();
                    RenderMap();
                    kmllayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctDotColor.Fill));
                }
                if (ChangesAccepted != null)
                {
                    ChangesAccepted(this, new EventArgs());
                }
            }
            else
                tbtnDataSource.IsChecked = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        public void SetDashboardHelper(DashboardHelper dash)
        {
            dashboardHelper = dash;
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            dashboardHelper = mapControl.GetNewDashboardHelper();
            if (dashboardHelper != null)
            {
                this.DashboardHelper = dashboardHelper;
                txtProjectPath.Text = mapControl.ProjectFilepath;
                FillComboBoxes();
                panelBoundaries.IsEnabled = true;
                this.dataFilters = new DataFilters(dashboardHelper);
                rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dataFilters, true);
                rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; rowFilterControl.FillSelectionComboboxes();
                panelFilters.Children.Add(rowFilterControl);
                //txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
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
                if (layerprop == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new DotDensityLayerProperties(myMap, this.DashboardHelper, this.mapControl);
                    layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this.mapControl.ILayerProperties_EditRequested);
                    this.layerprop = (DotDensityLayerProperties)layerProperties;
                    this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }

                layerprop.provider = provider;
                if (this.DashboardHelper != null)
                layerprop.SetdashboardHelper(DashboardHelper);
                        
                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    layerprop.shapeFilePath = shapeFileProperties[0].ToString();
                    layerprop.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    //IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                   shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear(); 
                        //layerprop.cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            //layerprop.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
           
        }     

        private void rctDotColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();           
            Brush brush = new SolidColorBrush(((SolidColorBrush)rctDotColor.Fill).Color);
            dialog.Color = System.Drawing.Color.FromArgb(((brush as SolidColorBrush).Color).A, ((brush as SolidColorBrush).Color).R, ((brush as SolidColorBrush).Color).G, ((brush as SolidColorBrush).Color).B); ;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctDotColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));               
                
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
                }

                if (KMLprovider == null)
                {
                    KMLprovider = new Mapping.DotDensityKmlLayerProvider(myMap);
                    KMLprovider.FeatureLoaded += new FeatureLoadedHandler(KMLprovider_FeatureLoaded);
                }
            
                object[] kmlFileProperties = KMLprovider.LoadKml(KMLMapServerName);
                if (kmlFileProperties != null)
                {
                    if (kmllayerprop == null)
                    {
                        ILayerProperties layerProperties = null;
                        layerProperties = new DotDensityKmlLayerProperties(myMap, this.DashboardHelper, this.mapControl);
                        layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                        layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                        layerProperties.EditRequested += new EventHandler(this.mapControl.ILayerProperties_EditRequested);
                        this.kmllayerprop = (DotDensityKmlLayerProperties)layerProperties;
                        this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }
                    kmllayerprop.shapeFilePath = KMLMapServerName;
                    kmllayerprop.provider = KMLprovider;
                    kmllayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(kmllayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        kmllayerprop.SetdashboardHelper(DashboardHelper);
                    
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
                  layerProperties = new DotDensityServerLayerProperties(myMap, this.DashboardHelper, this.mapControl);
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
                       rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
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
                        rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                RenderMap();
            }
        }

        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                return;
            }
            else
                ClearonShapeFile();                  
        }

        private void ClearonKML()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
            txtMapSeverpath.Text = string.Empty;
            if (Mapprovider != null)
            {
                Mapprovider.FeatureLoaded -= new FeatureLoadedHandler(Mapprovider_FeatureLoaded);
                Mapprovider = null;
            }
            if (provider != null)
            {               
                provider = null;
            }
        }

        private void ClearonShapeFile()
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
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

        private void ClearonMapServer()
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

        private void ResetShapeCombo()
        {
             //In case of map server
            if (radMapServer.IsChecked == true)
            {
                cmbShapeKey.Items.Clear();
                cmbShapeKey.SelectedIndex = -1;
                if (serverlayerprop != null)
                {
                    serverlayerprop.cbxShapeKey.Items.Clear();
                    serverlayerprop.cbxShapeKey.SelectedIndex = -1;
                }
            }
         
        }
      
        private void radKML_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                return;
            }
            else
                ClearonKML();            
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonMapServer();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonMapServer();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                return;
            }
            else
                ClearonMapServer();      
        }
           
        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        public void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetMapServer();
        }

        public void ResetMapServer()
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
                    if (this.serverlayerprop == null)
                    {
                        ILayerProperties layerProperties = null;
                        layerProperties = new DotDensityServerLayerProperties(myMap, this.DashboardHelper, this.mapControl);
                        layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                        layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                        layerProperties.EditRequested += new EventHandler(this.mapControl.ILayerProperties_EditRequested);
                        this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
                        this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }
                    serverlayerprop.shapeFilePath = MapServerName;
                    serverlayerprop.provider = Mapprovider;
                    serverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(serverlayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        serverlayerprop.SetdashboardHelper(DashboardHelper);
                   
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
            MapServerConnect();
        }

        public void MapServerConnect()
        {
            try
            {
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                cbxmapfeature.ItemsSource = rest.layers;
                if (rest.layers.Count > 0)
                    cbxmapfeature.SelectedIndex = 0;
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

        public void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MapfeatureSelectionChange();
        }

        public void MapfeatureSelectionChange()
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
                        if (serverlayerprop == null)
                        {
                            ILayerProperties layerProperties = null;
                            layerProperties = new DotDensityServerLayerProperties(myMap, this.DashboardHelper, this.mapControl);
                            layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                            layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
                            layerProperties.EditRequested += new EventHandler(this.mapControl.ILayerProperties_EditRequested);
                            this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
                            this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                        }
                        serverlayerprop.shapeFilePath = MapServerName;
                        serverlayerprop.provider = Mapprovider;
                        serverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(serverlayerprop.provider_FeatureLoaded);
                        if (this.DashboardHelper != null)
                            serverlayerprop.SetdashboardHelper(DashboardHelper);
                       
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
