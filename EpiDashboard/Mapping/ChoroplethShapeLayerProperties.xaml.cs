using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace EpiDashboard.Mapping
{
    public partial class ChoroplethShapeLayerProperties : ChoroplethLayerPropertiesUserControlBase, ILayerProperties
    {
        private ChoroplethShapeLayerProvider _provider;
        public ChoroplethShapeLayerProvider Provider
        {
            set { _provider = value; }
            get { return _provider; }
        }

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

        public IDictionary<string, object> shapeAttributes;
        private bool flagrunedit;
        private int Numclasses;

        public DataFilters datafilters { get; set; }

        public enum ColorRampType
        {
            Auto,
            Custom
        }

        private ColorRampType _colorRampType = new ColorRampType();
        public ColorRampType CurrentColorRampType
        {
            get { return _colorRampType; }
        }

        private bool _useCustomColors;
        public bool UseCustomColors
        {
            get { return _useCustomColors; }
        }

        public ChoroplethShapeLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            Provider = new ChoroplethShapeLayerProvider(myMap);

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
            #endregion; //translation
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
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
        }

        private void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested != null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
            }
        }

        private void RenderMap()
        {
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
                Provider.SetShapeRangeValues(
                    dashboardHelper, 
                    cbxShapeKey.SelectedItem.ToString(),
                    cbxDataKey.SelectedItem.ToString(),
                    cbxValue.SelectedItem.ToString(), 
                    null,
                    int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()),
                    "",
                    "");

                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
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
            RenderMap();
        }

        void btnShapeFile_Click(object sender, RoutedEventArgs e)
        {
            object[] shapeFileProperties = Provider.Load();
            
            if (shapeFileProperties != null)
            {
                if (shapeFileProperties.Length == 2)
                {
                    boundryFilePath = shapeFileProperties[0].ToString();
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    
                    if (shapeAttributes != null)
                    {
                        cbxShapeKey.Items.Clear();
                        
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void FillComboBoxes()
        {
            cbxDataKey.Items.Clear();
            cbxValue.Items.Clear();
            List<string> fields = dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
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

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            cbxDataKey.IsEnabled = false;
            cbxShapeKey.IsEnabled = false;
            cbxValue.IsEnabled = false;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }

        public void SetValues(string shapefilepath, string shapekey, string datakey, string val,
            string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes,
            Dictionary<int, object> classAttrib, bool quintilesChecked, int numclasses, string LegendText)
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
            partitionSetUsingQuantiles = quintilesChecked;
            Numclasses = numclasses;
            boundryFilePath = shapefilepath;
            rctHighColor.Fill = (SolidColorBrush)Highcolor;
            rctLowColor.Fill = (SolidColorBrush)Lowcolor;
            rctMissingColor.Fill = (SolidColorBrush)Missingcolor;
            cbxClasses.Text = classes;
            cbxShapeKey.Text = shapekey;
            cbxDataKey.Text = datakey;
            cbxValue.Text = val;
            grdMain.Width = 700;
            Provider.LegendText = LegendText;
        }

        public XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            try
            {
                string dataKey = cbxDataKey.SelectedItem.ToString();
                string shapeKey = cbxShapeKey.SelectedItem.ToString();
                string value = cbxValue.SelectedItem.ToString();
                string classCount = cbxClasses.SelectedIndex.ToString();
                SolidColorBrush highColor = (SolidColorBrush)rctHighColor.Fill;
                SolidColorBrush lowColor = (SolidColorBrush)rctLowColor.Fill;
                SolidColorBrush missingColor = (SolidColorBrush)rctMissingColor.Fill;

                string xmlString = "<shapeFile>" + boundryFilePath + "</shapeFile>" + Environment.NewLine;

                XmlAttribute type = doc.CreateAttribute("layerType");
                type.Value = "EpiDashboard.Mapping.ChoroplethShapeLayerProperties";

                return ChoroplethLayerPropertiesUserControlBase.Serialize(
                    doc,
                    Provider,
                    dataKey,
                    shapeKey,
                    value,
                    classCount,
                    highColor,
                    lowColor,
                    missingColor,
                    xmlString,
                    dashboardHelper,
                    type);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            
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
                            IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];

                            if (shapeAttributes != null)
                            {
                                cbxShapeKey.Items.Clear();
                                foreach (string key in shapeAttributes.Keys)
                                {
                                    cbxShapeKey.Items.Add(key);
                                }
                            }
                        }
                    }
                }

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
                
                if (child.Name.Equals("classes"))
                {
                    cbxClasses.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                    cbxClasses.SelectedIndex = int.Parse(child.InnerText);
                    cbxClasses.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                }
                
                if (child.Name.Equals("value"))
                {
                    cbxValue.SelectionChanged -= new SelectionChangedEventHandler(keys_SelectionChanged);
                    cbxValue.SelectedItem = child.InnerText;
                    cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
                }
                
                if (child.Name.Equals("highColor"))
                {
                    rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }

                if (child.Name.Equals("lowColor"))
                {
                    rctLowColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }

                if (child.Name.Equals("missingColor"))
                {
                    rctMissingColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
            }

            ChoroplethProperties_RenderMap();
        }

        private void ChoroplethProperties_RenderMap()
        {
            base.ChoroplethProperties_RenderMap(Provider);
            
            if (MapGenerated != null)
            {
                MapGenerated(this, new EventArgs());
            }
        }
        #endregion
    }
}
