using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for ChoroplethShapeLayerProperties.xaml
    /// </summary>
    public partial class ChoroplethKmlLayerProperties : ChoroplethLayerPropertiesUserControlBase, ILayerProperties
    {
        private ChoroplethKmlLayerProvider _provider;
        public ChoroplethKmlLayerProvider Provider
        {
            set { _provider = value; }
            get { return _provider; }
        }

        private System.Xml.XmlElement currentElement;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

        public IDictionary<string, object> shapeAttributes;
        private bool flagrunedit;
        public IDictionary<string, object> curfeatureAttributes;
        private int Numclasses;
        public DataFilters datafilters { get; set; }

        public ChoroplethKmlLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            Provider = new ChoroplethKmlLayerProvider(myMap);
            Provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);

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

        private void rctMissingColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }

        void rctFilter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FilterRequested != null)
            {
                FilterRequested(this, new EventArgs());
            }
        }

        void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flagrunedit = false;
            if (EditRequested != null)
            {
                EditRequested(this, new EventArgs());
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
                RenderMap();
            }
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }

        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
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

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
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
            Provider.Load();
        }

        private void FillComboBoxes()
        {
            cbxDataKey.Items.Clear();
            cbxValue.Items.Clear();
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

        public void SetValues(string shapekey, string datakey, string val, string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes, Dictionary<int, object> classAttrib, bool flagquintiles, int numclasses)
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
            cbxValue.Text = val;
            grdMain.Width = 700;
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

                System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
                type.Value = "EpiDashboard.Mapping.ChoroplethKmlLayerProperties";

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

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            if (Provider == null)
            {
                Provider = new ChoroplethKmlLayerProvider(myMap);
                Provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);
            }

            base.CreateFromXml(element, Provider);

            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile") && BoundryFilePathExists(child.InnerText))
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
                        cbxDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cbxShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cbxValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("classes"))
                    {
                        cbxClasses.SelectedIndex = int.Parse(child.InnerText);
                    }
                    if (child.Name.Equals("highColor"))
                    {
                        rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                    if (child.Name.Equals("lowColor"))
                    {
                        rctLowColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
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
