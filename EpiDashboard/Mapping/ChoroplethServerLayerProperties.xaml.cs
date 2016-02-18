using System;
using System.Collections.Generic;
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
using Epi;
using Epi.Fields;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for ChoroplethLayerProperties.xaml
    /// </summary>
    public partial class ChoroplethServerLayerProperties : ChoroplethLayerPropertiesUserControlBase, ILayerProperties
    {
        private ChoroplethServerLayerProvider _provider;
        public ChoroplethServerLayerProvider Provider
        {
            set { _provider = value; }
            get { return _provider; }
        }

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

       // public IDictionary<string, object> shapeAttributes;
        private bool flagrunedit;
        private int Numclasses;
        public DataFilters datafilters { get; set; }

        public string ShapeFilePath;
        public string cbxMapserverText;
        public string txtMapserverText;
        public string cbxMapFeatureText;

        private System.Xml.XmlElement currentElement;

        private byte opacity;
        public byte Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        public IDictionary<string, object> curfeatureAttributes;

        public ChoroplethServerLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

           // Provider = new ChoroplethServerLayerProvider(myMap);
           // Provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);

            FillComboBoxes();
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxClasses.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);

            rctEdit.MouseUp += new MouseButtonEventHandler(rctEdit_MouseUp);

            #region translation;
            lblTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
            lblClasses.Content = DashboardSharedStrings.GADGET_CLASSES;
            lblShapeKey.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblDataKey.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValueField.Content = DashboardSharedStrings.GADGET_MAP_VALUE;
            lblMissingValueColor.Content = DashboardSharedStrings.GADGET_MAP_COLOR_MISSING;
            lblLoValueColor.Content = DashboardSharedStrings.GADGET_LOW_VALUE_COLOR;
            lblHiValueColor.Content = DashboardSharedStrings.GADGET_HIGH_VALUE_COLOR;
            rctEditToolTip.Content = DashboardSharedStrings.MAP_LAYER_EDIT;
            #endregion; 
        }

        private void rctMissingColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                ChoroplethProperties_RenderMap();
            }
        }

        public void provider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                boundryFilePath = serverName;
                if (featureAttributes != null)
                {
                    curfeatureAttributes = featureAttributes;
                    cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cbxShapeKey.Items.Add(key);
                    }
                }
            }
            
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cbxDataKey.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                        cbxDataKey.SelectedItem = child.InnerText;
                        cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cbxShapeKey.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                        cbxShapeKey.SelectedItem = child.InnerText;
                        cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                    }
                    if (child.Name.Equals("value"))
                    {
                        cbxValue.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                        cbxValue.SelectedItem = child.InnerText;
                        cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                    }
                    if (child.Name.Equals("classes"))
                    {
                        cbxClasses.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                        cbxClasses.SelectedIndex = int.Parse(child.InnerText) + 1;
                        cbxClasses.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                    }
                    if (child.Name.Equals("highColor"))
                    {
                        rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                    if (child.Name.Equals("lowColor"))
                    {
                        rctLowColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                    if (child.Name.Equals("selectMapFeature"))
                    {
                        cbxMapFeatureText = child.InnerText;
                    }
                }
                
               ChoroplethProperties_RenderMap();
            }
        }

        void rctFilter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FilterRequested != null)
            {
                FilterRequested(this, new EventArgs());
            }
        }

        public void MoveUp()
        {
            Provider.MoveUp();
        }

        public void MoveDown()
        {
            Provider.MoveDown();
        }

        void rctLowColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                ChoroplethProperties_RenderMap();
            }
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                ChoroplethProperties_RenderMap();
            }
        }

        private void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested != null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
            }
        }

        private void ChoroplethProperties_RenderMap()
        {
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
                base.ChoroplethProperties_RenderMap(Provider);
                
                //Provider.SetShapeRangeValues(
                //    dashboardHelper,
                //    cbxShapeKey.SelectedItem.ToString(),
                //    cbxDataKey.SelectedItem.ToString(),
                //    cbxValue.SelectedItem.ToString(),
                //    null,
                //    int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()),
                //    "",
                //    "");
                
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }
               
        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }

        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
        }
        public StackPanel LegendStackPanel
        {
            get
            {
                return Provider.LegendStackPanel;
            }
        }

        public DashboardHelper GetDashboardHelper()
        {
            return this.dashboardHelper;
        }

        void mapControl_MapDataChanged(object sender, EventArgs e)
        {
            Provider.Refresh();
        }

        void keys_SelectionChanged(object sender, EventArgs e)
        {
            ChoroplethProperties_RenderMap();
        }

        void btnShapeFile_Click(object sender, RoutedEventArgs e)
        {
            Provider.Load();
        }

        private void FillComboBoxes()
        {
            cbxDataKey.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxDataKey.Items.Clear();
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);

            cbxValue.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.Items.Clear();
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); // dashboardHelper.GetFormFields();
            columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            
            foreach (string field in fields)
            {
                cbxDataKey.Items.Add(field);
            }
            foreach (string field in numericFields)
            {
                cbxValue.Items.Add(field);
            }
            
            cbxValue.Items.Insert(0, "{Record Count}");
        }

        #region ILayerProperties Members

        public void CloseLayer()
        {
            Provider.CloseLayer();
        }

        public Color FontColor
        {
            set
            {
                SolidColorBrush brush = new SolidColorBrush(value);
                lblDataKey.Foreground = brush;
                lblShapeKey.Foreground = brush;
                lblValueField.Foreground = brush;
            }
        }

        public void SetValues(string shapekey, string datakey, string val, string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes, Dictionary<int, object> classAttrib, bool flagquintiles, int numclasses, byte opacity)
        {
            FillComboBoxes();
            
            if (shapeAttributes != null)
            {
                cbxShapeKey.Items.Clear();
                foreach (string key in shapeAttributes.Keys)
                {
                    cbxShapeKey.Items.Add(key);
                }
            }
            
            ClassAttributeList = classAttrib;
            partitionSetUsingQuantiles = flagquintiles;
            Numclasses = numclasses;
            rctHighColor.Fill = (SolidColorBrush)Highcolor;
            rctLowColor.Fill = (SolidColorBrush)Lowcolor;
            rctMissingColor.Fill = (SolidColorBrush)Missingcolor;
            cbxClasses.Text = classes;
            cbxShapeKey.Text = shapekey;
            cbxDataKey.Text = datakey;

            cbxValue.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.Text = val;
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);

            grdMain.Width = 700;
            Opacity = opacity;
        }

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            cbxDataKey.IsEnabled = false;
            cbxShapeKey.IsEnabled = false;
            cbxValue.IsEnabled = false;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }

        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string connectionString = string.Empty;
            string tableName = string.Empty;
            string projectPath = string.Empty;
            string viewName = string.Empty;

            string legacyClassCountIndex = (cbxClasses.SelectedIndex - 1).ToString();
            
            if (dashboardHelper.View == null)
            {
                connectionString = dashboardHelper.Database.ConnectionString;
                tableName = dashboardHelper.TableName;
            }
            else
            {
                projectPath = dashboardHelper.View.Project.FilePath;
                viewName = dashboardHelper.View.Name;
            }
            
            string dataKey = cbxDataKey.SelectedItem.ToString();
            string shapeKey = cbxShapeKey.SelectedItem.ToString();
            string value = cbxValue.SelectedItem.ToString();
            SolidColorBrush highColor = (SolidColorBrush)rctHighColor.Fill;
            SolidColorBrush lowColor = (SolidColorBrush)rctLowColor.Fill;
            string xmlString = "<shapeFile>" + boundryFilePath + "</shapeFile><highColor>" + highColor.Color.ToString() + "</highColor><lowColor>" + lowColor.Color.ToString() + "</lowColor><classes>" + legacyClassCountIndex + "</classes><dataKey>" + dataKey + "</dataKey><shapeKey>" + shapeKey + "</shapeKey><value>" + value + "</value>" + "<selectMapFeature>" + cbxMapFeatureText + "</selectMapFeature> ";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.ChoroplethServerLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        private string GetformattedUrl(string sUrl)
        {
            if (cbxMapserverText == "NationalMap.gov - New York County Boundaries" || cbxMapserverText == "NationalMap.gov - Rhode Island Zip Code Boundaries" || cbxMapserverText == "NationalMap.gov - U.S. State Boundaries" || cbxMapserverText == "NationalMap.gov - World Boundaries")
            { sUrl = cbxMapserverText + "/0"; }

            return sUrl;

        }

        private void SetValuesFromUrl(string sPath)
        {
               if (sPath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/13" && string.IsNullOrEmpty(cbxMapFeatureText) == true )
                   cbxMapserverText = "NationalMap.gov - New York County Boundaries";
               else if (sPath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/19" && string.IsNullOrEmpty(cbxMapFeatureText) == true) 
                   cbxMapserverText = "NationalMap.gov - Rhode Island Zip Code Boundaries";
               else if (sPath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/17" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
                    cbxMapserverText = "NationalMap.gov - U.S. State Boundaries";
               else if (sPath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/tnm_blank_us/mapserver/17" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
                   cbxMapserverText = "NationalMap.gov - World Boundaries";
                  
                else
                {
                    string lastchar = sPath.Substring(sPath.Length - 1, 1);
                    string lastonebeforechar = sPath.Substring(sPath.Length - 2, 1);
                    if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar,0) == false)
                        txtMapserverText = sPath.Substring(0, sPath.Length - 2);
                    else if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == true)
                       txtMapserverText = sPath.Substring(0, sPath.Length - 3);
                }
             
           }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            currentElement = element;

            if (Provider == null)
            {
                Provider = new ChoroplethServerLayerProvider(myMap);
                Provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);
            }

            base.CreateFromXml(element, Provider);

            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile"))
                {
                    object[] shapeFileProperties = Provider.Load(child.InnerText);
                    if (shapeFileProperties != null)
                    {
                        if (shapeFileProperties.Length == 2)
                        {
                            boundryFilePath = shapeFileProperties[0].ToString();
                            FeatureLayer graphicsLayer = (FeatureLayer)shapeFileProperties[1];

                        //    if (shapeAttributes != null)
                        //    {
                        //        cbxShapeKey.Items.Clear();
                        //        foreach (string key in shapeAttributes.Keys)
                        //        {
                        //            cbxShapeKey.Items.Add(key);
                        //        }
                        //    }
                        }
                    }
                }
            }

            string shapefileurl="";

            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile"))
                {
                   shapefileurl = child.InnerText; 
                }
                
                if (child.Name.Equals("selectMapFeature"))
                {
                    cbxMapFeatureText = child.InnerText;
                }
            }

            if (shapefileurl.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/13" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
            {
                cbxMapserverText = "NationalMap.gov - New York County Boundaries";
            }
            else if (shapefileurl.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/19" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
            {
                cbxMapserverText = "NationalMap.gov - Rhode Island Zip Code Boundaries";
            }
            else if (shapefileurl.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/17" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
            {
                cbxMapserverText = "NationalMap.gov - U.S. State Boundaries";
            }
            else if (shapefileurl.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/tnm_blank_us/mapserver/17" && string.IsNullOrEmpty(cbxMapFeatureText) == true)
            {
                cbxMapserverText = "NationalMap.gov - World Boundaries";
            }
            else
            {
                string lastchar = shapefileurl.Substring(shapefileurl.Length - 1, 1);
                string lastonebeforechar = shapefileurl.Substring(shapefileurl.Length - 2, 1);

                if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == false)
                {
                    txtMapserverText = shapefileurl.Substring(0, shapefileurl.Length - 2);
                }
                else if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == true)
                {
                    txtMapserverText = shapefileurl.Substring(0, shapefileurl.Length - 3);
                }
            }
            
            string sUrl = GetformattedUrl(shapefileurl);

           

            //Provider.Load(sUrl); 
            //ChoroplethProperties_RenderMap();
        }

        #endregion
    }
}
